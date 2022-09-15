using Microsoft.CodeAnalysis;

namespace Neocra.GitMerge.Csharp;

public class SyntaxTriviaListCombined : ISyntaxList<SyntaxTrivia, SyntaxTriviaList> 
{
    private readonly SyntaxTriviaList syntaxTriviaList;

    public SyntaxTriviaListCombined(SyntaxTriviaList syntaxTriviaList)
    {
        this.syntaxTriviaList = syntaxTriviaList;
    }
            
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