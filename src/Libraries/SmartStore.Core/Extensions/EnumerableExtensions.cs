using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using SmartStore.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SmartStore.Core;

namespace SmartStore
{
	[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	public static class EnumerableExtensions
	{
		#region Nested classes

		private static class DefaultReadOnlyCollection<T>
		{
			private static ReadOnlyCollection<T> defaultCollection;

			[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
			internal static ReadOnlyCollection<T> Empty
			{
				get
				{
					if (defaultCollection == null)
					{
						defaultCollection = new ReadOnlyCollection<T>(new T[0]);
					}
					return defaultCollection;
				}
			}
		}

		#endregion

		#region IEnumerable

		private class Status
        {
            public bool EndOfSequence;
        }

        private static IEnumerable<T> TakeOnEnumerator<T>(IEnumerator<T> enumerator, int count, Status status)
        {
            while (--count > 0 && (enumerator.MoveNext() || !(status.EndOfSequence = true)))
            {
                yield return enumerator.Current;
            }
        }


        /// <summary>
        /// Slices the iteration over an enumerable by the given chunk size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="chunkSize">SIze of chunk</param>
        /// <returns>The sliced enumerable</returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items, int chunkSize = 100)
        {
            if (chunkSize < 1)
            {
                throw new ArgumentException("Chunks should not be smaller than 1 element");
            }
            var status = new Status { EndOfSequence = false };
            using (var enumerator = items.GetEnumerator())
            {
                while (!status.EndOfSequence)
                {
                    yield return TakeOnEnumerator(enumerator, chunkSize, status);
                }
            }
        }


		/// <summary>
		/// Performs an action on each item while iterating through a list. 
		/// This is a handy shortcut for <c>foreach(item in list) { ... }</c>
		/// </summary>
		/// <typeparam name="T">The type of the items.</typeparam>
		/// <param name="source">The list, which holds the objects.</param>
		/// <param name="action">The action delegate which is called on each item while iterating.</param>
		[DebuggerStepThrough]
		public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T t in source)
            {
                action(t);
            }
        }

		/// <summary>
		/// Performs an action on each item while iterating through a list. 
		/// This is a handy shortcut for <c>foreach(item in list) { ... }</c>
		/// </summary>
		/// <typeparam name="T">The type of the items.</typeparam>
		/// <param name="source">The list, which holds the objects.</param>
		/// <param name="action">The action delegate which is called on each item while iterating.</param>
		[DebuggerStepThrough]
		public static void Each<T>(this IEnumerable<T> source, Action<T, int> action)
		{
			int i = 0;
			foreach (T t in source)
			{
				action(t, i++);
			}
		}

        public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source)
        {
			if (source == null || !source.Any())
				return DefaultReadOnlyCollection<T>.Empty;

			var readOnly = source as ReadOnlyCollection<T>;
			if (readOnly != null)
			{
				return readOnly;
			}

			var list = source as List<T>;
			if (list != null) 
			{
				return list.AsReadOnly();
			}
			
			return new ReadOnlyCollection<T>(source.ToArray());
        }

		/// <summary>
		/// Converts an enumerable to a dictionary while tolerating duplicate entries (last wins)
		/// </summary>
		/// <param name="source">source</param>
		/// <param name="keySelector">keySelector</param>
		/// <returns>Result as dictionary</returns>
		public static Dictionary<TKey, TSource> ToDictionarySafe<TSource, TKey>(
			this IEnumerable<TSource> source,
			 Func<TSource, TKey> keySelector)
		{
			return source.ToDictionarySafe(keySelector, new Func<TSource, TSource>(src => src), null);
		}

		/// <summary>
		/// Converts an enumerable to a dictionary while tolerating duplicate entries (last wins)
		/// </summary>
		/// <param name="source">source</param>
		/// <param name="keySelector">keySelector</param>
		/// <param name="comparer">comparer</param>
		/// <returns>Result as dictionary</returns>
		public static Dictionary<TKey, TSource> ToDictionarySafe<TSource, TKey>(
			this IEnumerable<TSource> source,
			 Func<TSource, TKey> keySelector,
			 IEqualityComparer<TKey> comparer)
		{
			return source.ToDictionarySafe(keySelector, new Func<TSource, TSource>(src => src), comparer);
		}

