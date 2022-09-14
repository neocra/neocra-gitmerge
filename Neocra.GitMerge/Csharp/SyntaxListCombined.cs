using Microsoft.CodeAnalysis;

namespace Neocra.GitMerge.Csharp
{
    public class SyntaxTriviaListCombined : ISyntaxList<SyntaxTrivia, SyntaxTriviaList> 
    {
        private readonly SyntaxTriviaList syntaxTriviaList;

        public SyntaxTriviaListCombined(SyntaxTriviaList syntaxTriviaList)
        {
            this.syntaxTriviaList = syntaxTriviaList;
        }
            
        public int Count => this.syntaxTriviaList.Count;
        public SyntaxTriviaList Insert(int index, SyntaxTrivia value)
        {
            return this.syntaxTriviaList.Insert(index, value);
        }

        public SyntaxTriviaList RemoveAt(int index)
        {
            return this.syntaxTriviaList.RemoveAt(index);
        }
        
        public SyntaxTriviaList Move(int index, int movedIndex)
        {
            var val = this.syntaxTriviaList[index];

            var list =  this.syntaxTriviaList.RemoveAt(index);

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