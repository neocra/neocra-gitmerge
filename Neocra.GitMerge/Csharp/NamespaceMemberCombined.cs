using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp
{
    public class NamespaceMemberCombined : IMemberCombined<NamespaceDeclarationSyntax> 
    {
        private readonly NamespaceDeclarationSyntax classDeclarationSyntax;

        public NamespaceMemberCombined(NamespaceDeclarationSyntax classDeclarationSyntax)
        {
            this.classDeclarationSyntax = classDeclarationSyntax;

        }
        public SyntaxList<MemberDeclarationSyntax> Members => this.classDeclarationSyntax.Members;
 
        public NamespaceDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
        {
            return this.classDeclarationSyntax.WithMembers(members);
        }

        public NamespaceDeclarationSyntax WithMembers(IEnumerable<MemberDeclarationSyntax> members)
        {
            return this.classDeclarationSyntax.WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));
        }

        public NamespaceDeclarationSyntax WithMembers(IReadOnlyCollection<MemberDeclarationSyntax> members)
        {
            return this.classDeclarationSyntax.WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));
        }
    }
}