using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class ArgumentDiff : Diff<ArgumentSyntax>
{
    public ArgumentDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, ArgumentSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
    {
    }

    protected override string GetName()
    {
        return Value.Expression.ToFullString();
    }
}