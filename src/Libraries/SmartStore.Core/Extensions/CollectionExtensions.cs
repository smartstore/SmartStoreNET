using System;
using System.Collections.Generic;

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

        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count == 0;
        }
    }
}
