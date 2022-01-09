using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools
{
    public class ExpressionDiffToolsConfig
    {
        private readonly Tools.DiffTools diffTools;

        public ExpressionDiffToolsConfig(Tools.DiffTools diffTools)
        {
            this.diffTools = diffTools;
        }

        public IEnumerable<Diff> MakeARecursive(int index, ExpressionSyntax? delete1, ExpressionSyntax? add1, SyntaxTriviaList syntaxTriviaList)
        {
            return (delete1, add1) switch
            {
                (BinaryExpressionSyntax delete, BinaryExpressionSyntax add) => this.MakeARecursive(index, delete, add),
                (LiteralExpressionSyntax delete, LiteralExpressionSyntax add) => MakeARecursive(delete, add),
                (PrefixUnaryExpressionSyntax delete, PrefixUnaryExpressionSyntax add) => MakeARecursive(delete, add),
                (ParenthesizedExpressionSyntax delete, ParenthesizedExpressionSyntax add) => this.MakeARecursive(index, delete, add),
                (InvocationExpressionSyntax delete, InvocationExpressionSyntax add) => MakeARecursive(index, delete, add),
                (MemberAccessExpressionSyntax delete, MemberAccessExpressionSyntax add) => MakeARecursive(index, delete, add),
                (null, ExpressionSyntax d) => AddExpression(index, d, syntaxTriviaList),
                (ExpressionSyntax delete,ExpressionSyntax add) => UpdateExpression(index, delete, add, syntaxTriviaList),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IEnumerable<Diff> UpdateExpression(int index, ExpressionSyntax delete, ExpressionSyntax add, SyntaxTriviaList syntaxTriviaList)
        {
            if (delete.GetType() != add.GetType())
            {
                yield return new ExpressionDiff(DiffMode.Update, index, 0, add);
                yield break;
            }

            throw NotSupportedExceptions.Value((delete, add));
        }

        private IEnumerable<Diff> MakeARecursive(int index, MemberAccessExpressionSyntax delete1, MemberAccessExpressionSyntax add1)
        {
            var children = new List<Diff>();
            if (delete1.Expression.ToString() != add1.Expression.ToString())
            {
                throw new NotImplementedException();
            }

            if (delete1.Name.ToString() != add1.Name.ToString())
            {
                children.Add(new NameMemberAccessExpressionDiff(DiffMode.Update, index, 0, add1.Name));
            }

            if (children.Any())
            {
                yield return new ExpressionDiff(DiffMode.Update, index, 0, delete1, children);
            }
        }
        
        private IEnumerable<Diff> MakeARecursive(int index, InvocationExpressionSyntax delete1, InvocationExpressionSyntax add1)
        {
            var children = new List<Diff>();
            if (delete1.Expression.ToString() != add1.Expression.ToString())
            {
                throw new NotImplementedException();
            }

            if (delete1.ArgumentList.ToString() != add1.ArgumentList.ToString())
            {
                children.AddRange(this.diffTools.GetDiffOfChildrenFusion(new ArgumentDiffToolsConfig(),
                    delete1.ArgumentList.Arguments.ToList(),
                    add1.ArgumentList.Arguments.ToList()));
            }

            if (children.Any())
            {
                yield return new ExpressionDiff(DiffMode.Update, index, 0, delete1, children);
            }
        }

        private IEnumerable<Diff> AddExpression(int index, ExpressionSyntax expressionSyntax, SyntaxTriviaList leadingTrivia)
        {
            var withLeadingTrivia = expressionSyntax.WithLeadingTrivia(leadingTrivia);
            yield return new ExpressionDiff(DiffMode.Add, index, 0, withLeadingTrivia);
        }

        private IEnumerable<Diff> MakeARecursive(int index, ParenthesizedExpressionSyntax delete1, ParenthesizedExpressionSyntax add1)
        {
            var children = new List<Diff>();

            children.AddRange(this.MakeARecursive(index, delete1.Expression, add1.Expression, SyntaxTriviaList.Empty));

            if (children.Any())
            {
                yield return new ExpressionDiff(DiffMode.Update, index, 0, delete1, children);
            }
        }

        private IEnumerable<Diff> MakeARecursive(PrefixUnaryExpressionSyntax delete1, PrefixUnaryExpressionSyntax add1)
        {
            if (delete1.Operand.ToString() != add1.Operand.ToString())
            {
                throw new NotImplementedException();
            }

            yield break;
        }

        private IEnumerable<Diff> MakeARecursive(LiteralExpressionSyntax delete1, LiteralExpressionSyntax add1)
        {
            if (delete1.Token.ToString() != add1.Token.ToString())
            {
                yield return new ExpressionDiff(DiffMode.Update, 0, 0, add1);
            }
        }

        private IEnumerable<Diff> MakeARecursive(int index, BinaryExpressionSyntax delete1, BinaryExpressionSyntax add1)
        {
            var children = new List<Diff>();
            if (delete1.OperatorToken.ToString() != add1.OperatorToken.ToString())
            {
                throw new NotImplementedException();
            }

            children.AddRange(this.MakeARecursive(0, delete1.Left, add1.Left, SyntaxTriviaList.Empty));
            children.AddRange(this.MakeARecursive(1, delete1.Right, add1.Right, SyntaxTriviaList.Empty));

            if (children.Any())
            {
                yield return new ExpressionDiff(DiffMode.Update, index, 0, delete1, children);
            }
        }
    }

    public class NameMemberAccessExpressionDiff : Diff<SimpleNameSyntax>
    {
        public NameMemberAccessExpressionDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, SimpleNameSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
        }
    }

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

    public class ArgumentDiff : Diff<ArgumentSyntax>
    {
        public ArgumentDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, ArgumentSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
        }
    }
}