using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neocra.GitMerge.Csharp;
using Neocra.GitMerge.CsProj;
using Neocra.GitMerge.Tools;
using Neocra.GitMerge.Xml;

namespace Neocra.GitMerge
{
    public class Program
    {
        public static ILoggerProvider LoggerProvider { get; set; } = null!;

        public static async Task<int> Main(string[] args)
        {
            var mergeCommand = new Command("merge")
            {
                new Option<string>("--ancestor") { IsRequired = true },
                new Option<string>("--current") { IsRequired = true },
                new Option<string>("--other") { IsRequired = true },
                new Option<string>("--path-name"),
                new Option<bool>(new []{"--verbose", "-v"}, "Display verbosity"),
            };
            mergeCommand.Handler = CommandHandler.Create<MergeOptions>(RunMergeOptions);

            var command = new RootCommand()
            {
                mergeCommand
            };
            
            return await command.InvokeAsync(args);
        }

        public static int RunMergeOptions(MergeOptions opts)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IMerger, XmlMerger>();
            services.AddSingleton<IMerger, CsharpMerger>();
            services.AddSingleton<IMerger, CsprojMerger>();
            services.AddSingleton<MergerSelecter>();
            services.AddSingleton<XmlDataAccess>();
            services.AddSingleton<DiffTools>();
            
            services.AddLogging(c =>
            {
                c.AddSystemdConsole();
                if (LoggerProvider != null)
                {
                    c.AddProvider(LoggerProvider);
                }
            }).Configure<LoggerFilterOptions>(o => o.MinLevel = opts.Verbose ? LogLevel.Debug : LogLevel.Information);

            var provider = services.BuildServiceProvider();
            try
            {
                return provider.GetService<MergerSelecter>()
                        ?.Merge(opts) switch
                    {
                        MergeStatus.Good => 0,
                        MergeStatus.Conflict => 1,
                        _ => throw new ArgumentOutOfRangeException()
                    };

            }
            catch (Exception ex)
            {
                provider.GetService<ILogger<Program>>()
                    .LogError(ex, "Error on merge");
                
                return 1;
            }
        }
    }

}