using System;
using System.Collections.Generic;
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

    private IMemberCombined<MemberDeclarationSyntax> GetMemberCombined(MemberDeclarationSyntax newMember)
    {
        return newMember switch
        {
            ClassDeclarationSyntax c => new ClassMemberCombined(c),
            NamespaceDeclarationSyntax c => new NamespaceMemberCombined(c),
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
        
        return current.WithBody(this.ApplyStatementDiffOnBlockSyntax(diff, currentBody, index));
    }

    private MemberDeclarationSyntax ApplyStatementDiffOnMethodDeclaration(StatementDiff diff, MethodDeclarationSyntax current, int index)
    {
        var currentBody = current.Body;

        if (currentBody == null)
        {
            return current;
        }
        
        return current.WithBody(this.ApplyStatementDiffOnBlockSyntax(diff, currentBody, index));
    }

    private BlockSyntax ApplyStatementDiffOnBlockSyntax(StatementDiff diff, BlockSyntax currentBody, int index)
    {
        if (diff is not { Mode: DiffMode.Update })
        {
            return currentBody.WithStatements(this.ApplyInsertOrDeleteDiff(currentBody.Statements.To(), diff, index));
        }

        var baseMember = currentBody.Statements[diff.IndexOfChild];

        var newMember = this.ApplyStatementDiffOnStatementSyntax(diff, baseMember);

        return currentBody.WithStatements(currentBody.Statements.Replace(baseMember, newMember));
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
                var d => throw NotSupportedExceptions.Value(d)
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
                    var x => throw NotSupportedExceptions.Value(t)
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
    
    private T2 ApplyOnMemberDeclaration<T2>(MemberDeclarationDiff memberDeclarationDiff, IMemberCombined<T2> current, int i)
    {
        if (memberDeclarationDiff is not { Mode : DiffMode.Update })
        {
            return current.WithMembers(this.ApplyInsertOrDeleteDiff(current.Members.To(), memberDeclarationDiff, i));
        }
        
        var baseMember = current.Members[memberDeclarationDiff.IndexOfChild + i];

        var newMember = this.ApplyOnChildren(baseMember, memberDeclarationDiff.Children, this.ApplyOnMemberDeclarationSyntax);

        return current.WithMembers(current.Members.Replace(baseMember, newMember));
    }
    

    private StatementSyntax ApplyStatementDiffOnStatementSyntax(StatementDiff diff, StatementSyntax baseMember) 
    {
        if(diff is not {Mode: DiffMode.Update})
        {
            throw new NotImplementedException();
        }
            
        return this.ApplyOnChildren(baseMember, diff.Children, this.ApplyOnStatementSyntax);
    }

    private VariableDeclarationSyntax ApplyVariableDeclarationDiffOnVariableDeclaration(VariableDeclarationDiff diff, VariableDeclarationSyntax current)
    {
        return this.ApplyOnChildren(current, diff.Children, this.ApplyOnVariableDeclarationSyntax);
    }

    private StatementSyntax ApplyExpressionDiffOnExpressionStatementSyntax(ExpressionDiff expressionDiff, ExpressionStatementSyntax current)
    {
        return current.WithExpression(this.ApplyExpressionDiffOnExpressionSyntax(expressionDiff, current.Expression));
    }
        
    private VariableDeclarationSyntax ApplyVariableDeclaratorDiffOnVariableDeclarationSyntax(VariableDeclaratorDiff diff, VariableDeclarationSyntax current)
    {
        if (diff is not { Mode: DiffMode.Update })
        {
            return current.WithVariables(this.ApplyInsertOrDeleteDiff(current.Variables.To(), diff, 0));
        }

        var baseMember = current.Variables[diff.IndexOfChild];

        var newMember = this.ApplyOnChildren(baseMember, diff.Children, this.ApplyOnVariableDeclaratorSyntax);

        return current.WithVariables(current.Variables.Replace(baseMember, newMember));
    }

    private StatementSyntax ApplyStatementDiffOnBlockStatementSyntax(StatementDiff diff, BlockSyntax current, int index)
    {
        if (diff is not { Mode: DiffMode.Update })
        {
            return current.WithStatements(this.ApplyInsertOrDeleteDiff(current.Statements.To(), diff, index));
        }

        var baseMember = current.Statements[diff.IndexOfChild];

        var newMember = this.ApplyStatementDiffOnStatementSyntax(diff, baseMember);

        return current.WithStatements(current.Statements.Replace(baseMember, newMember));
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
            DiffMode.Move=> syntaxList.Move(syntaxIndexOfChild, syntax.MoveIndexOfChild),
            var value => throw NotSupportedExceptions.Value(value)
        };
    }
    
    private ExpressionSyntax ApplyOnExpressionSyntax(Diff child, ExpressionSyntax current, int index)
    {
        return (child, current) switch
        {
            (ExpressionDiff d, ParenthesizedExpressionSyntax m) => m.WithExpression(this.ApplyExpressionDiffOnExpressionSyntax(d, m.Expression)),
            (ExpressionDiff d, ExpressionSyntax m) => this.ApplyExpressionDiffOnExpressionSyntax(d, m),
            (AssignmentExpressionDiff d, AssignmentExpressionSyntax m) => this.ApplyAssignmentExpressionDiffOnExpressionSyntax(d, m),
            (AssignmentExpressionDiff d, InvocationExpressionSyntax m) => this.ApplyAssignmentExpressionDiffOnInvocationExpressionSyntax(d, m, index),
            (ArgumentDiff d, InvocationExpressionSyntax m) => this.ApplyArgumentDiffOnInvocationExpressionSyntax(d, m, index),
            (NameMemberAccessExpressionDiff d, MemberAccessExpressionSyntax m) => m.WithName(d.Value),
            (ExpressionBodyDiff d, ParenthesizedLambdaExpressionSyntax m) => this.ApplyExpressionBodyDiffOnParenthesizedLambdaExpressionSyntax(d, m),
            var value => throw NotSupportedExceptions.Value(value)
        };
    }

    private ExpressionSyntax ApplyExpressionBodyDiffOnParenthesizedLambdaExpressionSyntax(ExpressionBodyDiff diff, ParenthesizedLambdaExpressionSyntax current)
    {
        if (diff.Mode == DiffMode.Add)
        {
            return diff.Value;
        }
            
        if (diff.Mode == DiffMode.Update)
        {
            if (diff.Children == null)
            {
                throw NotSupportedExceptions.Value(current);
            }

            return ApplyOnChildren(current, diff.Children, ApplyOnExpressionSyntax);
        }

        return current switch
        {
            var value => throw NotSupportedExceptions.Value(value)
        };
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
        if (diff.Mode == DiffMode.Add)
        {
            return current.WithBody(diff.Value);
        }
            
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

    private ExpressionSyntax ApplyAssignmentExpressionDiffOnInvocationExpressionSyntax(AssignmentExpressionDiff expressionDiff, InvocationExpressionSyntax current, int index)
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

    private MemberDeclarationSyntax ApplyOnMemberDeclarationSyntax(Diff child, MemberDeclarationSyntax newMember, int index)
    {
        return (child, newMember) switch
        {
            (StatementDiff d, MethodDeclarationSyntax m) => this.ApplyStatementDiffOnMethodDeclaration(d, m, index),
            (StatementDiff d, ConstructorDeclarationSyntax m) => this.ApplyStatementDiffOnConstructorDeclaration(d, m, index),
            (MemberDeclarationDiff d, { } m) =>  this.ApplyOnMemberDeclaration(d, this.GetMemberCombined(m), index),
            (MethodReturnTypeDiff d, MethodDeclarationSyntax methodDeclarationSyntax) => methodDeclarationSyntax.WithReturnType(d.Value),
            (EqualsValueClauseDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplyEqualsValueClauseDiffOnPropertyDeclaration(d, propertyDeclarationSyntax),
            (SemicolonTokenDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplySemicolonTokenDiffOnPropertyDeclaration(d, propertyDeclarationSyntax),
            (AccessorListDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplyAccessorListSyntaxOnPropertyDeclaration(d, propertyDeclarationSyntax),
            (TokenDiff diff, NamespaceDeclarationSyntax d) => this.ApplyTokenDiffOnNamespaceDeclarationSyntax(diff, d),
            (TokenDiff diff, ClassDeclarationSyntax d) => this.ApplyTokenDiffOnTypeDeclarationSyntax(diff, d),
            var d => throw NotSupportedExceptions.Value(d)
        };
    }
        
    private StatementSyntax ApplyOnStatementSyntax(Diff child, StatementSyntax newMember, int index = 0)
    {
        return (child, newMember) switch
        {
            (StatementDiff b, IfStatementSyntax ifStatementSyntax) => ifStatementSyntax.WithStatement(this.ApplyStatementDiffOnStatementSyntax(b, ifStatementSyntax.Statement)),
            (StatementDiff b, BlockSyntax blockSyntax) => this.ApplyStatementDiffOnBlockStatementSyntax(b, blockSyntax, index),
            (VariableDeclarationDiff d, LocalDeclarationStatementSyntax ifStatementSyntax) => ifStatementSyntax.WithDeclaration(this.ApplyVariableDeclarationDiffOnVariableDeclaration(d, ifStatementSyntax.Declaration)),
            (VariableDeclarationTypeDiff d, LocalDeclarationStatementSyntax syntax) => syntax.WithDeclaration(syntax.Declaration.WithType(d.DeclarationType)),
            (ExpressionDiff d, ReturnStatementSyntax m) => this.ApplyExpressionDiffOnReturnStatement(d, m),
            (ExpressionDiff b, ExpressionStatementSyntax s)=> this.ApplyExpressionDiffOnExpressionStatementSyntax(b, s),
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
        
    private VariableDeclarationSyntax ApplyOnVariableDeclarationSyntax(Diff child, VariableDeclarationSyntax newMember, int index = 0)
    {
        return (child, newMember) switch
        {
            (VariableDeclaratorDiff d, VariableDeclarationSyntax m) =>  this.ApplyVariableDeclaratorDiffOnVariableDeclarationSyntax(d, m) , 
            (VariableDeclarationTypeDiff d, VariableDeclarationSyntax m) => m.WithType(d.DeclarationType),
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