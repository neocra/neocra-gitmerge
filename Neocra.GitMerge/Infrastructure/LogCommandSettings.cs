using System.ComponentModel;
using Serilog.Events;
using Spectre.Console.Cli;

namespace Neocra.GitMerge.Infrastructure;

public class LogCommandSettings : CommandSettings
{
    [CommandOption("--logLevel")]
    [Description("Minimum level for logging")]
    [TypeConverter(typeof(VerbosityConverter))]
    [DefaultValue(LogEventLevel.Information)]
    public LogEventLevel LogLevel { get; set; }
}