using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{
    public class ExpressionDiff : Diff<ExpressionSyntax>, IDiffChildren
    {
        public ExpressionDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, ExpressionSyntax value, List<Diff>? children = null) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
            this.Children = children;
        }

        public List<Diff>? Children { get; }
    }
}