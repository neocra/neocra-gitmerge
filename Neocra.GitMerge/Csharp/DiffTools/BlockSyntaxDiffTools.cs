using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class BlockSyntaxDiffTools
{
    private readonly Tools.DiffTools diffTools;

    public BlockSyntaxDiffTools(Tools.DiffTools diffTools)
    {
        this.diffTools = diffTools;
    }

    public IEnumerable<Diff> MakeARecursive(int index, BlockSyntax? delete1, BlockSyntax? add1)
    {
        if (delete1 == null || add1 == null)
        {
            throw NotSupportedExceptions.Value((delete1, add1));
        }
            
        var children = this.diffTools.GetDiffOfChildrenFusion(
            new StatementDiffToolsConfig(this.diffTools),
            delete1.Statements.ToList(),
            add1.Statements.ToList()).ToList();

        foreach (var child in children)
        {
            yield return child;
        }
    }
}