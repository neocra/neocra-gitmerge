using System.ComponentModel.DataAnnotations;
using Neocra.GitMerge.Infrastructure;
using Spectre.Console.Cli;

namespace Neocra.GitMerge;

public class MergeSettings : LogCommandSettings
{
    [CommandOption("--ancestor")]
    public string Ancestor { get; set; } = null!;

    [CommandOption("--current")]
    public string Current { get; set; } = null!;

    [CommandOption("--other")]
    public string Other { get; set; } = null!;

    [CommandOption("--path-name")]
    public string? PathName { get; set; }
        
    public bool Verbose { get; set; }
}