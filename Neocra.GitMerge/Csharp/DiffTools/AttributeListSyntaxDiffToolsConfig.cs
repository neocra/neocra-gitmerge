using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools
{
    public class AttributeListSyntaxDiffToolsConfig : DiffToolsConfig<AttributeListSyntax, AttributeListDiff>
    {
        public override int Distance(AttributeListDiff delete, AttributeListDiff add)
        {
            throw new NotImplementedException();
        }

        public override bool CanFusion(AttributeListDiff delete, AttributeListDiff add)
        {
            throw new NotImplementedException();
        }

        public override AttributeListDiff CreateMove(AttributeListDiff delete, AttributeListDiff add)
        {
            throw new NotImplementedException();
        }

        public override bool IsElementEquals(AttributeListSyntax a, AttributeListSyntax b)
        {
            throw new NotImplementedException();
        }
        
        public override Diff? MakeARecursive(AttributeListDiff delete, AttributeListDiff add)
        {
            throw new NotImplementedException();
        }

        public override AttributeListDiff CreateDiff(DiffMode mode, List<AttributeListSyntax> elements, int index)
        {
            throw new NotImplementedException();
        }
    }
}