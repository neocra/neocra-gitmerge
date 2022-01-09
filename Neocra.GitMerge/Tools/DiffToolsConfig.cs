using System.Collections.Generic;

namespace Neocra.GitMerge.Tools
{
    public abstract class DiffToolsConfig<T, TDiff>
        where TDiff: Diff
    {
        public abstract int Distance(TDiff delete, TDiff add);
        public abstract bool CanFusion(TDiff delete, TDiff add);
        public abstract TDiff CreateMove(TDiff delete, TDiff add);
        public abstract bool IsElementEquals(T a, T b);
        
        public abstract Diff? MakeARecursive(TDiff delete, TDiff add);

        public abstract TDiff CreateDiff(DiffMode mode, List<T> elements, int index);
    }
}