using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{
    public class MethodReturnTypeDiff : Diff<TypeSyntax>
    {
        public MethodReturnTypeDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, TypeSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
        }
    }
}