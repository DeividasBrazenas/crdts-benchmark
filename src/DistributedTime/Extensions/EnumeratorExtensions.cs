using System.Collections.Generic;
using System.Diagnostics;

namespace CRDT.DistributedTime.Extensions
{
    public static class EnumeratorExtensions
    {
        [DebuggerStepThrough]
        public static T? NextOrNull<T>(this IEnumerator<T> iterator) where T : struct
            => iterator.MoveNext() ? iterator.Current : null;
    }
}