using Microsoft.CodeAnalysis;

namespace Neocra.GitMerge.Csharp
{
    public class SyntaxListCombined<T> : ISyntaxList<T, SyntaxList<T>> 
        where T : SyntaxNode
    {
        private readonly SyntaxList<T> syntaxList;

        public SyntaxListCombined(SyntaxList<T> syntaxList)
        {
            this.syntaxList = syntaxList;
        }
            
        public int Count => this.syntaxList.Count;

        public SyntaxList<T> Insert(int index, T value)
        {
            return this.syntaxList.Insert(index, value);
        }

        public SyntaxList<T> RemoveAt(int index)
        {
            return this.syntaxList.RemoveAt(index);
        }
        
        public SyntaxList<T> Move(int index, int movedIndex)
        {
            var val = this.syntaxList[index];

            var list =  this.syntaxList.RemoveAt(index);

            if (index < movedIndex)
            {
                list = list.Insert(movedIndex - 1, val);
            }
            else
            {
                list = list.Insert(movedIndex, val);
            }

            return list;
        }
    }
}