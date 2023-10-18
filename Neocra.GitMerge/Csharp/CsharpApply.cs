using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Csharp.DiffTools;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp;

public class CsharpApply
{
    public CompilationUnitSyntax Apply(CompilationUnitSyntax current, List<Diff> diffUsing)
    {
        return this.ApplyOnChildren(current, diffUsing, this.ApplyOnCompilationUnitSyntax);
    }

    private IMemberCombined<MemberDeclarationSyntax>? GetMemberCombined(MemberDeclarationSyntax newMember)
    {
        return newMember switch
        {
            ClassDeclarationSyntax c => new ClassMemberCombined(c),
            NamespaceDeclarationSyntax c => new NamespaceMemberCombined(c),
            FileScopedNamespaceDeclarationSyntax c => new FileScopedNamespaceMemberCombined(c),
            PropertyDeclarationSyntax c => null,
            MethodDeclarationSyntax c => null, // TODO : Statements ?
            ConstructorDeclarationSyntax c => null, // TODO : Statements ?
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
    
    private IList<BlockSyntax, SyntaxList<StatementSyntax>, StatementSyntax> GetMemberCombined(BlockSyntax newMember)
    {
        return newMember switch
        {
            BlockSyntax c => new BlockSyntaxCombined(c),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
        
    private IList<VariableDeclarationSyntax, SeparatedSyntaxList<VariableDeclaratorSyntax>, VariableDeclaratorSyntax> GetMemberCombined(VariableDeclarationSyntax newMember)
    {
        return newMember switch
        {
            VariableDeclarationSyntax c => new VariableDeclarationSyntaxCombined(c),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
    
    private MemberDeclarationSyntax ApplyStatementDiffOnConstructorDeclaration(StatementDiff diff, ConstructorDeclarationSyntax current, int index)
    {
        var currentBody = current.Body;

        if (currentBody == null)
        {
            return current;
        }
        
        return current.WithBody(this.ApplyStatementDiffOnBlockSyntax(diff, currentBody));
    }

    private MemberDeclarationSyntax ApplyStatementDiffOnMethodDeclaration(StatementDiff diff, MethodDeclarationSyntax current, int index)
    {
        var currentBody = current.Body;

        if (currentBody == null)
        {
            return current;
        }
        
        return current.WithBody(this.ApplyStatementDiffOnBlockSyntax(diff, currentBody));
    }



    private MemberDeclarationSyntax ApplyTokenDiffOnTypeDeclarationSyntax(TokenDiff diff, TypeDeclarationSyntax current) => this.ApplyTokenDiffOnMemberDeclarationSyntax(diff, current, current.WithCloseBraceToken, current.CloseBraceToken, current.WithOpenBraceToken, current.OpenBraceToken);

    private MemberDeclarationSyntax ApplyTokenDiffOnNamespaceDeclarationSyntax(TokenDiff diff, NamespaceDeclarationSyntax current) => this.ApplyTokenDiffOnMemberDeclarationSyntax(diff, current, current.WithCloseBraceToken, current.CloseBraceToken, current.WithOpenBraceToken, current.OpenBraceToken);

    private MemberDeclarationSyntax ApplyTokenDiffOnMemberDeclarationSyntax<T>(TokenDiff diff, T current, Func<SyntaxToken, T> withCloseBraceToken,
        SyntaxToken nCloseBraceToken, Func<SyntaxToken, T> withOpenBraceToken, SyntaxToken nOpenBraceToken)
        where T : MemberDeclarationSyntax
    {
        if (diff.Mode == DiffMode.Update)
        {
            return  diff switch
            {
                { TokenDiffEnum: TokenDiffEnum.CloseBrace } => withCloseBraceToken(this.ApplyOnChildren(nCloseBraceToken, diff.Children, this.Apply)),
                { TokenDiffEnum: TokenDiffEnum.OpenBrace } => withOpenBraceToken(this.ApplyOnChildren(nOpenBraceToken, diff.Children, this.Apply)),
                _ => throw NotSupportedExceptions.Value(diff)
            };
        }

        throw NotSupportedExceptions.Value(current);
    }

    private SyntaxToken Apply(Diff diff, SyntaxToken current, int index)
    {
        switch (diff)
        {
            case TriviaDiff t:
                return t switch
                {
                    { Type: TriviaType.Trailing } => current.WithTrailingTrivia(
                        this.ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(current.TrailingTrivia), t, index)),
                    { Type: TriviaType.Leading } => current.WithLeadingTrivia(
                        this.ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(current.LeadingTrivia), t, index)),
                    _ => throw NotSupportedExceptions.Value(t)
                };
            case var x:
                throw NotSupportedExceptions.Value(x);
        }
    }


    private MemberDeclarationSyntax ApplyAccessorListSyntaxOnPropertyDeclaration(AccessorListDiff accessorListSyntax, PropertyDeclarationSyntax propertyDeclarationSyntax)
    {   
        switch (accessorListSyntax.Mode)
        {
            case DiffMode.Update:

                var a = propertyDeclarationSyntax.AccessorList;

                if (a == null)
                {
                    throw NotSupportedExceptions.Value(a);
                }

                var children = accessorListSyntax.Children ?? new List<Diff>();
                foreach (var child in children)
                {
                    var index = children.Sum(d =>
                    {
                        if (d.Mode == DiffMode.Delete && d.IndexOfChild < child.IndexOfChild)
                        {
                            return -1;
                        }

                        return 0;
                    });

                        
                    a = child switch
                    {
                        TriviaDiff d => (d.Type switch
                        {
                            TriviaType.Leading => a.WithLeadingTrivia(this.ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(a.GetLeadingTrivia()), d, index)),
                            TriviaType.Trailing => a.WithTrailingTrivia(this.ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(a.GetTrailingTrivia()), d, index)),
                            var t => throw NotSupportedExceptions.Value(t)
                        }),
                        var d => throw NotSupportedExceptions.Value(d)
                    };
                }
                    
                return propertyDeclarationSyntax.WithAccessorList(a);
            case var m:
                throw new NotSupportedException(m.ToString());
        }
    }

    private MemberDeclarationSyntax ApplySemicolonTokenDiffOnPropertyDeclaration(SemicolonTokenDiff semicolonTokenDiff, PropertyDeclarationSyntax propertyDeclarationSyntax)
    {
        switch (semicolonTokenDiff.Mode)
        {
            case DiffMode.Update:
                return propertyDeclarationSyntax.WithSemicolonToken(semicolonTokenDiff.Value);
            case var m:
                throw new NotSupportedException(m.ToString());
        }
    }

    private MemberDeclarationSyntax ApplyEqualsValueClauseDiffOnPropertyDeclaration(EqualsValueClauseDiff equalsValueClauseDiff, PropertyDeclarationSyntax propertyDeclarationSyntax)
    {
        switch (equalsValueClauseDiff.Mode)
        {
            case DiffMode.Add:
                return propertyDeclarationSyntax
                    .WithInitializer(equalsValueClauseDiff.Value);
            case var m:
                throw new NotSupportedException(m.ToString());
        }
    }


    private StatementSyntax ApplyExpressionDiffOnReturnStatement(ExpressionDiff expressionDiff, ReturnStatementSyntax current)
    {
        if (expressionDiff.Mode == DiffMode.Add)
        {
            return current.WithExpression( expressionDiff.Value);
        }

        if (current.Expression == null)
        {
            throw NotSupportedExceptions.Value(current);
        }
        
        return current.WithExpression(this.ApplyExpressionDiffOnExpressionSyntax(expressionDiff, current.Expression));
    }

    private ExpressionSyntax ApplyExpressionDiffOnExpressionSyntax(ExpressionDiff expressionDiff, ExpressionSyntax current)
    {
        if (expressionDiff.Mode == DiffMode.Update)
        {
            if (expressionDiff.Children == null)
            {
                return current switch
                {
                    LiteralExpressionSyntax l => expressionDiff.Value,
                    BinaryExpressionSyntax b => expressionDiff.Value,
                    var value => throw NotSupportedExceptions.Value(value)
                };
            }

            return this.ApplyOnChildren(current, expressionDiff.Children, this.ApplyOnExpressionSyntax);
        }

        throw NotSupportedExceptions.Value(current);
    }

    private ExpressionSyntax ApplyAssignmentExpressionDiffOnExpressionSyntax(AssignmentExpressionDiff expressionDiff, AssignmentExpressionSyntax current)
    {
        if (expressionDiff.Mode == DiffMode.Add)
        {
            return expressionDiff.Value;
        }
            
        if (expressionDiff.Mode == DiffMode.Update)
        {
            if (expressionDiff.Children == null)
            {
                throw NotSupportedExceptions.Value(current);
            }
                
            foreach (var child in expressionDiff.Children)
            {
                current = expressionDiff.AssignmentExpressionDiffMode switch
                {
                    AssignmentExpressionDiffMode.Left=> current.WithLeft(this.ApplyDiffOnExpressionSyntax(child, current.Left)),
                    AssignmentExpressionDiffMode.Right=> current.WithRight(this.ApplyDiffOnExpressionSyntax(child, current.Right)),
                    var value => throw NotSupportedExceptions.Value(value)
                };
            }

            if (current == null)
            {
                throw new NotSupportedException();
            }

            return current;
        }

        return current switch
        {
            var value => throw NotSupportedExceptions.Value(value)
        };
    }

    private ExpressionSyntax ApplyDiffOnExpressionSyntax(Diff child, ExpressionSyntax currentLeft)
    {
        return child switch
        {
            ExpressionDiff d => this.ApplyExpressionDiffOnExpressionSyntax(d, currentLeft),
            var value => throw NotSupportedExceptions.Value(value)
        };
    }

    private ExpressionSyntax ApplyArgumentDiffOnInvocationExpressionSyntax(ArgumentDiff diff, InvocationExpressionSyntax current, int index)
    {
        var l = current.ArgumentList.Arguments;

        l = this.ApplyInsertOrDeleteDiff(l.To(), diff, index);

        return current.WithArgumentList(current.ArgumentList.WithArguments(l));
    }

    private T2 ApplyOnMemberDeclaration<T2>(MemberDeclarationDiff memberDeclarationDiff, IMemberCombined<T2>? current,
        int i)
    {
        if(current == null)
        {
            throw NotSupportedExceptions.Value(memberDeclarationDiff);
        }
        
        var baseMember = current.Members[memberDeclarationDiff.IndexOfChild + i];
        var memberCombined = this.GetMemberCombined(baseMember);
        var newMember2 = baseMember;
        if (memberCombined != null)
        {
            newMember2 = this.ApplyOnChildren2 (memberCombined, memberDeclarationDiff.Children
                ?.Where(m => m.GetType() == typeof(MemberDeclarationDiff)).ToList(), this.ApplyOnMemberDeclarationSyntax,
                this.ApplyOnChildren3);
        }
        
        newMember2 = this.ApplyOnChildren(newMember2, memberDeclarationDiff.Children
            ?.Where(m => m.GetType() != typeof(MemberDeclarationDiff)).ToList(), this.ApplyOnMemberDeclarationSyntax);

        return current.WithMembers(current.Members.Replace(baseMember, newMember2));
    }

    private MemberDeclarationSyntax ApplyOnChildren3(MemberDeclarationSyntax arg1, List<Diff> arg2, Func<Diff, MemberDeclarationSyntax, int, MemberDeclarationSyntax> arg3)
    {
        var memberCombined = this.GetMemberCombined(arg1);
        if(memberCombined != null)
        {
            return ApplyOnChildren2(memberCombined, arg2, arg3, this.ApplyOnChildren3);
        }

        return this.ApplyOnChildren(arg1, arg2, this.ApplyOnMemberDeclarationSyntax);
    }


    private T ApplyOnChildren2<T, TListChild, TChild>(
        IList<T, TListChild, TChild> current, 
        List<Diff>? children, 
        Func<Diff, TChild, int, TChild> apply,
        Func<TChild, List<Diff>, Func<Diff, TChild, int, TChild>, TChild> applyOnChildren)
        where TListChild : IReadOnlyCollection<TChild>
    {  
        if (children == null || !children.Any())
        {
            return current.WithMembers(current.Members);
        }
        
        var members = new List<(int, TChild)>();


        for (var index = 0; index < current.Members.Count; index++)
        {
            var member = current.Members.ElementAt(index);
            var diff = children.FirstOrDefault(d => d.IndexOfChild == index && d.Mode != DiffMode.Add);

            if (diff == null)
            {
                members.Add((index * 10 + 5, member));
                continue;
            }

            var m = member;
            if (diff is IDiffChildren { Children: not null } diffChildren)
            {
                m =  applyOnChildren(m, diffChildren.Children, apply);
            }

            if (diff.Mode == DiffMode.Update)
            {
                members.Add((index * 10 + 5, m));
            }
            else if(diff.Mode == DiffMode.Move)
            {
                members.Add((diff.MoveIndexOfChild * 10, m));
            }
        }

        foreach (var diff in children
                     .Where(c=> c.Mode == DiffMode.Add)
                     .OfType<Diff<TChild>>())
        {
            members.Add((diff.IndexOfChild * 10, diff.Value));
        }

        var ms = members
            .OrderBy(l => l.Item1)
            .Select(l => l.Item2)
            .ToImmutableList();

        return current.WithMembers(ms);
    }

    private StatementSyntax ApplyExpressionDiffOnExpressionStatementSyntax(ExpressionDiff expressionDiff, ExpressionStatementSyntax current)
    {
        return current.WithExpression(this.ApplyExpressionDiffOnExpressionSyntax(expressionDiff, current.Expression));
    }
        
    private VariableDeclarationSyntax ApplyVariableDeclaratorDiffOnVariableDeclarationSyntax(VariableDeclaratorDiff diff, VariableDeclarationSyntax current, int index=0)
    {
        if (diff is not { Mode: DiffMode.Update })
        {
            return current.WithVariables(this.ApplyInsertOrDeleteDiff(current.Variables.To(), diff, 0));
        }

        var baseMember = current.Variables[diff.IndexOfChild];

        var newMember = this.ApplyOnChildren(baseMember, diff.Children, this.ApplyOnVariableDeclaratorSyntax);

        return current.WithVariables(current.Variables.Replace(baseMember, newMember));
    }
    
    private StatementSyntax ApplyStatementDiffOnStatementSyntax(StatementDiff diff, StatementSyntax baseMember) 
    {
        if(diff is not {Mode: DiffMode.Update})
        {
            throw new NotImplementedException();
        }
            
        return this.ApplyOnChildren(baseMember, diff.Children, this.ApplyOnStatementSyntax);
    }
    
    private BlockSyntax ApplyStatementDiffOnBlockSyntax(StatementDiff diff, BlockSyntax current)
    {
        return this.ApplyOnChildren2 (this.GetMemberCombined(current), diff.Children, this.ApplyOnStatementSyntax,
            this.ApplyOnChildren);
    }
    
    private TCollection ApplyInsertOrDeleteDiff<T, TCollection>(
        ISyntaxList<T, TCollection>  syntaxList, 
        Diff<T> syntax,
        int index) 
    {
        var syntaxIndexOfChild = syntax.IndexOfChild + index;
        var syntaxValue = syntax.Value;
        return syntax.Mode switch
        {
            DiffMode.Add => syntaxList.Insert(syntaxIndexOfChild, syntaxValue),
            DiffMode.Delete => syntaxList.RemoveAt(syntaxIndexOfChild),
            var value => throw NotSupportedExceptions.Value(value)
        };
    }
    
    private ExpressionSyntax ApplyOnExpressionSyntax(Diff child, ExpressionSyntax current, int index = 0)
    {
        return (child, current) switch
        {
            (ExpressionDiff d, ParenthesizedExpressionSyntax m) => m.WithExpression(this.ApplyExpressionDiffOnExpressionSyntax(d, m.Expression)),
            (AssignmentExpressionDiff d, AssignmentExpressionSyntax m) => this.ApplyAssignmentExpressionDiffOnExpressionSyntax(d, m),
            (ArgumentDiff d, InvocationExpressionSyntax m) => this.ApplyArgumentDiffOnInvocationExpressionSyntax(d, m, index),
            (NameMemberAccessExpressionDiff {Value: SimpleNameSyntax value}, MemberAccessExpressionSyntax m) => m.WithName(value),
            (ExpressionBodyDiff d, ParenthesizedLambdaExpressionSyntax m) => this.ApplyExpressionBodyDiffOnParenthesizedLambdaExpressionSyntax(d, m),
            var value => throw NotSupportedExceptions.Value(value)
        };
    }

    private ExpressionSyntax ApplyExpressionBodyDiffOnParenthesizedLambdaExpressionSyntax(ExpressionBodyDiff diff, ParenthesizedLambdaExpressionSyntax current)
    {
        return ApplyOnChildren(current, diff.Children, ApplyOnExpressionSyntax);
    }

    private ParenthesizedLambdaExpressionSyntax ApplyOnExpressionSyntax(Diff child, ParenthesizedLambdaExpressionSyntax current, int index)
    {
        return (child, current) switch
        {
            (ExpressionDiff d, ParenthesizedLambdaExpressionSyntax m) => this.ApplyExpressionDiffOnParenthesizedLambdaExpressionSyntax(d, m),
            var value => throw NotSupportedExceptions.Value(value)
        };
    }

    private ParenthesizedLambdaExpressionSyntax ApplyExpressionDiffOnParenthesizedLambdaExpressionSyntax(ExpressionDiff diff, ParenthesizedLambdaExpressionSyntax current)
    {
        if (diff.Mode == DiffMode.Update)
        {
            if (diff.Children == null)
            {
                return current.WithBody(diff.Value);
            }

            return ApplyOnChildren(current, diff.Children, ApplyOnExpressionSyntax);
        }

        return current switch
        {
            var value => throw NotSupportedExceptions.Value(value)
        };
    }

    private MemberDeclarationSyntax ApplyOnMemberDeclarationSyntax(Diff child, MemberDeclarationSyntax newMember, int index)
    {
        return (child, newMember) switch
        {
            (StatementDiff d, MethodDeclarationSyntax m) => this.ApplyStatementDiffOnMethodDeclaration(d, m, index),
            (StatementDiff d, ConstructorDeclarationSyntax m) => this.ApplyStatementDiffOnConstructorDeclaration(d, m, index),
            (MethodReturnTypeDiff d, MethodDeclarationSyntax methodDeclarationSyntax) => methodDeclarationSyntax.WithReturnType(d.Value),
            (EqualsValueClauseDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplyEqualsValueClauseDiffOnPropertyDeclaration(d, propertyDeclarationSyntax),
            (SemicolonTokenDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplySemicolonTokenDiffOnPropertyDeclaration(d, propertyDeclarationSyntax),
            (AccessorListDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplyAccessorListSyntaxOnPropertyDeclaration(d, propertyDeclarationSyntax),
            (TokenDiff diff, NamespaceDeclarationSyntax d) => this.ApplyTokenDiffOnNamespaceDeclarationSyntax(diff, d),
            (TokenDiff diff, ClassDeclarationSyntax d) => this.ApplyTokenDiffOnTypeDeclarationSyntax(diff, d),
            (NameMemberAccessExpressionDiff diff, BaseNamespaceDeclarationSyntax d) => d.WithName(diff.Value),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
    
    private StatementSyntax ApplyOnStatementSyntax(Diff child, StatementSyntax newMember, int index = 0)
    {
        return (child, newMember) switch
        {
            (StatementDiff b, IfStatementSyntax ifStatementSyntax) => ifStatementSyntax.WithStatement(this.ApplyStatementDiffOnStatementSyntax(b, ifStatementSyntax.Statement)),
            (VariableDeclarationDiff d, LocalDeclarationStatementSyntax ifStatementSyntax) => ifStatementSyntax.WithDeclaration(this.ApplyVariableDeclarationDiffOnVariableDeclaration(d, ifStatementSyntax.Declaration)),
            (ExpressionDiff b, ExpressionStatementSyntax s)=> this.ApplyExpressionDiffOnExpressionStatementSyntax(b, s),
            (StatementDiff d, BlockSyntax m) => this.ApplyStatementDiffOnBlockSyntax(d, m),
            (StatementDiff d, ReturnStatementSyntax m) => ApplyOnChildren(newMember, d.Children, ApplyOnStatementSyntax),
            (ExpressionDiff d, ReturnStatementSyntax m) => this.ApplyExpressionDiffOnReturnStatement(d, m),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
    private VariableDeclarationSyntax ApplyVariableDeclarationDiffOnVariableDeclaration(VariableDeclarationDiff diff, VariableDeclarationSyntax current)
    {
        var m = current;

        m = ApplyOnChildren2(
            this.GetMemberCombined(m),
            diff.Children.OfType<VariableDeclaratorDiff>().Cast<Diff>().ToList(),
            ApplyOnVariableDeclaratorSyntax,
            this.ApplyOnChildren);
        
        return this.ApplyOnChildren(m, diff.Children
            .Where(d=> d.GetType() != typeof(VariableDeclaratorDiff)).ToList(), this.ApplyOnVariableDeclarationSyntax);
    }
            
    private VariableDeclarationSyntax ApplyOnVariableDeclarationSyntax(Diff child, VariableDeclarationSyntax newMember, int index = 0)
    {
        return (child, newMember) switch
        {
            (VariableDeclaratorDiff d, VariableDeclarationSyntax m) =>  this.ApplyVariableDeclaratorDiffOnVariableDeclarationSyntax(d, m) , 
            (VariableDeclarationTypeDiff d, VariableDeclarationSyntax m) => m.WithType(d.DeclarationType),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
    private VariableDeclaratorSyntax ApplyOnVariableDeclaratorSyntax(Diff child, VariableDeclaratorSyntax current, int index = 0)
    {
        return (child, current) switch
        {
            (ExpressionDiff d, VariableDeclaratorSyntax m) => m.Initializer == null ? m : m.WithInitializer(m.Initializer.WithValue(this.ApplyExpressionDiffOnExpressionSyntax(d, m.Initializer.Value))),
            (IdentifierDiff d, VariableDeclaratorSyntax m) => m.WithIdentifier(d.Value),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }

    private CompilationUnitSyntax ApplyOnCompilationUnitSyntax(Diff diff, CompilationUnitSyntax current, int index)
    {
        return diff switch
        {
            MemberDeclarationDiff memberDeclarationDiff => this.ApplyOnMemberDeclaration(memberDeclarationDiff, new CompilationUnitSyntaxCombined(current), 0),
            UsingDirectiveDiff d => current.WithUsings(this.ApplyInsertOrDeleteDiff(current.Usings.To(), d, 0)),
            var d => throw new NotSupportedException(d.ToString())
        };
    }
        
        
    private T ApplyOnChildren<T>(T current, List<Diff>? children, Func<Diff, T, int, T> apply)
    {
        if (children == null)
        {
            return current;
        }
            
        int i = 0;
        int currentI = 0;
        int currentIndex = 0;
        foreach (var child in children
                     .OrderBy(c=> c.Mode switch
                     {
                         DiffMode.Move => 1,
                         _ => 2
                     }).ThenBy(c=>c.IndexOfChild))
        {
            if (currentIndex != child.IndexOfChild)
            {
                currentIndex = child.IndexOfChild;
                currentI = i;
            }

            current = apply(child, current, currentI);

            if (child.Mode == DiffMode.Move)
            {
                currentI--;
            }
                
            if (child.Mode == DiffMode.Delete)
            {
                i--;
            }
                
            if (child.Mode == DiffMode.Add)
            {
                i++;
            }
        }

        return current;
    }
}

internal class FileScopedNamespaceMemberCombined : IMemberCombined<MemberDeclarationSyntax>
{
    private readonly FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax;

    public FileScopedNamespaceMemberCombined(FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
    {
        this.fileScopedNamespaceDeclarationSyntax = fileScopedNamespaceDeclarationSyntax;
    }

    public SyntaxList<MemberDeclarationSyntax> Members => fileScopedNamespaceDeclarationSyntax.Members;
    public MemberDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
    {
        return fileScopedNamespaceDeclarationSyntax.WithMembers(members);
    }

    public MemberDeclarationSyntax WithMembers(IEnumerable<MemberDeclarationSyntax> members)
    {
        return fileScopedNamespaceDeclarationSyntax.WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));
    }
}

internal class VariableDeclarationSyntaxCombined : IList<VariableDeclarationSyntax,SeparatedSyntaxList<VariableDeclaratorSyntax>,VariableDeclaratorSyntax>
{
    private readonly VariableDeclarationSyntax variableDeclarationSyntax;

    public VariableDeclarationSyntaxCombined(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        this.variableDeclarationSyntax = variableDeclarationSyntax;
    }

    public SeparatedSyntaxList<VariableDeclaratorSyntax> Members => this.variableDeclarationSyntax.Variables;
    public VariableDeclarationSyntax WithMembers(SeparatedSyntaxList<VariableDeclaratorSyntax> members)
    {
        return this.variableDeclarationSyntax.WithVariables(members);
    }

    public VariableDeclarationSyntax WithMembers(IEnumerable<VariableDeclaratorSyntax> members)
    {
        return variableDeclarationSyntax.WithVariables(new SeparatedSyntaxList<VariableDeclaratorSyntax>()
            .AddRange(members));
    }
}