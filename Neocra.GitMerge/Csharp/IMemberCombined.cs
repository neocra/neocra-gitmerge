using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp
{
    public interface IMemberCombined<out TParent> 
    {
        SyntaxList<MemberDeclarationSyntax> Members { get; }

        TParent WithMembers(SyntaxList<MemberDeclarationSyntax> members);
    }
}