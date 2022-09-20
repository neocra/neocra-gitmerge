using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Neocra.GitMerge.Tools;

namespace Neocra.GitMerge.Csharp.DiffTools;

public class SyntaxTriviaListDiffToolsConfig : DiffToolsConfig<SyntaxTrivia, TriviaDiff>
{
    public TriviaType TriviaType { get; }

    public SyntaxTriviaListDiffToolsConfig(TriviaType triviaType)
    {
        this.TriviaType = triviaType;
    }
    
    public override bool CanFusion(TriviaDiff delete, TriviaDiff add)
    {
        return false;
    }
    
    public override bool IsElementEquals(SyntaxTrivia a, SyntaxTrivia b)
    {
        return a.ToFullString() == b.ToFullString();
    }
    
    public override TriviaDiff CreateDiff(DiffMode mode, List<SyntaxTrivia> elements, int index)
    {
        return new TriviaDiff(mode, index, 0, elements[index], this.TriviaType);
    }
}