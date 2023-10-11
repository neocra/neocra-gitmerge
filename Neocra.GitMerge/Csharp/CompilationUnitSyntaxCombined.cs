using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp
{
    public class CompilationUnitSyntaxCombined : IMemberCombined<CompilationUnitSyntax> 
    {
        private readonly CompilationUnitSyntax compilationUnitSyntax;

        public CompilationUnitSyntaxCombined(CompilationUnitSyntax compilationUnitSyntax)
        {
            this.compilationUnitSyntax = compilationUnitSyntax;

        }
        public SyntaxList<MemberDeclarationSyntax> Members => this.compilationUnitSyntax.Members;
 
        public CompilationUnitSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
        {
            return this.compilationUnitSyntax.WithMembers(members);
        }

        public CompilationUnitSyntax WithMembers(IEnumerable<MemberDeclarationSyntax> members)
        {
            return this.compilationUnitSyntax.WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));
        }
    }
}