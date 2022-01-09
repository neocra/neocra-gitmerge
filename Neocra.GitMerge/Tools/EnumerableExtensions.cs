using System;
using System.Collections.Generic;

namespace Neocra.GitMerge.Tools
{
    public static class EnumerableExtensions
    {
        public static int? GetIndexOf<T>(this List<T> nodes, T node, Func<T, T, bool> isEquals)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (isEquals(node, nodes[i]))
                {
                    return i;
                }
            }

            return null;
        }
    }
}