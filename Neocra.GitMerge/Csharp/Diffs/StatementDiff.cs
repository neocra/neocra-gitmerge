using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs
{
    public class StatementDiff : Diff<StatementSyntax>, IDiffChildren
    {
        public List<Diff>? Children { get; }

        public StatementDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, StatementSyntax statementSyntax, List<Diff>? children = null) : base(mode, indexOfChild, moveIndexOfChild, statementSyntax)
        {
            this.Children = children;
        }
    }
}