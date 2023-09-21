using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Neocra.GitMerge;

public class MergeCommand : AsyncCommand<MergeSettings>
{
    private readonly MergerSelecter mergerSelector;
    private readonly ILogger<MergeCommand> logger;

    public MergeCommand(MergerSelecter mergerSelector, ILogger<MergeCommand> logger)
    {
        this.mergerSelector = mergerSelector;
        this.logger = logger;
    }

    public override Task<int> ExecuteAsync(CommandContext context, MergeSettings settings)
    {
        try
        {
            return Task.FromResult(this.mergerSelector.Merge(settings) switch
            {
                MergeStatus.Good => 0,
                MergeStatus.Conflict => 1,
                _ => throw new ArgumentOutOfRangeException()
            });

        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error on merge");
                
            return Task.FromResult(1);
        }
    }
}