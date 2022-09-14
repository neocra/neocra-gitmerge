using Microsoft.CodeAnalysis;

namespace Neocra.GitMerge.Csharp
{
    public class SeparatedSyntaxListCombined<T> : ISyntaxList<T, SeparatedSyntaxList<T>> 
        where T : SyntaxNode
    {
        private readonly SeparatedSyntaxList<T> separatedSyntaxList;

        public SeparatedSyntaxListCombined(SeparatedSyntaxList<T> separatedSyntaxList)
        {
            this.separatedSyntaxList = separatedSyntaxList;
        }

        public int Count => this.separatedSyntaxList.Count;

        public SeparatedSyntaxList<T> Insert(int index, T value)
        {
            return this.separatedSyntaxList.Insert(index, value);
        }

        public SeparatedSyntaxList<T> RemoveAt(int index)
        {
            return this.separatedSyntaxList.RemoveAt(index);
        }

        public SeparatedSyntaxList<T> Move(int index, int movedIndex)
        {
            var val = this.separatedSyntaxList[index];

           var list =  this.separatedSyntaxList.RemoveAt(index);

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