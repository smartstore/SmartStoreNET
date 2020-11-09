using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SmartStore.Collections;

namespace SmartStore
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> initial, IEnumerable<T> other)
        {
            if (other == null)
                return;

            if (initial is List<T> list)
            {
                list.AddRange(other);
                return;
            }

            foreach (var local in other)
            {
                initial.Add(local);
            }
        }

        public static SyncedCollection<T> AsSynchronized<T>(this ICollection<T> source)
        {
            return AsSynchronized(source, new object());
        }

        public static SyncedCollection<T> AsSynchronized<T>(this ICollection<T> source, object syncRoot)
        {
            if (source is SyncedCollection<T> sc)
            {
                return sc;
            }

            return new SyncedCollection<T>(source, syncRoot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count == 0;
        }
    }
}
