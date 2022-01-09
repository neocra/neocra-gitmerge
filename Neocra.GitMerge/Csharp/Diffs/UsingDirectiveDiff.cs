using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{
    public class UsingDirectiveDiff : Diff<UsingDirectiveSyntax>
    {
        public UsingDirectiveDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, UsingDirectiveSyntax value) 
            : base(mode, indexOfChild, moveIndexOfChild, value)
        {
        }
    }
}