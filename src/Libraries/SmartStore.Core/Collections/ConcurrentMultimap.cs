using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace SmartStore.Collections
{
	/// <summary>
	/// A thread-safe data structure that contains multiple values for each key.
	/// </summary>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <typeparam name="TValue">The type of value.</typeparam>
	[JsonConverter(typeof(ConcurrentMultiMapConverter))]
	[Serializable]
	public class ConcurrentMultimap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, IProducerConsumerCollection<TValue>>>
	{
		private readonly ConcurrentDictionary<TKey, IProducerConsumerCollection<TValue>> _items;
		private static readonly Func<IEnumerable<TValue>, IProducerConsumerCollection<TValue>> _bagCreator = (col) => new ConcurrentBag<TValue>(col);

		public ConcurrentMultimap()
			: this(null, null)
		{
		}

		public ConcurrentMultimap(IEqualityComparer<TKey> comparer)
			: this(null, comparer)
		{
		}

		public ConcurrentMultimap(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items)
			: this(items, null)
		{
			// for serialization
		}

		public ConcurrentMultimap(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items, IEqualityComparer<TKey> comparer)
		{
			_items = new ConcurrentDictionary<TKey, IProducerConsumerCollection<TValue>>(comparer ?? EqualityComparer<TKey>.Default);

			if (items != null)
			{
				foreach (var kvp in items)
				{
					_items.TryAdd(kvp.Key, _bagCreator(kvp.Value ?? Enumerable.Empty<TValue>()));		
				}
			}
		}

		/// <summary>
		/// Gets the count of groups/keys.
		/// </summary>
		public int Count
		{
			get
			{
				return _items.Keys.Count;
			}
		}

		/// <summary>
		/// Gets the total count of items in all groups.
		/// </summary>
		public int TotalValueCount
		{
			get
			{
				return this._items.Values.Sum(x => x.Count);
			}
		}

		/// <summary>
		/// Gets the collection of values stored under the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		public virtual IProducerConsumerCollection<TValue> this[TKey key]
		{
			get
			{
				IProducerConsumerCollection<TValue> value;
				if (!_items.TryGetValue(key, out value))
				{
					_items.TryAdd(key, _bagCreator(Enumerable.Empty<TValue>()));
				}

				return value;
			}
		}

		/// <summary>
		/// Gets the collection of keys.
		/// </summary>
		public virtual ICollection<TKey> Keys
		{
			get { return _items.Keys; }
		}

		/// <summary>
		/// Gets the collection of collections of values.
		/// </summary>
		public virtual ICollection<IProducerConsumerCollection<TValue>> Values
		{
			get { return _items.Values; }
		}

		/// <summary>
		/// Attempts to add the specified value for the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public virtual void TryAdd(TKey key, TValue value)
		{
			this[key].TryAdd(value);
		}

		/// <summary>
		/// Attempts to add the specified values to the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		public virtual void TryAddRange(TKey key, IEnumerable<TValue> values)
		{
			Guard.NotNull(values, nameof(values));

			values.Each(x => this[key].TryAdd(x));
		}

		/// <summary>
		/// Attempts to remove all values for the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns><c>True</c> if any such values existed; otherwise <c>false</c>.</returns>
		public virtual bool TryRemoveAll(TKey key)
		{
			IProducerConsumerCollection<TValue> collection;
			return _items.TryRemove(key, out collection);
		}

		/// <summary>
		/// Removes all values.
		/// </summary>
		public virtual void Clear()
		{
			_items.Clear();
		}

		/// <summary>
		/// Determines whether the multimap contains any values for the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns><c>True</c> if the multimap has one or more values for the specified key, otherwise <c>false</c>.</returns>
		public virtual bool ContainsKey(TKey key)
		{
			return _items.ContainsKey(key);
		}

		/// <summary>
		/// Determines whether the multimap contains the specified value for the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns><c>True</c> if the multimap contains such a value; otherwise, <c>false</c>.</returns>
		public virtual bool ContainsValue(TKey key, TValue value)
		{
			return _items.ContainsKey(key) && _items[key].Contains(value);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the multimap.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the multimap.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the multimap.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the multimap.</returns>
		public virtual IEnumerator<KeyValuePair<TKey, IProducerConsumerCollection<TValue>>> GetEnumerator()
		{
			foreach (var pair in _items)
				yield return pair;
		}

		#region Static members

		public static ConcurrentMultimap<TKey, TValue> CreateFromLookup(ILookup<TKey, TValue> source)
		{
			Guard.NotNull(source, nameof(source));

			var map = new ConcurrentMultimap<TKey, TValue>();

			foreach (IGrouping<TKey, TValue> group in source)
			{
				map.TryAddRange(group.Key, group);
			}

			return map;
		}

		#endregion
	}

	public class ConcurrentMultiMapConverter : MultiMapConverter
	{
		public override bool CanConvert(Type objectType)
		{
			var canConvert = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ConcurrentMultimap<,>);
			return canConvert;
		}
	}
}