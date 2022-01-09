using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Neocra.GitMerge.Csharp.Diffs;
using Neocra.GitMerge.Csharp.DiffTools;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp
{
    public class CsharpMerger : IMerger
    {
        private readonly Tools.DiffTools diffTools;
        private readonly ILogger<CsharpMerger> logger;

        public CsharpMerger(Tools.DiffTools diffTools, ILogger<CsharpMerger> logger)
        {
            this.diffTools = diffTools;
            this.logger = logger;
        }

        public MergeStatus Merge(MergeOptions opts)
        {
            var ancestor = this.ParseFile(opts.Ancestor);
            var current = this.ParseFile(opts.Current);
            var other = this.ParseFile(opts.Other);

            var diffCurrent = this.Diffs(ancestor, current);
            var diffOther = this.Diffs(ancestor, other);
            
            if (DetectConflict(diffCurrent, diffOther))
            {
                return MergeStatus.Conflict;
            }
            
            ancestor = Apply(ancestor, diffCurrent.Union(diffOther).ToList());
            
            File.Delete(opts.Current);
            
            using (var streamWriter = new StreamWriter(File.OpenWrite(opts.Current)))
            {
                ancestor.WriteTo(streamWriter);
            }

            return MergeStatus.Good;
        }

        private List<Diff> Diffs(CompilationUnitSyntax ancestor, CompilationUnitSyntax current)
        {
            var diffUsingCurrent = this.diffTools.GetDiffOfChildrenFusion(
                new UsingDirectiveDiffToolsConfig(),
                ancestor.Usings.ToList(),
                current.Usings.ToList()).ToList();

            var diffMembersCurrent = this.diffTools.GetDiffOfChildrenFusion(
                new MemberDeclarationDiffToolsConfig(this.diffTools),
                ancestor.Members.ToList(),
                current.Members.ToList()).ToList();
            return diffUsingCurrent.OfType<Diff>()
                .Union(diffMembersCurrent).ToList();
        }

        private bool DetectConflict(List<Diff> diffsCurrent, List<Diff> diffsOther)
        {
            foreach (var diffCurrent in diffsCurrent)
            {
                var diffOther = diffsOther.FirstOrDefault(d => d.IndexOfChild == diffCurrent.IndexOfChild);

                if (diffOther == null)
                {
                    continue;
                }

                switch (diffCurrent.Mode, diffOther.Mode)
                {
                    case (DiffMode.Update, DiffMode.Update):
                        if (diffCurrent is IDiffChildren c1 && diffOther is IDiffChildren c2)
                        {
                            if (c1.Children != null && c2.Children != null)
                            {
                                if (DetectConflict(c1.Children, c2.Children))
                                {
                                    return true;
                                }
                            
                                c1.Children.AddRange(c2.Children);
                                diffsOther.Remove(diffOther);
                            }
                        }
                        break;
                    case (DiffMode.Delete, DiffMode.Delete):
                        diffsOther.Remove(diffOther);
                        break; 
                    case (DiffMode.Add, DiffMode.Add):
                        break;
                    case (DiffMode.Move, DiffMode.Move):
                        diffsOther.Remove(diffOther);
                        break;
                    case var value:
                        throw new NotSupportedException(value.ToString());
                }
            }

            return false;
        }
        
        private CompilationUnitSyntax Apply(CompilationUnitSyntax current, List<Diff> diffUsing)
        {
            this.LogDiffs("", diffUsing);
            
            foreach (var diff in diffUsing)
            {
                current = diff switch
                {
                    MemberDeclarationDiff memberDeclarationDiff => this.ApplyOnMemberDeclaration(memberDeclarationDiff, new CompilationUnitSyntaxCombined(current), 0),
                    UsingDirectiveDiff d => current.WithUsings(this.ApplyInsertOrDeleteDiff(current.Usings.To(), d, 0)),
                    var d => throw new NotSupportedException(d.ToString())
                };
            }

            return current;
        }

        private void LogDiffs(string parentDiff, List<Diff> diffUsing)
        {
            foreach (var diff in diffUsing)
            {
                if (diff is IDiffChildren { Children: { } } parent && parent.Children.Any())
                {
                    LogDiffs(parentDiff + " > " + parent, parent.Children);
                }
                else
                {
                    this.logger.LogInformation("{diffUsing}", parentDiff + " > " + diff);
                }
            }
        }

        private T2 ApplyOnMemberDeclaration<T2>(MemberDeclarationDiff memberDeclarationDiff, IMemberCombined<T2> current, int i)
        {
            if (memberDeclarationDiff is { Mode : DiffMode.Move })
            {
                var m = current.Members[memberDeclarationDiff.IndexOfChild];

                var members = current.Members.RemoveAt(memberDeclarationDiff.IndexOfChild);

                members = members.Insert(memberDeclarationDiff.MoveIndexOfChild, m);
                
                return current.WithMembers(members);
            }
            
            if (memberDeclarationDiff is not { Mode : DiffMode.Update })
            {
                return current.WithMembers(this.ApplyInsertOrDeleteDiff(current.Members.To(), memberDeclarationDiff, i));
            }
            
            var baseMember = current.Members[memberDeclarationDiff.IndexOfChild];
            var newMember = baseMember;
            var children = memberDeclarationDiff.Children ?? new List<Diff>();
            foreach (var child in children)
            {
                var index = children.Sum(d =>
                {
                    if (d.Mode == DiffMode.Delete && d.IndexOfChild < child.IndexOfChild)
                    {
                        return -1;
                    }
                    if (d.Mode == DiffMode.Add && d.IndexOfChild < child.IndexOfChild)
                    {
                        return 1;
                    }

                    return 0;
                });

                newMember = (child, newMember) switch
                {
                    (MemberDeclarationDiff d, { } m) =>  this.ApplyOnMemberDeclaration(d, this.GetMemberCombined(m), index),
                    (StatementDiff d, MethodDeclarationSyntax m) => this.ApplyStatementDiffOnMethodDeclaration(d, m, index),
                    (MethodReturnTypeDiff d, MethodDeclarationSyntax methodDeclarationSyntax) => methodDeclarationSyntax.WithReturnType(d.Value),
                    (EqualsValueClauseDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplyEqualsValueClauseDiffOnPropertyDeclaration(d, propertyDeclarationSyntax),
                    (SemicolonTokenDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplySemicolonTokenDiffOnPropertyDeclaration(d, propertyDeclarationSyntax),
                    (AccessorListDiff d, PropertyDeclarationSyntax propertyDeclarationSyntax) => this.ApplyAccessorListSyntaxOnPropertyDeclaration(d, propertyDeclarationSyntax),
                    (TokenDiff diff, NamespaceDeclarationSyntax d) => this.ApplyTokenDiffOnNamespaceDeclarationSyntax(diff, d),
                    (TokenDiff diff, ClassDeclarationSyntax d) => this.ApplyTokenDiffOnTypeDeclarationSyntax(diff, d),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return current.WithMembers(current.Members.Replace(baseMember, newMember));
        }

        private MemberDeclarationSyntax ApplyTokenDiffOnTypeDeclarationSyntax(TokenDiff diff, TypeDeclarationSyntax classDeclarationSyntax)
        {
            if (diff.Mode == DiffMode.Update)
            {
                if (diff.Children != null)
                {
                    foreach (var child in diff.Children)
                    {
                        classDeclarationSyntax = (classDeclarationSyntax, diff.TokenDiffEnum) switch
                        {
                            (var n, TokenDiffEnum.CloseBrace) => n.WithCloseBraceToken(Apply(child, n.CloseBraceToken)),
                            (var n, TokenDiffEnum.OpenBrace) => n.WithOpenBraceToken(Apply(child, n.OpenBraceToken)),
                            var d => throw NotSupportedExceptions.Value(d)
                        };
                    }
                }

                return classDeclarationSyntax;
            }
            
            throw NotSupportedExceptions.Value(classDeclarationSyntax);
        }

        private MemberDeclarationSyntax ApplyTokenDiffOnNamespaceDeclarationSyntax(TokenDiff diff, NamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            if (diff.Mode == DiffMode.Update)
            {
                if (diff.Children != null)
                {
                    foreach (var child in diff.Children)
                    {
                        namespaceDeclarationSyntax = (namespaceDeclarationSyntax, diff.TokenDiffEnum) switch
                        {
                            (var n, TokenDiffEnum.CloseBrace) => n.WithCloseBraceToken(Apply(child, n.CloseBraceToken)),
                            (var n, TokenDiffEnum.OpenBrace) => n.WithOpenBraceToken(Apply(child, n.OpenBraceToken)),
                            var d => throw NotSupportedExceptions.Value(d)
                        };
                    }
                }

                return namespaceDeclarationSyntax;
            }

            throw NotSupportedExceptions.Value(namespaceDeclarationSyntax);
        }

        private SyntaxToken Apply(Diff variableDeclaratorDiff, SyntaxToken nCloseBraceToken)
        {
            switch (variableDeclaratorDiff)
            {
                case TriviaDiff t:
                    switch (t.Type)
                    {
                        case TriviaType.Trailing:
                            return nCloseBraceToken.WithTrailingTrivia(ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(nCloseBraceToken.TrailingTrivia), t, 0));
                        case TriviaType.Leading:
                            return nCloseBraceToken.WithLeadingTrivia(ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(nCloseBraceToken.LeadingTrivia), t, 0));
                        case var x : 
                            throw NotSupportedExceptions.Value(t);
                    }
                case var x : 
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
                                TriviaType.Leading => a.WithLeadingTrivia(ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(a.GetLeadingTrivia()), d, index)),
                                TriviaType.Trailing => a.WithTrailingTrivia(ApplyInsertOrDeleteDiff(new SyntaxTriviaListCombined(a.GetTrailingTrivia()), d, index)),
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
            return current.WithExpression(this.ApplyExpressionDiffOnExpressionSyntax(expressionDiff, current.Expression));
        }

        private ExpressionSyntax ApplyExpressionDiffOnExpressionSyntax(ExpressionDiff expressionDiff, ExpressionSyntax? current)
        {
            if (expressionDiff.Mode == DiffMode.Add)
            {
                return expressionDiff.Value;
            }
            
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
                
                foreach (var child in expressionDiff.Children)
                {
                    current = (child, current) switch
                    {
                        (ExpressionDiff d, ParenthesizedExpressionSyntax m) => m.WithExpression(ApplyExpressionDiffOnExpressionSyntax(d, m.Expression)),
                        (ExpressionDiff d, ExpressionSyntax m) => ApplyExpressionDiffOnExpressionSyntax(d, m),
                        (ArgumentDiff d, InvocationExpressionSyntax m) => ApplyArgumentDiffOnInvocationExpressionSyntax(d, m),
                        (NameMemberAccessExpressionDiff d, MemberAccessExpressionSyntax m) => m.WithName(d.Value),
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

        private ExpressionSyntax ApplyArgumentDiffOnInvocationExpressionSyntax(ArgumentDiff diff, InvocationExpressionSyntax current)
        {
            var l = current.ArgumentList.Arguments;

            l = ApplyInsertOrDeleteDiff(l.To(), diff, 0);

            return current.WithArgumentList(current.ArgumentList.WithArguments(l));
        }

        private StatementSyntax ApplyVariableDeclaratorDiffOnStatementSyntax(VariableDeclaratorDiff memberDeclarationDiff, StatementSyntax current)
        {
            if (memberDeclarationDiff is not { Mode : DiffMode.Update })
            {
                return current switch
                {
                    LocalDeclarationStatementSyntax d => d
                        .WithDeclaration(
                            d.Declaration
                                .WithVariables(this.ApplyInsertOrDeleteDiff(d.Declaration.Variables.To(), memberDeclarationDiff, 0))),
                    IfStatementSyntax i => i.WithStatement(this.ApplyVariableDeclaratorDiffOnStatementSyntax(memberDeclarationDiff, i.Statement)),
                    BlockSyntax i => i.WithStatements(this.Apply(memberDeclarationDiff, i.Statements)),
                    var value => throw NotSupportedExceptions.Value(value)
                };
            }
            
            foreach (var child in memberDeclarationDiff.Children)
            {
                current = (child, current) switch
                {
                    (ExpressionDiff d, IfStatementSyntax m) => ApplyExpressionDiffOnIfStatement(d, m),
                    (ExpressionDiff d, ReturnStatementSyntax m) => ApplyExpressionDiffOnReturnStatement(d, m),
                    (ExpressionDiff d, LocalDeclarationStatementSyntax m) => ApplyExpressionDiffOnLocalDeclarationStatementSyntax(d, m),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return current;
        }

        private StatementSyntax ApplyExpressionDiffOnLocalDeclarationStatementSyntax(ExpressionDiff diff, LocalDeclarationStatementSyntax current)
        {
            throw NotSupportedExceptions.Value(diff);
            // return current.wi
        }

        private StatementSyntax ApplyExpressionDiffOnIfStatement(ExpressionDiff expressionDiff, IfStatementSyntax ifStatementSyntax)
        {
            return ifStatementSyntax.WithCondition(ApplyExpressionDiffOnExpressionSyntax(expressionDiff, ifStatementSyntax.Condition));
        }

        private SyntaxList<StatementSyntax> Apply(VariableDeclaratorDiff variableDeclaratorDiff, SyntaxList<StatementSyntax> blockSyntaxStatements)
        {
            var member = blockSyntaxStatements[variableDeclaratorDiff.IndexOfChild];

            var val = this.ApplyVariableDeclaratorDiffOnStatementSyntax(variableDeclaratorDiff, member);

            return blockSyntaxStatements.Replace(member, val);
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
        
        private MemberDeclarationSyntax ApplyStatementDiffOnMethodDeclaration(StatementDiff diff, MethodDeclarationSyntax current, int index)
        {
            var currentBody = current.Body;

            if (diff is { Mode: DiffMode.Move })
            {
                if (current.Body == null)
                {
                    return current;
                }
                
                var m = current.Body.Statements[diff.IndexOfChild];

                var statements = current.Body.Statements.RemoveAt(diff.IndexOfChild);

                statements = statements.Insert(diff.MoveIndexOfChild, m);
                
                return current.WithBody(current.Body.WithStatements(statements));
            }

            if (currentBody == null)
            {
                return current;
            }
            
            if (diff is not { Mode: DiffMode.Update })
            {
                return current.WithBody(currentBody.WithStatements(this.ApplyInsertOrDeleteDiff(currentBody.Statements.To(), diff, index)));
            }

            var baseMember = currentBody.Statements[diff.IndexOfChild];

            var newMember = this.ApplyStatementDiffOnStatementSyntax(diff, baseMember);

            return current.WithBody(currentBody.WithStatements(currentBody.Statements.Replace(baseMember, newMember)));
        }

        private StatementSyntax ApplyStatementDiffOnStatementSyntax(StatementDiff diff, StatementSyntax baseMember) 
        {
            if(diff is not {Mode: DiffMode.Update})
            {
                throw new NotImplementedException();
            }
            
            var newMember = baseMember;
            var children = diff.Children ?? new List<Diff>();
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
                
                newMember = (child, newMember) switch
                {
                    (StatementDiff b, IfStatementSyntax ifStatementSyntax) => ifStatementSyntax.WithStatement(this.ApplyStatementDiffOnStatementSyntax(b, ifStatementSyntax.Statement)),
                    (StatementDiff b, BlockSyntax blockSyntax) => this.ApplyStatementDiffOnBlockStatementSyntax(b, blockSyntax, index),
                    (VariableDeclarationDiff d, IfStatementSyntax ifStatementSyntax) => this.ApplyVariableDeclarationDiffOnStatementSyntax(d, ifStatementSyntax),
                    (VariableDeclarationDiff d, LocalDeclarationStatementSyntax ifStatementSyntax) => ifStatementSyntax.WithDeclaration(this.ApplyVariableDeclarationDiffOnVariableDeclaration(d, ifStatementSyntax.Declaration)),
                    (ExpressionDiff d, ReturnStatementSyntax returnStatementSyntax) => this.ApplyExpressionDiffOnReturnStatement(d, returnStatementSyntax),
                    (StatementDiff b, LocalDeclarationStatementSyntax s)=> this.ApplyStatementDiffOnLocalDeclarationSyntax(b, s),
                    (StatementDiff b, ReturnStatementSyntax s)=> this.ApplyStatementDiffOnReturnStatementSyntax(b, s),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return newMember;
        }

        private StatementSyntax ApplyStatementDiffOnReturnStatementSyntax(StatementDiff diff, ReturnStatementSyntax current)
        {
            if(diff is {Mode: DiffMode.Move})
            {
                throw new NotImplementedException();
            }
            
            if(diff is not {Mode: DiffMode.Update})
            {
                throw new NotImplementedException();
            }
            
            StatementSyntax newMember = current;
            foreach (var child in diff.Children ?? new List<Diff>())
            {
                newMember = (child) switch
                {
                    (ExpressionDiff d) => ApplyExpressionDiffOnReturnStatement(d, current),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return newMember;
        }

        private StatementSyntax ApplyStatementDiffOnLocalDeclarationSyntax(StatementDiff diff, LocalDeclarationStatementSyntax current)
        {
            StatementSyntax newMember = current;
            foreach (var child in diff.Children ?? new List<Diff>())
            {
                newMember = (child) switch
                {
                    (StatementDiff d)=> ApplyStatementDiffOnLocalDeclarationSyntax(d, current),
                    (VariableDeclarationDiff d) => current.WithDeclaration(this.ApplyVariableDeclarationDiffOnVariableDeclaration(d, current.Declaration)),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return newMember;
        }

        private VariableDeclarationSyntax ApplyVariableDeclarationDiffOnVariableDeclaration(VariableDeclarationDiff diff, VariableDeclarationSyntax current)
        {
            var newMember = current;
            foreach (var child in diff.Children)
            {
                newMember = (child, newMember) switch
                {
                    (VariableDeclaratorDiff d, VariableDeclarationSyntax m) =>  this.ApplyVariableDeclaratorDiffOnVariableDeclarationSyntax(d, m) , 
                    (VariableDeclarationTypeDiff d, VariableDeclarationSyntax m) => m.WithType(d.DeclarationType),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return newMember;
        }

        private VariableDeclarationSyntax ApplyVariableDeclaratorDiffOnVariableDeclarationSyntax(VariableDeclaratorDiff diff, VariableDeclarationSyntax current)
        {
            if (diff is not { Mode: DiffMode.Update })
            {
                return current.WithVariables(ApplyInsertOrDeleteDiff(current.Variables.To(), diff, 0));
            }

            var baseMember = current.Variables[diff.IndexOfChild];

            var newMember = this.ApplyVariableDeclaratorDiffOnVariableDeclaratorSyntax(diff, baseMember);

            return current.WithVariables(current.Variables.Replace(baseMember, newMember));
        }

        private VariableDeclaratorSyntax ApplyVariableDeclaratorDiffOnVariableDeclaratorSyntax(VariableDeclaratorDiff diff, VariableDeclaratorSyntax current)
        {
            var newMember = current;
            foreach (var child in diff.Children)
            {
                newMember = (child, newMember) switch
                {
                    (ExpressionDiff d, VariableDeclaratorSyntax m) => m.Initializer == null ? m : m.WithInitializer(m.Initializer.WithValue(ApplyExpressionDiffOnExpressionSyntax(d, m.Initializer.Value))),
                    (IdentifierDiff d, VariableDeclaratorSyntax m) => m.WithIdentifier(d.Value),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return newMember;
        }

        private StatementSyntax ApplyStatementDiffOnBlockStatementSyntax(StatementDiff diff, BlockSyntax current, int index)
        {
            if (diff is not { Mode: DiffMode.Update })
            {
                return current.WithStatements(ApplyInsertOrDeleteDiff(current.Statements.To(), diff, index));
            }

            var baseMember = current.Statements[diff.IndexOfChild];

            var newMember = this.ApplyStatementDiffOnStatementSyntax(diff, baseMember);

            return current.WithStatements(current.Statements.Replace(baseMember, newMember));
        }

        private StatementSyntax ApplyVariableDeclarationDiffOnStatementSyntax(VariableDeclarationDiff variableDeclaratorTypeDiff, StatementSyntax current)
        {
            if (variableDeclaratorTypeDiff is not { Mode : DiffMode.Update })
            {
                return current;
            }

            foreach (var child in variableDeclaratorTypeDiff.Children)
            {
                current = (child, current) switch
                {
                    (VariableDeclaratorDiff d, _) => this.ApplyVariableDeclaratorDiffOnStatementSyntax(d, current),
                    (VariableDeclarationTypeDiff d, LocalDeclarationStatementSyntax syntax) => syntax.WithDeclaration(syntax.Declaration.WithType(d.DeclarationType)),
                    var d => throw NotSupportedExceptions.Value(d)
                };
            }

            return current;
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
        
        private CompilationUnitSyntax ParseFile(string file)
        {
            var fileContent = File.ReadAllText(file); 
            SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
            return tree.GetCompilationUnitRoot();
        }

        public string ProviderCode => "csharp";
    }
}