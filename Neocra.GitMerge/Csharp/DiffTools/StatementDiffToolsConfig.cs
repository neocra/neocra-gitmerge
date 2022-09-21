using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools
{

    public class StatementDiffToolsConfig : DiffToolsConfig<StatementSyntax,StatementDiff>
    {
        private readonly Tools.DiffTools diffTools;
        private readonly BlockSyntaxDiffTools blockSyntaxDiffTools;
        private readonly ExpressionDiffToolsConfig expressionDiffToolsConfig;

        public StatementDiffToolsConfig(Tools.DiffTools diffTools)
        {
            this.diffTools = diffTools;
            this.blockSyntaxDiffTools = new BlockSyntaxDiffTools(diffTools);
            this.expressionDiffToolsConfig = new ExpressionDiffToolsConfig(diffTools);
        }

        public override int Distance(StatementDiff delete, StatementDiff add)
        {
            return (delete.Value, add.Value) switch
            {
                (LocalDeclarationStatementSyntax d, LocalDeclarationStatementSyntax a) => StringTools.Compute(d.ToString(), a.ToString()),
                (IfStatementSyntax d, IfStatementSyntax a) => StringTools.Compute(d.ToString(), a.ToString()),
                (ExpressionStatementSyntax d, ExpressionStatementSyntax a) => Distance(d, a),
                (ReturnStatementSyntax d, ReturnStatementSyntax a) => StringTools.Compute(d.ToString(), a.ToString()),
                (BlockSyntax d, BlockSyntax a) => StringTools.Compute(d.ToString(), a.ToString()),
                var v => throw NotSupportedExceptions.Value(v)
            };
        }

        private static int Distance(ExpressionStatementSyntax delete, ExpressionStatementSyntax add)
        {
            return (delete.Expression, add.Expression) switch
            {
                (AssignmentExpressionSyntax d, AssignmentExpressionSyntax a) => d.Left.ToString() == a.Left.ToString() ? 0 : StringTools.Compute(d.ToString(), a.ToString()),
                (InvocationExpressionSyntax d, InvocationExpressionSyntax a) => StringTools.Compute(d.ToString(), a.ToString()),
                var v => throw NotSupportedExceptions.Value(v)
            };
        }

        public override bool CanFusion(StatementDiff delete, StatementDiff add)
        {
            return delete.Value.GetType() == add.Value.GetType();
        }
        
        public override Diff? MakeARecursive(StatementDiff delete, StatementDiff add)
        {
            return this.MakeARecursive(delete.IndexOfChild, delete.Value, add.Value);
        }

        private Diff? MakeARecursive(int index, StatementSyntax delete, StatementSyntax add)
        {
            var children = ((delete, add) switch
            {
                (LocalDeclarationStatementSyntax delete1, LocalDeclarationStatementSyntax add1) => this.MakeARecursiveOnLocalDeclaration(index, delete1, add1),
                (IfStatementSyntax delete1, IfStatementSyntax add1) => this.MakeARecursive(index, delete1, add1),
                (ReturnStatementSyntax delete1, ReturnStatementSyntax add1) => this.MakeARecursive(index, delete1, add1),
                (BlockSyntax delete1, BlockSyntax add1) => this.blockSyntaxDiffTools.MakeARecursive(index, delete1, add1),
                (ExpressionStatementSyntax delete1, ExpressionStatementSyntax add1) => this.MakeARecursive(index, delete1, add1),
                var v => throw NotSupportedExceptions.Value(v)
            }).ToList();

            if (children.Any())
            {
                return new StatementDiff(DiffMode.Update, index, 0, delete, children);
            }

            return null;
        }

        private IEnumerable<Diff> MakeARecursive(int index, ExpressionStatementSyntax delete1, ExpressionStatementSyntax add1)
        {
            foreach (var d in expressionDiffToolsConfig.MakeARecursive(index, delete1.Expression, add1.Expression, SyntaxTriviaList.Empty)) yield return d;
        }

        private IEnumerable<Diff> MakeARecursive(int index, ReturnStatementSyntax delete1, ReturnStatementSyntax add1)
        {
            foreach (var d in this.diffTools.GetDiffOfChildrenFusion(
                new AttributeListSyntaxDiffToolsConfig(),
                delete1.AttributeLists.ToList(),
                add1.AttributeLists.ToList())) yield return d;

            foreach (var d in expressionDiffToolsConfig.MakeARecursive(index, delete1.Expression, add1.Expression, add1.ReturnKeyword.TrailingTrivia)) yield return d;
        }

        private IEnumerable<Diff> MakeARecursive(int index, IfStatementSyntax delete1, IfStatementSyntax add1)
        {
            if (delete1.Condition.ToFullString() != add1.Condition.ToFullString())
            {
                foreach (var d in expressionDiffToolsConfig.MakeARecursive(index, delete1.Condition,
                             add1.Condition,
                             SyntaxTriviaList.Empty)) yield return d;
            }

            foreach (var d in this.diffTools.GetDiffOfChildrenFusion(
                new AttributeListSyntaxDiffToolsConfig(),
                delete1.AttributeLists.ToList(),
                add1.AttributeLists.ToList())) yield return d;

            var makeARecursive = this.MakeARecursive(index, delete1.Statement, add1.Statement);
            if (makeARecursive != null)
            {
                yield return makeARecursive;
            }
        }

        private IEnumerable<Diff> MakeARecursiveOnLocalDeclaration(int index, LocalDeclarationStatementSyntax delete1, LocalDeclarationStatementSyntax add1)
        {
            var children = this.diffTools.GetDiffOfChildrenFusion(
                new VariableDeclaratorDiffToolsConfig(this.diffTools),
                delete1.Declaration.Variables.ToList(),
                add1.Declaration.Variables.ToList())
                .ToList();

            if (delete1.Declaration.Type.ToString() != add1.Declaration.Type.ToString())
            {
                children.Add(new VariableDeclarationTypeDiff(DiffMode.Update, index, 0, add1.Declaration.Type));
            }

            if (children.Any())
            {
                yield return new VariableDeclarationDiff(DiffMode.Update, index, 0, children);
            }
        }

        public override StatementDiff CreateMove(StatementDiff delete, StatementDiff add)
        {
            var children = (delete.Children ?? new List<Diff>())
                .Union(add.Children ?? new List<Diff>())
                .ToList();
            
            return new StatementDiff(DiffMode.Move, delete.IndexOfChild, add.IndexOfChild, delete.Value, children);

        }

        public override bool IsElementEquals(StatementSyntax a, StatementSyntax b)
        {
            return a.ToString() == b.ToString();
        }

        public override StatementDiff CreateDiff(DiffMode mode, List<StatementSyntax> elements, int index)
        {
            return new StatementDiff(mode, index, 0, elements[index]);
        }
    }
}