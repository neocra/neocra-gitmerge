using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{
    public class VariableDeclaratorDiff : Diff<VariableDeclaratorSyntax>, IDiffChildren
    {
        public VariableDeclaratorDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, VariableDeclaratorSyntax value, List<Diff>? children = null) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
            this.Children = children ?? new List<Diff>();
        }

        public List<Diff> Children { get; }
    }
}