using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neocra.GitMerge.Csharp
{
    public interface IMemberCombined<out TParent> : IList<TParent, SyntaxList<MemberDeclarationSyntax>, MemberDeclarationSyntax>
    {
        
    }

    public interface IList<out TParent, TListChild, TChild>
        where TListChild: IReadOnlyCollection<TChild>
    {
        TListChild Members { get; }

        TParent WithMembers(TListChild members);
        
        TParent WithMembers(IEnumerable<TChild> members);
    }
}