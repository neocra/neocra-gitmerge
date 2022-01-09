using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp
{
    public class ClassMemberCombined : IMemberCombined<ClassDeclarationSyntax> 
    {
        private readonly ClassDeclarationSyntax classDeclarationSyntax;

        public ClassMemberCombined(ClassDeclarationSyntax classDeclarationSyntax)
        {
            this.classDeclarationSyntax = classDeclarationSyntax;

        }
        public SyntaxList<MemberDeclarationSyntax> Members => this.classDeclarationSyntax.Members;
 
        public ClassDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
        {
            return this.classDeclarationSyntax.WithMembers(members);
        }
    }
}