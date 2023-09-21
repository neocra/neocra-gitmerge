using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Neocra.GitMerge.Csharp;
using Neocra.GitMerge.CsProj;
using Neocra.GitMerge.Infrastructure;
using Neocra.GitMerge.Tools;
using Neocra.GitMerge.Xml;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Spectre;
using Spectre.Console.Cli;

namespace Neocra.GitMerge;

public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var services = GetServiceCollection();
            var registrar = new TypeRegistrar(services);
            
            var app = new CommandApp(registrar);

            app.Configure(config =>
            {
                config.SetInterceptor(new LogInterceptor());
                config.AddCommand<InstallCommand>("install");
                config.AddCommand<UnInstallCommand>("uninstall");
                config.AddCommand<MergeCommand>("merge");
            });

            return await app.RunAsync(args);
        }

        private static ServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IMerger, XmlMerger>();
            services.AddSingleton<IMerger, CsharpMerger>();
            services.AddSingleton<CsharpApply>();
            services.AddSingleton<IMerger, CsprojMerger>();
            services.AddSingleton<MergerSelecter>();
            services.AddSingleton<XmlDataAccess>();
            services.AddSingleton<DiffTools>();


            services.AddLogging(configure =>
                configure.AddSerilog(new LoggerConfiguration()
                    // log level will be dynamically be controlled by our log interceptor upon running
                    .MinimumLevel.ControlledBy(LogInterceptor.LogLevel)
                    .WriteTo.Spectre()
                    .CreateLogger()
                ));

            return services;
        }
    }

public sealed class VerbosityConverter : TypeConverter
    {
        private readonly Dictionary<string, LogEventLevel> lookup = new(StringComparer.OrdinalIgnoreCase)
        {
            {"d", LogEventLevel.Debug},
            {"v", LogEventLevel.Verbose},
            {"i", LogEventLevel.Information},
            {"w", LogEventLevel.Warning},
            {"e", LogEventLevel.Error},
            {"f", LogEventLevel.Fatal}
        };

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                var result = this.lookup.TryGetValue(stringValue, out var verbosity);
                if (!result)
                {
                    const string format = "The value '{0}' is not a valid verbosity.";
                    var message = string.Format(CultureInfo.InvariantCulture, format, value);
                    throw new InvalidOperationException(message);
                }
                return verbosity;
            }
            throw new NotSupportedException("Can't convert value to verbosity.");
        }
    }