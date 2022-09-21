using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class TriviaDiffTools
{
    private readonly Tools.DiffTools diffTools;

    public TriviaDiffTools(Tools.DiffTools diffTools)
    {
        this.diffTools = diffTools;
    }

    public List<Diff> GetTriviaChildren(CSharpSyntaxNode deleteAccessorList, CSharpSyntaxNode addAccessorList)
    {
        var children = new List<Diff>();

        if (deleteAccessorList.GetLeadingTrivia() != addAccessorList.GetLeadingTrivia())
        {
            children.AddRange(this.diffTools.GetDiffOfChildrenFusion(
                new SyntaxTriviaListDiffToolsConfig(TriviaType.Leading),
                deleteAccessorList.GetLeadingTrivia().ToList(),
                addAccessorList.GetLeadingTrivia().ToList()));
        }

        if (deleteAccessorList.GetTrailingTrivia() != addAccessorList.GetTrailingTrivia())
        {
            children.AddRange(this.diffTools.GetDiffOfChildrenFusion(
                new SyntaxTriviaListDiffToolsConfig(TriviaType.Trailing),
                deleteAccessorList.GetTrailingTrivia().ToList(),
                addAccessorList.GetTrailingTrivia().ToList()));
        }

        return children;
    }
}