using System;
using System.Collections.Generic;
using System.Linq;

namespace Neocra.GitMerge.Tools
{
    public class DiffTools
    {
        public IEnumerable<Diff> GetDiffOfChildrenFusion<T, TDiff>(
            DiffToolsConfig<T, TDiff> diffConfig,
            List<T> childrenAncestor,
            List<T> childrenCurrent) where TDiff : Diff
        {
            var diffs = this.GetDiffOfChildren(childrenAncestor, childrenCurrent, diffConfig.CreateDiff, diffConfig.IsElementEquals)
                .ToList();

            var q = (from a in diffs
                where a.Mode == DiffMode.Delete
                let c = (from b in diffs
                    where b.Mode == DiffMode.Add
                        && diffConfig.CanFusion(a, b)
                    select new { b, compute = diffConfig.Distance(a, b) })
                select new
                {
                    a,
                    mostSimilar = c.OrderBy(b => b.compute).FirstOrDefault(),
                }).ToList();

            var newdiffs = q
                .OrderBy(c => c.mostSimilar?.compute ?? int.MaxValue)
                .ToList();

            for (var index = 0; index < newdiffs.Count; index++)
            {
                var newdiff = newdiffs[index];
                var delete = newdiff.a;

                var add = newdiff.mostSimilar?.b;

                if (add == null)
                {
                    continue;
                }
                
                if (!diffs.Contains(add))
                {
                    continue;
                }
                
                if (add.IndexOfChild != delete.IndexOfChild)
                {
                    // It's a move
                    yield return diffConfig.CreateMove(delete, add);
                }

                var diff = diffConfig.MakeARecursive(delete, add);
                if (diff != null)
                {
                    yield return diff;
                }

                diffs.Remove(delete);
                diffs.Remove(add);
            }

            foreach (var t in diffs)
            {
                yield return t;
            }
        }

        private IEnumerable<TDiff> GetDiffOfChildren<T, TDiff>(
            List<T> childrenAncestor,
            List<T> childrenCurrent,
            Func<DiffMode, List<T>, int, TDiff> createDiff,
            Func<T, T, bool> isNodeEquals)
        {
            var childs1 = GetDiffOfChildrenInternal(childrenAncestor, childrenCurrent, createDiff, isNodeEquals, DiffMode.Add, DiffMode.Delete)
                .ToList();
            var childs2 = GetDiffOfChildrenInternal(childrenCurrent, childrenAncestor, createDiff, isNodeEquals, DiffMode.Delete, DiffMode.Add)
                .ToList();

            if (childs1.Count > childs2.Count)
            {
                return childs2;
            }
            else
            {
                return childs1;
            }
        }

        private IEnumerable<TDiff> GetDiffOfChildrenInternal<T, TDiff>(
            List<T> childrenAncestor,
            List<T> childrenCurrent,
            Func<DiffMode, List<T>, int, TDiff> createDiff,
            Func<T, T, bool> isNodeEquals, DiffMode addMode, DiffMode deleteMode)
        {
            var ancestorIndex = 0;
            var currentIndex = 0;

            while (ancestorIndex < childrenAncestor.Count && currentIndex < childrenCurrent.Count)
            {
                var ancestorChild = childrenAncestor[ancestorIndex];
                var currentChild = childrenCurrent[currentIndex];

                if (isNodeEquals(ancestorChild, currentChild))
                {
                    ancestorIndex++;
                    currentIndex++;
                    continue;
                }

                int nextAncestorChildEqual = ancestorIndex;
                int? nextCurrentChildEqual = null;
                while (nextAncestorChildEqual < childrenAncestor.Count
                    && (nextCurrentChildEqual = childrenCurrent
                        .Skip(currentIndex)
                        .ToList()
                        .GetIndexOf(childrenAncestor[nextAncestorChildEqual], isNodeEquals)) == null)
                {
                    nextAncestorChildEqual++;
                }

                if (nextAncestorChildEqual < childrenAncestor.Count)
                {
                    if (nextCurrentChildEqual != null)
                    {
                        // ancestorIndex < x < nextAncestorChildEqual => delete all between 
                        // currentIndex < x < nextCurrentChildEqual => insert all between
                        foreach (var xmlDiff in this.ReturnDiffForEach(deleteMode, childrenAncestor, ancestorIndex, nextAncestorChildEqual, createDiff))
                            yield return xmlDiff;

                        foreach (var xmlDiff in this.ReturnDiffForEach(addMode, childrenCurrent, currentIndex, nextCurrentChildEqual.Value + currentIndex, createDiff))
                            yield return xmlDiff;

                        ancestorIndex = nextAncestorChildEqual;
                        currentIndex = nextCurrentChildEqual.Value + currentIndex;
                    }
                }
                else
                {
                    break;
                }
            }

            foreach (var xmlDiff in this.ReturnDiffForEach(deleteMode, childrenAncestor, ancestorIndex, childrenAncestor.Count, createDiff))
                yield return xmlDiff;

            foreach (var xmlDiff in this.ReturnDiffForEach(addMode, childrenCurrent, currentIndex, childrenCurrent.Count, createDiff))
                yield return xmlDiff;
        }

        private IEnumerable<TDiff> ReturnDiffForEach<T, TDiff>(DiffMode diffMode, List<T> childrenCurrent, int currentIndex, int childrenCurrentCount,
            Func<DiffMode, List<T>, int, TDiff> createXmlDiff)
        {
            for (var i = currentIndex; i < childrenCurrentCount; i++)
            {
                yield return createXmlDiff(diffMode, childrenCurrent, i);
            }
        }
    }
}