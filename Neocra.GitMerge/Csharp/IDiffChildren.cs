using System.Collections.Generic;

namespace Neocra.GitMerge.Csharp
{
    public interface IDiffChildren
    {
        List<Diff>? Children { get; }
    }
}