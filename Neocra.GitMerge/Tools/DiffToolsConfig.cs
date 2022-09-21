using System;
using System.Collections.Generic;

namespace Neocra.GitMerge.Tools
{
    public abstract class DiffToolsConfig<T, TDiff>
        where TDiff: Diff
    {
        public virtual int Distance(TDiff delete, TDiff add)
        {
            throw new NotSupportedException();
        }
        
        public abstract bool CanFusion(TDiff delete, TDiff add);
        public virtual TDiff CreateMove(TDiff delete, TDiff add)
        {
            throw new NotSupportedException();
        }

        public abstract bool IsElementEquals(T a, T b);
        
        public virtual Diff? MakeARecursive(TDiff delete, TDiff add)
        {
            throw new NotSupportedException();
        }


        public abstract TDiff CreateDiff(DiffMode mode, List<T> elements, int index);
    }
}