using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools
{
    public class MemberDeclarationDiffToolsConfig : DiffToolsConfig<MemberDeclarationSyntax, MemberDeclarationDiff>
    {
        private readonly Tools.DiffTools diffTools;
        private readonly ExpressionDiffToolsConfig expressionDiffToolsConfig;
        private readonly BlockSyntaxDiffTools blockSyntaxDiffTools;
        private readonly TriviaDiffTools triviaDiffTools;

        public MemberDeclarationDiffToolsConfig(Tools.DiffTools diffTools)
        {
            this.diffTools = diffTools;
            this.blockSyntaxDiffTools = new BlockSyntaxDiffTools(diffTools);
            this.expressionDiffToolsConfig = new ExpressionDiffToolsConfig(diffTools);
            this.triviaDiffTools = new TriviaDiffTools(diffTools);
        }

        public override int Distance(MemberDeclarationDiff delete, MemberDeclarationDiff add)
        {
            return (delete.Value, add.Value) switch
            {
                (ClassDeclarationSyntax delete1, ClassDeclarationSyntax add1) => StringTools.Compute(delete1.ToString(), add1.ToString()),
                (NamespaceDeclarationSyntax delete1, NamespaceDeclarationSyntax add1) => StringTools.Compute(delete1.ToString(), add1.ToString()),
                (MethodDeclarationSyntax delete1, MethodDeclarationSyntax add1) => StringTools.Compute(delete1.ToString(), add1.ToString()),
                (PropertyDeclarationSyntax delete1, PropertyDeclarationSyntax add1) => StringTools.Compute(delete1.ToString(), add1.ToString()),
                (ConstructorDeclarationSyntax delete1, ConstructorDeclarationSyntax add1) => StringTools.Compute(delete1.ToString(), add1.ToString()),
                var v => throw NotSupportedExceptions.Value(v)
            };
        }

        public override bool CanFusion(MemberDeclarationDiff delete, MemberDeclarationDiff add)
        {
            return (delete.Value, add.Value) switch
            {
                (ClassDeclarationSyntax, ClassDeclarationSyntax) => true,
                (NamespaceDeclarationSyntax, NamespaceDeclarationSyntax) => true,
                (MethodDeclarationSyntax, MethodDeclarationSyntax) => true,
                (MethodDeclarationSyntax, _) => false,
                (_, MethodDeclarationSyntax) => false,
                (PropertyDeclarationSyntax, PropertyDeclarationSyntax) => true,
                (ConstructorDeclarationSyntax, ConstructorDeclarationSyntax) => true,
                (ConstructorDeclarationSyntax, _) => false,
                (_, ConstructorDeclarationSyntax) => false,
                (FieldDeclarationSyntax, FieldDeclarationSyntax) => false,
                (FieldDeclarationSyntax, _) => false,
                (_, FieldDeclarationSyntax) => false,
                var v => throw NotSupportedExceptions.Value(v)
            };
        }

        public override MemberDeclarationDiff CreateMove(MemberDeclarationDiff delete, MemberDeclarationDiff add)
        {
            var children = (delete.Children ?? new List<Diff>())
                .Union(add.Children ?? new List<Diff>())
                .ToList();

            return new MemberDeclarationDiff(DiffMode.Move, delete.IndexOfChild, add.IndexOfChild, delete.Value, children);
        }

        public override bool IsElementEquals(MemberDeclarationSyntax a, MemberDeclarationSyntax b)
        {
            return a.ToFullString() == b.ToFullString();
        }

        public override Diff? MakeARecursive(MemberDeclarationDiff delete, MemberDeclarationDiff add)
        {
            var children = ((delete.Value, add.Value) switch
            {
                (ClassDeclarationSyntax delete1, ClassDeclarationSyntax add1) => this.MakeARecursive(delete.IndexOfChild, delete1, add1),
                (NamespaceDeclarationSyntax delete1, NamespaceDeclarationSyntax add1) => this.MakeARecursive(delete.IndexOfChild, delete1, add1),
                (MethodDeclarationSyntax delete1, MethodDeclarationSyntax add1) => this.MakeARecursive(delete.IndexOfChild, delete1, add1),
                (PropertyDeclarationSyntax delete1, PropertyDeclarationSyntax add1) => this.MakeARecursive(delete.IndexOfChild, delete1, add1),
                (ConstructorDeclarationSyntax delete1, ConstructorDeclarationSyntax add1) => this.MakeARecursive(delete.IndexOfChild, delete1, add1),
                var v => throw NotSupportedExceptions.Value(v)
            }).ToList();

            if (children.Any())
            {
                return new MemberDeclarationDiff(DiffMode.Update, delete.IndexOfChild, 0, delete.Value, children);
            }

            return null;
        }

        private IEnumerable<Diff> MakeARecursive(int index, ConstructorDeclarationSyntax delete, ConstructorDeclarationSyntax add)
        {
            var children = this.triviaDiffTools.GetTriviaChildren(delete, add);

            if (delete.Identifier.ToString() != add.Identifier.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.Body?.ToString() != add.Body?.ToString())
            {
                children.AddRange(this.blockSyntaxDiffTools.MakeARecursive(index, delete.Body, add.Body));
            }

            if (delete.Initializer?.ToString() != add.Initializer?.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.Modifiers.ToString() != add.Modifiers.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.AttributeLists.ToString() != add.AttributeLists.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.ExpressionBody?.ToString() != add.ExpressionBody?.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.ParameterList.ToString() != add.ParameterList.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.SemicolonToken.ToString() != add.SemicolonToken.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.SemicolonToken.ToString() != add.SemicolonToken.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            return children;
        }

        private IEnumerable<Diff> MakeARecursive(int index, PropertyDeclarationSyntax delete, PropertyDeclarationSyntax add)
        {
            if (delete.Identifier.ToString() != add.Identifier.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Identifier, add.Identifier));
            }

            if (delete.Modifiers.ToString() != add.Modifiers.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Modifiers, add.Modifiers));
            }

            if (delete.Type.ToString() != add.Type.ToString())
            {
                throw NotSupportedExceptions.Value((delete.Type, add.Type));
            }

            if (delete.AccessorList != null && add.AccessorList != null)
            {
                if (delete.AccessorList.ToFullString() != add.AccessorList.ToFullString())
                {
                    yield return this.MakeARecursive(delete.AccessorList, add.AccessorList, index);
                }
            }

            if (delete.ExpressionBody?.ToString() != add.ExpressionBody?.ToString())
            {
                if (delete.ExpressionBody != null && add.ExpressionBody != null)
                {
                    foreach (var d in expressionDiffToolsConfig.MakeARecursive(index,
                                 delete.ExpressionBody.Expression,
                                 add.ExpressionBody.Expression,
                                 SyntaxTriviaList.Empty)) yield return d;
                }
            }

            if (delete.Initializer?.ToString() != add.Initializer?.ToString())
            {
                foreach (var d in this.MakeARecursive(index,
                             delete.Initializer,
                             add.Initializer)) yield return d;
            }

            if (delete.SemicolonToken.ToString() != add.SemicolonToken.ToString())
            {
                foreach (var d in MakeARecursive(index,
                             delete.SemicolonToken,
                             add.SemicolonToken,
                             SyntaxTriviaList.Empty)) yield return d;
            }

            if (delete.AttributeLists.ToString() != add.AttributeLists.ToString())
            {
                throw NotSupportedExceptions.Value((delete.AttributeLists, add.AttributeLists));
            }

            if (delete.ExplicitInterfaceSpecifier?.ToString() != add.ExplicitInterfaceSpecifier?.ToString())
            {
                throw NotSupportedExceptions.Value((delete.ExplicitInterfaceSpecifier, add.ExplicitInterfaceSpecifier));
            }

            yield break;
        }

        private Diff MakeARecursive(AccessorListSyntax deleteAccessorList, AccessorListSyntax addAccessorList, int indexOfChild)
        {
            var children = this.triviaDiffTools.GetTriviaChildren(deleteAccessorList, addAccessorList);

            if (deleteAccessorList.Accessors.ToFullString() != addAccessorList.Accessors.ToFullString())
            {
                throw NotSupportedExceptions.Value((deleteAccessorList.Accessors, addAccessorList.Accessors));
            }

            return new AccessorListDiff(DiffMode.Update, indexOfChild, 0, deleteAccessorList, children);
        }

        private IEnumerable<Diff> MakeARecursive(int delete, SyntaxToken deleteSemicolonToken, SyntaxToken addSemicolonToken, SyntaxTriviaList empty)
        {
            yield return new SemicolonTokenDiff(DiffMode.Update, delete, 0, addSemicolonToken);
        }

        private IEnumerable<Diff> MakeARecursive(int delete, EqualsValueClauseSyntax? deleteInitializer, EqualsValueClauseSyntax? addInitializer)
        {
            if (deleteInitializer == null && addInitializer != null)
            {
                yield return new EqualsValueClauseDiff(DiffMode.Add, delete, 0, addInitializer);

                yield break;
            }

            throw new NotImplementedException();
        }

        private IEnumerable<Diff> MakeARecursive(int index, MethodDeclarationSyntax delete, MethodDeclarationSyntax add)
        {
            if (delete.ReturnType.ToString() != add.ReturnType.ToString())
            {
                yield return new MethodReturnTypeDiff(DiffMode.Update, index, 0, add.ReturnType);
            }

            if (delete.Body != null && add.Body != null)
            {
                foreach (var d in this.diffTools.GetDiffOfChildrenFusion(
                             new StatementDiffToolsConfig(this.diffTools),
                             delete.Body.Statements.ToList(),
                             add.Body.Statements.ToList()))
                {
                    yield return d;
                }
            }
        }

        private List<Diff> MakeARecursive(int index, NamespaceDeclarationSyntax delete, NamespaceDeclarationSyntax add)
        {
            var children = this.triviaDiffTools.GetTriviaChildren(delete, add);

            children.AddRange(this.GetTokenDiff(TokenDiffEnum.CloseBrace, delete.CloseBraceToken, add.CloseBraceToken));
            children.AddRange(this.GetTokenDiff(TokenDiffEnum.OpenBrace, delete.OpenBraceToken, add.OpenBraceToken));

            children.AddRange(this.diffTools.GetDiffOfChildrenFusion(
                new MemberDeclarationDiffToolsConfig(this.diffTools),
                delete.Members.ToList(),
                add.Members.ToList()));

            return children;
        }

        private List<Diff> GetTokenDiff(TokenDiffEnum tokenDiffEnum, SyntaxToken delete, SyntaxToken add)
        {
            var closeChldren = this.GetTriviaChildren(delete, add);

            if (closeChldren.Any())
            {
                return new List<Diff>
                {
                    new TokenDiff(DiffMode.Update, tokenDiffEnum, 0, 0, closeChldren)
                };
            }

            return new List<Diff>();
        }

        private IEnumerable<Diff> MakeARecursive(int index, ClassDeclarationSyntax delete, ClassDeclarationSyntax add)
        {
            var children = this.triviaDiffTools.GetTriviaChildren(delete, add);

            children.AddRange(this.GetTokenDiff(TokenDiffEnum.CloseBrace, delete.CloseBraceToken, add.CloseBraceToken));
            children.AddRange(this.GetTokenDiff(TokenDiffEnum.OpenBrace, delete.OpenBraceToken, add.OpenBraceToken));

            children.AddRange(this.diffTools.GetDiffOfChildrenFusion(
                new MemberDeclarationDiffToolsConfig(this.diffTools),
                delete.Members.ToList(),
                add.Members.ToList()));

            return children;
        }

        public override MemberDeclarationDiff CreateDiff(DiffMode mode, List<MemberDeclarationSyntax> elements, int index)
        {
            return new MemberDeclarationDiff(mode, index, 0, elements[index]);
        }

        private List<Diff> GetTriviaChildren(SyntaxToken delete, SyntaxToken add)
        {
            var children = new List<Diff>();

            if (delete.LeadingTrivia != add.LeadingTrivia)
            {
                children.AddRange(this.diffTools.GetDiffOfChildrenFusion(
                    new SyntaxTriviaListDiffToolsConfig(TriviaType.Leading),
                    delete.LeadingTrivia.ToList(),
                    add.LeadingTrivia.ToList()));
            }

            if (delete.TrailingTrivia != add.TrailingTrivia)
            {
                children.AddRange(this.diffTools.GetDiffOfChildrenFusion(
                    new SyntaxTriviaListDiffToolsConfig(TriviaType.Trailing),
                    delete.TrailingTrivia.ToList(),
                    add.TrailingTrivia.ToList()));
            }

            return children;
        }
    }

    public enum TokenDiffEnum
    {
        CloseBrace,
        OpenBrace
    }
    
    public class TokenDiff : Diff, IDiffChildren
    {
        public TokenDiff(DiffMode mode,
            TokenDiffEnum tokenDiffEnum,
            int indexOfChild,
            int moveIndexOfChild,
            List<Diff>? children) : base(mode, indexOfChild, moveIndexOfChild)
        {
            this.TokenDiffEnum = tokenDiffEnum;
            this.Children = children;
        }

        public TokenDiffEnum TokenDiffEnum { get; }
        
        public List<Diff>? Children { get; }
    }

    public class TriviaDiff : Diff<SyntaxTrivia>
    {
        public TriviaType Type { get; set; }
        public TriviaDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, SyntaxTrivia value, TriviaType type) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
            this.Type = type;
        }

        protected override string GetName()
        {
            return Value.ToFullString();
        }
    }

    internal class AccessorListDiff : Diff<AccessorListSyntax>, IDiffChildren
    {
        public AccessorListDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, AccessorListSyntax value, List<Diff>? children) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
            this.Children = children;
        }

        public List<Diff>? Children { get; }
    }

    public class SemicolonTokenDiff : Diff<SyntaxToken>
    {
        public SemicolonTokenDiff(DiffMode disffMode, int index, int i, SyntaxToken addInitializer) 
            : base(disffMode, index, i, addInitializer)
        {
        }
    }

    public class EqualsValueClauseDiff : Diff<EqualsValueClauseSyntax>
    {
        public EqualsValueClauseDiff(DiffMode mode, int indexOfChild, int moveIndexOfChild, EqualsValueClauseSyntax value) : base(mode, indexOfChild, moveIndexOfChild, value)
        {
        }
    }
}