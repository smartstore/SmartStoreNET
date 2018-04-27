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
	public static class CollectionSlicer
	{
		/// <summary>
		/// Slices the iteration over an enumerable by the given slice sizes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source sequence to slice</param>
		/// <param name="sizes">
		/// Slice sizes. At least one size is required. Multiple sizes result in differently sized slices,
		/// whereat the last size is used for the "rest" (if any)
		/// </param>
		/// <returns>The sliced enumerable</returns>
		public static IEnumerable<IEnumerable<T>> Slice<T>(this IEnumerable<T> source, params int[] sizes)
		{
			if (!sizes.Any(step => step != 0))
			{
				throw new InvalidOperationException("Can't slice a collection with step length 0.");
			}

			return new Slicer<T>(source.GetEnumerator(), sizes).Slice();
		}
	}

	internal sealed class Slicer<T>
	{
		private readonly IEnumerator<T> _iterator;
		private readonly int[] _sizes;
		private volatile bool _hasNext;
		private volatile int _currentSize;
		private volatile int _index;

		public Slicer(IEnumerator<T> iterator, int[] sizes)
		{
			_iterator = iterator;
			_sizes = sizes;
			_index = 0;
			_currentSize = 0;
			_hasNext = true;
		}

		public int Index
		{
			get { return _index; }
		}

		public IEnumerable<IEnumerable<T>> Slice()
		{
			var length = _sizes.Length;
			var index = 1;
			var size = 0;

			for (var i = 0; _hasNext; ++i)
			{
				if (i < length)
				{
					size = _sizes[i];
					_currentSize = size - 1;
				}

				while (_index < index && _hasNext)
				{
					_hasNext = MoveNext();
				}

				if (_hasNext)
				{
					yield return new List<T>(SliceInternal());
					index += size;
				}
			}
		}

		private IEnumerable<T> SliceInternal()
		{
			if (_currentSize == -1) yield break;
			yield return _iterator.Current;

			for (var count = 0; count < _currentSize && _hasNext; ++count)
			{
				_hasNext = MoveNext();

				if (_hasNext)
				{
					yield return _iterator.Current;
				}
			}
		}

		private bool MoveNext()
		{
			++_index;
			return _iterator.MoveNext();
		}
	}

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

			if (source is ReadOnlyCollection<T> readOnly)
			{
				return readOnly;
			}
			else if (source is List<T> list) 
			{
				return list.AsReadOnly();
			}
			
			return new ReadOnlyCollection<T>(source.ToList());
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

		/// <summary>
		/// Orders a collection of entities by a specific ID sequence
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <param name="source">The entity collection to sort</param>
		/// <param name="ids">The IDs to order by</param>
		/// <returns>The sorted entity collection</returns>
		public static IEnumerable<TEntity> OrderBySequence<TEntity>(this IEnumerable<TEntity> source, IEnumerable<int> ids) where TEntity : BaseEntity
		{
			Guard.NotNull(source, nameof(source));
			Guard.NotNull(ids, nameof(ids));

			var sorted = from id in ids
						 join entity in source on id equals entity.Id
						 select entity;

			return sorted;
		}

		#endregion

		#region Multimap

		public static Multimap<TKey, TValue> ToMultimap<TSource, TKey, TValue>(
                                                this IEnumerable<TSource> source,
                                                Func<TSource, TKey> keySelector,
                                                Func<TSource, TValue> valueSelector)
        {
			return source.ToMultimap(keySelector, valueSelector, null);
        }

		public static Multimap<TKey, TValue> ToMultimap<TSource, TKey, TValue>(
												this IEnumerable<TSource> source,
												Func<TSource, TKey> keySelector,
												Func<TSource, TValue> valueSelector,
												IEqualityComparer<TKey> comparer)
		{
			Guard.NotNull(source, nameof(source));
			Guard.NotNull(keySelector, nameof(keySelector));
			Guard.NotNull(valueSelector, nameof(valueSelector));

			var map = new Multimap<TKey, TValue>(comparer);

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
