using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SmartStore
{
    public static class NameValueCollectionExtension
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            Guard.NotNull(collection, nameof(collection));

            var query = from key in collection.AllKeys
                        where key != null
                        select key;

            Func<string, string> elementSelector = key => collection[key];

            return query.ToDictionary<string, string, string>(key => key, elementSelector);
        }
    }
}
