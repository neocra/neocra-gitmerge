using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{
    public class VariableDeclarationTypeDiff : Diff
    {
        public TypeSyntax DeclarationType { get; }

        public VariableDeclarationTypeDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, TypeSyntax declarationType) : base(mode, indexOfChild, moveIndexOfChild)
        {
            this.DeclarationType = declarationType;
        }
    }
    
    public class VariableDeclarationDiff : Diff, IDiffChildren
    {
        public VariableDeclarationDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, List<Diff> children) : base(mode, indexOfChild, moveIndexOfChild)
        {
            this.Children = children;
        }

        public List<Diff> Children { get; }
    }
}