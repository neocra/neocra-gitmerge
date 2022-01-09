using Microsoft.CodeAnalysis;

namespace Neocra.GitMerge.Csharp
{
    public static class SyntaxListExtensions
    {
        public static ISyntaxList<T, SyntaxList<T>> To<T>(this SyntaxList<T> list) where T : SyntaxNode
        {
            return new SyntaxListCombined<T>(list);
        }
        
        public static ISyntaxList<T, SeparatedSyntaxList<T>> To<T>(this SeparatedSyntaxList<T> list) where T : SyntaxNode
        {
            return new SeparatedSyntaxListCombined<T>(list);
        }
    }
}