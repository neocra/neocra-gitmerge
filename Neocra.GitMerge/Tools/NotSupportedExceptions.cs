using System;

namespace Neocra.GitMerge.Tools
{
    public static class NotSupportedExceptions
    {
        public static NotSupportedException Value<T, T2>((T, T2) v)
        {
            return new NotSupportedException($"{v.Item1?.GetType()},{v.Item2?.GetType()} : {v}");
        }
        
        public static NotSupportedException Value<T>(T v)
        {
            return new NotSupportedException($"{v?.GetType()} : {v}");
        }
    }
}