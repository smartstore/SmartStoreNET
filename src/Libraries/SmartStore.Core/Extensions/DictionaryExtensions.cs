using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.Dynamic;
using SmartStore.Utilities;

namespace SmartStore
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> values, IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            foreach (var kvp in other)
            {
                if (values.ContainsKey(kvp.Key))
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }
                values.Add(kvp);
            }
        }

        public static void Merge(this IDictionary<string, object> instance, string key, object value, bool replaceExisting = true)
        {
            if (replaceExisting || !instance.ContainsKey(key))
            {
                instance[key] = value;
            }
        }

        public static void Merge(this IDictionary<string, object> instance, object values, bool replaceExisting = true)
        {
			instance.Merge(CommonHelper.ObjectToDictionary(values), replaceExisting);
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> instance, IDictionary<TKey, TValue> from, bool replaceExisting = true)
        {
            foreach (var kvp in from)
            {
                if (replaceExisting || !instance.ContainsKey(kvp.Key))
                {
                    instance[kvp.Key] = kvp.Value;
                }
            }
        }

        public static void AppendInValue(this IDictionary<string, object> instance, string key, string separator, object value)
        {
            instance[key] = !instance.ContainsKey(key) ? value.ToString() : (instance[key] + separator + value);
        }

        public static void PrependInValue(this IDictionary<string, object> instance, string key, string separator, object value)
        {
            instance[key] = !instance.ContainsKey(key) ? value.ToString() : (value + separator + instance[key]);
        }

		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, object> instance, TKey key)
		{
			try
			{
				object val;
				if (instance != null && instance.TryGetValue(key, out val) && val != null)
				{
					return (TValue)Convert.ChangeType(val, typeof(TValue), CultureInfo.InvariantCulture);
				}
			}
			catch (Exception ex)
			{
				ex.Dump();
			}

			return default(TValue);
		}

        public static ExpandoObject ToExpandoObject(this IDictionary<string, object> source, bool castIfPossible = false)
        {
            Guard.ArgumentNotNull(source, "source");

            if (castIfPossible && source is ExpandoObject)
            {
                return source as ExpandoObject;
            }

            var result = new ExpandoObject();
            result.AddRange(source);

            return result;
        }

    }

}
