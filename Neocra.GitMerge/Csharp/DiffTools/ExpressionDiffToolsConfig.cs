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
        private readonly TriviaDiffTools triviaDiffTools;

        public ExpressionDiffToolsConfig(Tools.DiffTools diffTools)
        {
            this.diffTools = diffTools;
            this.triviaDiffTools = new TriviaDiffTools(diffTools);
        }

        public IEnumerable<Diff> MakeARecursive(int index, ExpressionSyntax? delete1, ExpressionSyntax? add1, SyntaxTriviaList syntaxTriviaList)
        {
            return (delete1, add1) switch
            {
                (BinaryExpressionSyntax delete, BinaryExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (LiteralExpressionSyntax delete, LiteralExpressionSyntax add) => MakeARecursive(delete, add),
                (PrefixUnaryExpressionSyntax delete, PrefixUnaryExpressionSyntax add) => MakeARecursive(delete, add),
                (ParenthesizedExpressionSyntax delete, ParenthesizedExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (ParenthesizedLambdaExpressionSyntax delete, ParenthesizedLambdaExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (InvocationExpressionSyntax delete, InvocationExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (MemberAccessExpressionSyntax delete, MemberAccessExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (AssignmentExpressionSyntax delete, AssignmentExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (IdentifierNameSyntax delete, IdentifierNameSyntax add) => MakeARecursive(index, delete, add),
                (ObjectCreationExpressionSyntax delete, ObjectCreationExpressionSyntax add) => C(MakeARecursive, index, delete, add),
                (null, ExpressionSyntax d) => AddExpression(index, d, syntaxTriviaList),
                (ExpressionSyntax delete, ExpressionSyntax add) => UpdateExpression(index, delete, add, syntaxTriviaList),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IEnumerable<Diff> MakeARecursive(int index, ParenthesizedLambdaExpressionSyntax delete, ParenthesizedLambdaExpressionSyntax add)
        {
            if (delete.Block?.ToFullString() != add.Block?.ToFullString())
            {
                throw NotSupportedExceptions.Value((delete, add));
            }
            
            if (delete.Modifiers.ToFullString() != add.Modifiers.ToFullString())
            {
                throw NotSupportedExceptions.Value((delete, add));
            }
            
            if (delete.ParameterList.ToFullString() != add.ParameterList.ToFullString())
            {
                throw NotSupportedExceptions.Value((delete, add));
            }

            if (delete.ExpressionBody?.ToFullString() != add.ExpressionBody?.ToFullString())
            {
                if (delete.ExpressionBody == null || add.ExpressionBody == null)
                {
                    throw NotSupportedExceptions.Value((delete.ExpressionBody, add.ExpressionBody));
                }
                
                yield return new ExpressionBodyDiff(DiffMode.Update, index, 0, delete.ExpressionBody, 
                    MakeARecursive(index, delete.ExpressionBody, add.ExpressionBody, SyntaxTriviaList.Empty).ToList());
            }
        }

        private IEnumerable<Diff> MakeARecursive(int index, ObjectCreationExpressionSyntax delete1, ObjectCreationExpressionSyntax add1)
        {
            if (delete1.ArgumentList?.ToString() != add1.ArgumentList?.ToString())
            {
                if (delete1.ArgumentList == null || add1.ArgumentList == null)
                {
                    throw NotSupportedExceptions.Value((delete1, add1));
                }
                
                foreach (var diff in this.diffTools.GetDiffOfChildrenFusion(new ArgumentDiffToolsConfig(),
                             delete1.ArgumentList.Arguments.ToList(),
                             add1.ArgumentList.Arguments.ToList()))
                    yield return diff;
            }
        }

        private IEnumerable<Diff> MakeARecursive(int index, IdentifierNameSyntax delete1, IdentifierNameSyntax add1)
        {
            throw NotSupportedExceptions.Value((delete1, add1));
        }

        private IEnumerable<Diff> C<T>(Func<int, T, T, IEnumerable<Diff>> makeARecursive, int index, T delete, T add)
            where T : ExpressionSyntax
        {
            var children = new List<Diff>();
            
            children.AddRange(makeARecursive(index, delete, add));
            
            if (children.Any())
            {
                yield return new ExpressionDiff(DiffMode.Update, index, 0, add, children);
                yield break;
            }

            throw NotSupportedExceptions.Value((delete, add));
        }

        private IEnumerable<Diff> MakeARecursive(int index, AssignmentExpressionSyntax delete1, AssignmentExpressionSyntax add1)
        {
            if (delete1.Left.ToString() != add1.Left.ToString())
            {
               yield return new AssignmentExpressionDiff(DiffMode.Update, AssignmentExpressionDiffMode.Left, index, 0, delete1, MakeARecursive(index, delete1.Left, add1.Left, SyntaxTriviaList.Empty).ToList());
            }

            if (delete1.Right.ToString() != add1.Right.ToString())
            {
                yield return new AssignmentExpressionDiff(DiffMode.Update,
                    AssignmentExpressionDiffMode.Right,
                    index,
                    0,
                    delete1,
                    MakeARecursive(index, delete1.Right, add1.Right, SyntaxTriviaList.Empty).ToList());
            }
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
            if (delete1.Expression.ToString() != add1.Expression.ToString())
            {
                throw new NotImplementedException();
            }

            if (delete1.Name.ToString() != add1.Name.ToString())
            {
                yield return new NameMemberAccessExpressionDiff(DiffMode.Update, index, 0, add1.Name);
            }
        }
        
        private IEnumerable<Diff> MakeARecursive(int index, InvocationExpressionSyntax delete1, InvocationExpressionSyntax add1)
        {
            foreach (var diff in this.triviaDiffTools.GetTriviaChildren(delete1, add1))
            {
                yield return diff;
            }

            if (delete1.Expression.ToString() != add1.Expression.ToString())
            {
                throw new NotImplementedException();
            }

            if (delete1.ArgumentList.ToString() != add1.ArgumentList.ToString())
            {
                foreach (var diff in this.diffTools.GetDiffOfChildrenFusion(new ArgumentDiffToolsConfig(),
                             delete1.ArgumentList.Arguments.ToList(),
                             add1.ArgumentList.Arguments.ToList()))
                    yield return diff;
            }
        }

        private IEnumerable<Diff> AddExpression(int index, ExpressionSyntax expressionSyntax, SyntaxTriviaList leadingTrivia)
        {
            var withLeadingTrivia = expressionSyntax.WithLeadingTrivia(leadingTrivia);
            yield return new ExpressionDiff(DiffMode.Add, index, 0, withLeadingTrivia);
        }

        private IEnumerable<Diff> MakeARecursive(int index, ParenthesizedExpressionSyntax delete1, ParenthesizedExpressionSyntax add1)
        {
            foreach (var diff in this.MakeARecursive(index, delete1.Expression, add1.Expression, SyntaxTriviaList.Empty))
            {
                yield return diff;
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
            if (delete1.OperatorToken.ToString() != add1.OperatorToken.ToString())
            {
                throw new NotImplementedException();
            }

            foreach (var diff in this.MakeARecursive(0, delete1.Left, add1.Left, SyntaxTriviaList.Empty))
            {
                yield return diff;
            }

            foreach (var diff in this.MakeARecursive(1, delete1.Right, add1.Right, SyntaxTriviaList.Empty))
            {
                yield return diff;
            }
        }
    }
}