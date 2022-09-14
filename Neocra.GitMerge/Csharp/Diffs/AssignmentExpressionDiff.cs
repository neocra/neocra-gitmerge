using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp.Diffs;

internal class AssignmentExpressionDiff :  Diff<AssignmentExpressionSyntax>, IDiffChildren
{
    public AssignmentExpressionDiff(DiffMode mode, AssignmentExpressionDiffMode assignmentExpressionDiffMode, int indexOfChild, int moveIndexOfChild, AssignmentExpressionSyntax value, List<Diff>? children = null) : base(mode, indexOfChild, moveIndexOfChild, value)
    {
        this.AssignmentExpressionDiffMode = assignmentExpressionDiffMode;
        this.Children = children;
    }

    public AssignmentExpressionDiffMode AssignmentExpressionDiffMode { get; }
    public List<Diff>? Children { get; }
}

internal enum AssignmentExpressionDiffMode
{
    Left,
    Right
}