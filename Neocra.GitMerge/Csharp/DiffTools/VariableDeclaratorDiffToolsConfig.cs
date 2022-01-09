using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools
{
    public class VariableDeclaratorDiffToolsConfig : DiffToolsConfig<VariableDeclaratorSyntax, VariableDeclaratorDiff>
    {
        private readonly ExpressionDiffToolsConfig expressionDiffToolsConfig;

        public VariableDeclaratorDiffToolsConfig(Tools.DiffTools diffTools)
        {
            this.expressionDiffToolsConfig = new ExpressionDiffToolsConfig(diffTools);
        }

        public override int Distance(VariableDeclaratorDiff delete, VariableDeclaratorDiff add)
        {
            return StringTools.Compute(delete.ToString(), add.ToString());
        }

        public override bool CanFusion(VariableDeclaratorDiff delete, VariableDeclaratorDiff add)
        {
            return true;
        }

        public override VariableDeclaratorDiff CreateMove(VariableDeclaratorDiff delete, VariableDeclaratorDiff add)
        {
            throw new NotImplementedException();
        }

        public override bool IsElementEquals(VariableDeclaratorSyntax a, VariableDeclaratorSyntax b)
        {
            return a.ToString() == b.ToString();
        }

        public override Diff MakeARecursive(VariableDeclaratorDiff delete, VariableDeclaratorDiff add)
        {
            var children = new List<Diff>();
            if (delete.Value.Identifier.ToString() != add.Value.Identifier.ToString())
            {
                children.Add(new IdentifierDiff(DiffMode.Update, delete.IndexOfChild, 0, add.Value.Identifier));
            }

            if (delete.Value.Initializer != null && add.Value.Initializer != null)
            {
                if (delete.Value.Initializer.ToString() != add.Value.Initializer.ToString())
                {
                    children.AddRange(MakeARecursive(delete.Value.Initializer, add.Value.Initializer));
                }
            }
            
            if (delete.Value.ArgumentList?.ToString() != add.Value.ArgumentList?.ToString())
            {
                throw new NotImplementedException();
            }

            if (children.Any())
            {
                return new VariableDeclaratorDiff(DiffMode.Update, delete.IndexOfChild, 0, delete.Value, children);
            }

            throw new NotSupportedException();
        }

        public override VariableDeclaratorDiff CreateDiff(DiffMode mode, List<VariableDeclaratorSyntax> elements, int index)
        {
            return new VariableDeclaratorDiff(mode, index, 0, elements[index]);
        }
        
        private IEnumerable<Diff> MakeARecursive(EqualsValueClauseSyntax delete, EqualsValueClauseSyntax add)
        {
            if (delete.EqualsToken.ToString() != add.EqualsToken.ToString())
            {
                throw new NotImplementedException();
            }
            
            if (delete.Value.ToString() != add.Value.ToString())
            {
                return expressionDiffToolsConfig.MakeARecursive(0, delete.Value, add.Value, SyntaxTriviaList.Empty);
            }

            throw new NotImplementedException();
        }
    }

    public class IdentifierDiff : Diff<SyntaxToken>
    {
        public IdentifierDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, SyntaxToken value) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
        }
    }
}