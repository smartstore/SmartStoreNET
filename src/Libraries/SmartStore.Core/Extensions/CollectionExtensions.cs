using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace SmartStore
{
    
    public static class CollectionExtensions
    {

        public static void AddRange<T>(this ICollection<T> initial, IEnumerable<T> other)
        {
            if (other == null)
                return;

            var list = initial as List<T>;

            if (list != null)
            {
                list.AddRange(other);
                return;
            }

            other.Each(x => initial.Add(x));
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return (source == null || source.Count == 0);
        }

		//public static bool HasItems(this IEnumerable source)
		//{
		//	return source != null && source.GetEnumerator().MoveNext();
		//}

        public static bool EqualsAll<T>(this IList<T> a, IList<T> b)
        {
            if (a == null || b == null)
                return (a == null && b == null);

            if (a.Count != b.Count)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < a.Count; i++)
            {
                if (!comparer.Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }

    }

}