		/// <summary>
		/// Converts an enumerable to a dictionary while tolerating duplicate entries (last wins)
		/// </summary>
		/// <param name="source">source</param>
		/// <param name="keySelector">keySelector</param>
		/// <param name="elementSelector">elementSelector</param>
		/// <returns>Result as dictionary</returns>
		public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(
			this IEnumerable<TSource> source,
			 Func<TSource, TKey> keySelector,
			 Func<TSource, TElement> elementSelector)
		{
			return source.ToDictionarySafe(keySelector, elementSelector, null);
		}

		/// <summary>
		/// Converts an enumerable to a dictionary while tolerating duplicate entries (last wins)
		/// </summary>
		/// <param name="source">source</param>
		/// <param name="keySelector">keySelector</param>
		/// <param name="elementSelector">elementSelector</param>
		/// <param name="comparer">comparer</param>
		/// <returns>Result as dictionary</returns>
		public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(
			this IEnumerable<TSource> source,
			 Func<TSource, TKey> keySelector,
			 Func<TSource, TElement> elementSelector,
			 IEqualityComparer<TKey> comparer)
		{
			Guard.NotNull(source, nameof(source));
			Guard.NotNull(keySelector, nameof(keySelector));
			Guard.NotNull(elementSelector, nameof(elementSelector));

			var dictionary = new Dictionary<TKey, TElement>(comparer);

			foreach (var local in source)
			{
				dictionary[keySelector(local)] = elementSelector(local);
			}

			return dictionary;
		}

		/// <summary>The distinct by.</summary>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TKey">Key type</typeparam>
		/// <returns>the unique list</returns>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
			where TKey : IEquatable<TKey>
		{
			return source.Distinct(GenericEqualityComparer<TSource>.CompareMember(keySelector));
		}


		#endregion

		#region Multimap

		public static Multimap<TKey, TValue> ToMultimap<TSource, TKey, TValue>(
                                                this IEnumerable<TSource> source,
                                                Func<TSource, TKey> keySelector,
                                                Func<TSource, TValue> valueSelector)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(keySelector, nameof(keySelector));
            Guard.NotNull(valueSelector, nameof(valueSelector));

            var map = new Multimap<TKey, TValue>();

            foreach (var item in source)
            {
                map.Add(keySelector(item), valueSelector(item));
            }

            return map;
        }

        #endregion

        #region NameValueCollection

        public static void AddRange(this NameValueCollection initial, NameValueCollection other)
        {
            Guard.NotNull(initial, "initial");

            if (other == null)
                return;

            foreach (var item in other.AllKeys)
            {
                initial.Add(item, other[item]);
            }
        }

		/// <summary>
		/// Builds an URL query string
		/// </summary>
		/// <param name="nvc">Name value collection</param>
		/// <param name="encoding">Encoding type. Can be null.</param>
		/// <param name="encode">Whether to encode keys and values</param>
		/// <returns>The query string without leading a question mark</returns>
		public static string BuildQueryString(this NameValueCollection nvc, Encoding encoding, bool encode = true)
		{
			var sb = new StringBuilder();

			if (nvc != null)
			{
				foreach (string str in nvc)
				{
					if (sb.Length > 0)
						sb.Append('&');

					if (!encode)
						sb.Append(str);
					else if (encoding == null)
						sb.Append(HttpUtility.UrlEncode(str));
					else
						sb.Append(HttpUtility.UrlEncode(str, encoding));

					sb.Append('=');

					if (!encode)
						sb.Append(nvc[str]);
					else if (encoding == null)
						sb.Append(HttpUtility.UrlEncode(nvc[str]));
					else
						sb.Append(HttpUtility.UrlEncode(nvc[str], encoding));
				}
			}

			return sb.ToString();
		}

        #endregion
    }
}
