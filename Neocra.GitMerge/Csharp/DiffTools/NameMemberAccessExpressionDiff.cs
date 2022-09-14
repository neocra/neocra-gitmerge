using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class NameMemberAccessExpressionDiff : Diff<SimpleNameSyntax>
{
    public NameMemberAccessExpressionDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, SimpleNameSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
    {
    }
}