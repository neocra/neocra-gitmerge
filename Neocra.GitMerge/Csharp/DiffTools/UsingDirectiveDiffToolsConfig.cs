using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools
{
    public class UsingDirectiveDiffToolsConfig : DiffToolsConfig<UsingDirectiveSyntax, UsingDirectiveDiff>
    {
        public override bool CanFusion(UsingDirectiveDiff delete, UsingDirectiveDiff add)
        {
            return false;
        }
        
        public override bool IsElementEquals(UsingDirectiveSyntax a, UsingDirectiveSyntax b)
        {
            return a.ToString() == b.ToString();
        }
        
        public override UsingDirectiveDiff CreateDiff(DiffMode mode, List<UsingDirectiveSyntax> elements, int index)
        {
            return new UsingDirectiveDiff(mode, index, 0, elements[index]);
        }
    }
}