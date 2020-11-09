using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class ConcurrentMultimap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, SyncedCollection<TValue>>>
    {
        private readonly ConcurrentDictionary<TKey, SyncedCollection<TValue>> _dict;
        private readonly Func<IEnumerable<TValue>, ICollection<TValue>> _collectionCreator;

        public ConcurrentMultimap()
            : this(null, null, null)
        {
        }

        public ConcurrentMultimap(IEqualityComparer<TKey> comparer)
            : this(null, comparer, null)
        {
        }

        public ConcurrentMultimap(IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> items)
            : this(items, null, null)
        {
            // for serialization
        }

        public ConcurrentMultimap(
            IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> items,
            IEqualityComparer<TKey> comparer)
            : this(items, comparer, null)
        {
            // for serialization
        }

        public ConcurrentMultimap(
            IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> items,
            IEqualityComparer<TKey> comparer,
            Func<IEnumerable<TValue>, ICollection<TValue>> collectionCreator)
        {
            _collectionCreator = collectionCreator;
            _dict = new ConcurrentDictionary<TKey, SyncedCollection<TValue>>(
                ConvertItems(items),
                comparer ?? EqualityComparer<TKey>.Default);
        }

        private IEnumerable<KeyValuePair<TKey, SyncedCollection<TValue>>> ConvertItems(IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> items)
        {
            if (items == null)
                yield break;

            foreach (var item in items)
            {
                yield return new KeyValuePair<TKey, SyncedCollection<TValue>>(item.Key, CreateCollection(item.Value));
            }
        }

        protected virtual SyncedCollection<TValue> CreateCollection(IEnumerable<TValue> values)
        {
            var creator = _collectionCreator ?? Multimap<TKey, TValue>.DefaultCollectionCreator;
            var col = creator(values ?? Enumerable.Empty<TValue>());

            return col.AsSynchronized();
        }

        /// <summary>
        /// Gets the count of groups/keys.
        /// </summary>
        public int Count => _dict.Keys.Count;

        /// <summary>
        /// Gets the total count of items in all groups.
        /// </summary>
        public int TotalValueCount => this._dict.Values.Sum(x => x.Count);

        /// <summary>
        /// Gets the count of items in the requested group.
        /// </summary>
        public int ValueCount(TKey key)
        {
            if (_dict.TryGetValue(key, out var col))
            {
                return col.Count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the collection of values stored under the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public virtual SyncedCollection<TValue> this[TKey key]
        {
            get
            {
                GetOrCreateValues(key, null, out var col);
                return col;
            }
        }

        /// <summary>
        /// Gets the collection of keys.
        /// </summary>
        public virtual ICollection<TKey> Keys => _dict.Keys;

        /// <summary>
        /// Gets all value collections.
        /// </summary>
        public virtual ICollection<SyncedCollection<TValue>> Values => _dict.Values;

        /// <summary>
        /// Attempts to add the specified value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public virtual void TryAdd(TKey key, TValue value)
        {
            if (!GetOrCreateValues(key, new[] { value }, out var col))
            {
                col.Add(value);
            }
        }

        /// <summary>
        /// Attempts to add the specified values to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public virtual void TryAddRange(TKey key, IEnumerable<TValue> values)
        {
            Guard.NotNull(values, nameof(values));

            if (!GetOrCreateValues(key, values, out var col))
            {
                col.AddRange(values);
            }
        }

        /// <summary>
        /// Attempts to remove the specified value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>True</c> if such a value existed and was removed; otherwise <c>false</c>.</returns>
        public virtual bool TryRemove(TKey key, TValue value)
        {
            if (!_dict.ContainsKey(key))
                return false;

            var col = _dict[key];
            var removed = false;

            removed = col.Remove(value);
            if (col.Count == 0)
            {
                _dict.TryRemove(key, out _);
            }

            return removed;
        }

        /// <summary>
        /// Attempts to remove a range of values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values to remove from the group.</param>
        /// <returns><c>True</c> if at least one item in group <paramref name="key"/> has been removed; otherwise <c>false</c>.</returns>
        public virtual bool TryRemoveRange(TKey key, IEnumerable<TValue> values)
        {
            Guard.NotNull(values, nameof(values));

            int numRemoved = 0;

            if (_dict.TryGetValue(key, out var col))
            {
                numRemoved = col.RemoveRange(values);
            }

            return numRemoved > 0;
        }

        /// <summary>
        /// Attempts to remove and return all values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>True</c> if any such values existed; otherwise <c>false</c>.</returns>
        public virtual bool TryRemoveAll(TKey key, out SyncedCollection<TValue> collection)
        {
            return _dict.TryRemove(key, out collection);
        }

        /// <summary>
        /// Removes all values.
        /// </summary>
        public virtual void Clear()
        {
            _dict.Clear();
        }

        /// <summary>
        /// Determines whether the multimap contains any values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>True</c> if the multimap has one or more values for the specified key, otherwise <c>false</c>.</returns>
        public virtual bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the multimap contains the specified value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>True</c> if the multimap contains such a value; otherwise, <c>false</c>.</returns>
        public virtual bool ContainsValue(TKey key, TValue value)
        {
            return _dict.ContainsKey(key) && _dict[key].Contains(value);
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
        public virtual IEnumerator<KeyValuePair<TKey, SyncedCollection<TValue>>> GetEnumerator()
        {
            foreach (var pair in _dict)
                yield return pair;
        }

        private bool GetOrCreateValues(TKey key, IEnumerable<TValue> initial, out SyncedCollection<TValue> col)
        {
            // Return true when created
            var created = false;

            col = _dict.GetOrAdd(key, k =>
            {
                created = true;
                return CreateCollection(initial);
            });

            return created;
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