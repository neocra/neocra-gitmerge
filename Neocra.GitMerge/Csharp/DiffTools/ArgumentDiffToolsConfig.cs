using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class ArgumentDiffToolsConfig : DiffToolsConfig<ArgumentSyntax, ArgumentDiff>
{
    public override int Distance(ArgumentDiff delete, ArgumentDiff add)
    {
        throw new NotImplementedException();
    }

    public override bool CanFusion(ArgumentDiff delete, ArgumentDiff add)
    {
        return false;
    }

    public override ArgumentDiff CreateMove(ArgumentDiff delete, ArgumentDiff add)
    {
        throw new NotImplementedException();
    }

    public override bool IsElementEquals(ArgumentSyntax a, ArgumentSyntax b)
    {
        return a.ToString() == b.ToString();
    }
        
    public override Diff? MakeARecursive(ArgumentDiff delete, ArgumentDiff add)
    {
        throw new NotImplementedException();
    }

    public override ArgumentDiff CreateDiff(DiffMode mode, List<ArgumentSyntax> elements, int index)
    {
        return new ArgumentDiff(mode, index, 0, elements[index]);
    }
}