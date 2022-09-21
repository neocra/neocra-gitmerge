using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class ArgumentDiffToolsConfig : DiffToolsConfig<ArgumentSyntax, ArgumentDiff>
{
    public override bool CanFusion(ArgumentDiff delete, ArgumentDiff add)
    {
        return false;
    }
    
    public override bool IsElementEquals(ArgumentSyntax a, ArgumentSyntax b)
    {
        return a.ToFullString() == b.ToFullString();
    }
    
    public override ArgumentDiff CreateDiff(DiffMode mode, List<ArgumentSyntax> elements, int index)
    {
        return new ArgumentDiff(mode, index, 0, elements[index]);
    }
}