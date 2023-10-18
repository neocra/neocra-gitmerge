using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class NameMemberAccessExpressionDiff : Diff<NameSyntax>
{
    public NameMemberAccessExpressionDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, NameSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
    {
    }
}