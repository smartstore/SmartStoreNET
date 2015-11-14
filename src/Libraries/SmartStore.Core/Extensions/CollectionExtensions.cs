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
    }
}
