using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace Neocra.GitMerge;

public class UnInstallCommand : AsyncCommand<UnInstallSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, UnInstallSettings settings)
    {
        throw new NotImplementedException();
    }
}