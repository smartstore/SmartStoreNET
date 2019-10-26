﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.Dynamic;
using SmartStore.Utilities;
using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInValue(this IDictionary<string, object> instance, string key, string separator, string value)
        {
			AddInValue(instance, key, separator, value, false);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PrependInValue(this IDictionary<string, object> instance, string key, string separator, string value)
        {
			AddInValue(instance, key, separator, value, true);
        }

		private static void AddInValue(IDictionary<string, object> instance, string key, string separator, string value, bool prepend = false)
		{
			if (!instance.TryGetValue(key, out var obj))
			{
				instance[key] = value;
			}
			else
			{
				var arr = obj.ToString().Trim().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
				var arrValue = value.Trim().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable();

				arr = prepend ? arrValue.Union(arr) : arr.Union(arrValue);

				instance[key] = string.Join(separator, arr);
			}
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> instance, TKey key)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));

			instance.TryGetValue(key, out var val);
			return val;
		}

		public static ExpandoObject ToExpandoObject(this IDictionary<string, object> source, bool castIfPossible = false)
        {
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			if (castIfPossible && source is ExpandoObject)
            {
                return source as ExpandoObject;
            }

            var result = new ExpandoObject();
            result.AddRange(source);

            return result;
        }


		public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value, bool updateIfExists = false)
		{
			if (source == null || key == null)
			{
				return false;
			}

			if (source.ContainsKey(key))
			{
				if (updateIfExists)
				{
					source[key] = value;
					return true;
				}
			}
			else
			{
				source.Add(key, value);
				return true;
			}

			return false;
		}

		public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, out TValue value)
		{
			value = default(TValue);

			if (source != null && key != null && source.TryGetValue(key, out value))
			{
				source.Remove(key);
				return true;
			}

			return false;
		}
    }

}
