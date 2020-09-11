using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SmartStore.Collections
{
    /// <summary>
    /// A data structure that contains multiple values for each key.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TValue">The type of value.</typeparam>
    [JsonConverter(typeof(MultiMapConverter))]
    [Serializable]
    public class Multimap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ICollection<TValue>>>
    {
        private readonly IDictionary<TKey, ICollection<TValue>> _dict;
        private readonly Func<IEnumerable<TValue>, ICollection<TValue>> _collectionCreator;
        private readonly bool _isReadonly = false;

        internal readonly static Func<IEnumerable<TValue>, ICollection<TValue>> DefaultCollectionCreator =
            x => new List<TValue>(x ?? Enumerable.Empty<TValue>());

        public Multimap()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        public Multimap(IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, ICollection<TValue>>(comparer ?? EqualityComparer<TKey>.Default);
            _collectionCreator = DefaultCollectionCreator;
        }

        public Multimap(Func<IEnumerable<TValue>, ICollection<TValue>> collectionCreator)
            : this(new Dictionary<TKey, ICollection<TValue>>(), collectionCreator)
        {
        }

        public Multimap(IEqualityComparer<TKey> comparer, Func<IEnumerable<TValue>, ICollection<TValue>> collectionCreator)
            : this(new Dictionary<TKey, ICollection<TValue>>(comparer ?? EqualityComparer<TKey>.Default), collectionCreator)
        {
        }

        internal Multimap(IDictionary<TKey, ICollection<TValue>> dictionary, Func<IEnumerable<TValue>, ICollection<TValue>> collectionCreator)
        {
            Guard.NotNull(dictionary, nameof(dictionary));
            Guard.NotNull(collectionCreator, nameof(collectionCreator));

            _dict = dictionary;
            _collectionCreator = collectionCreator;
        }

        protected Multimap(IDictionary<TKey, ICollection<TValue>> dictionary, bool isReadonly)
        {
            Guard.NotNull(dictionary, nameof(dictionary));

            _dict = dictionary;

            if (isReadonly && dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    dictionary[kvp.Key] = kvp.Value.AsReadOnly();
                }
            }

            _isReadonly = isReadonly;
        }

        public Multimap(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items)
            : this(items, null)
        {
            // for serialization
        }

        public Multimap(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items, IEqualityComparer<TKey> comparer)
        {
            // for serialization
            Guard.NotNull(items, nameof(items));

            _dict = new Dictionary<TKey, ICollection<TValue>>(comparer ?? EqualityComparer<TKey>.Default);

            if (items != null)
            {
                foreach (var kvp in items)
                {
                    _dict[kvp.Key] = CreateCollection(kvp.Value);
                }
            }
        }

        protected virtual ICollection<TValue> CreateCollection(IEnumerable<TValue> values)
        {
            return (_collectionCreator ?? DefaultCollectionCreator)(values ?? Enumerable.Empty<TValue>());
        }

        /// <summary>
        /// Gets the count of groups/keys.
        /// </summary>
        public int Count => this._dict.Keys.Count;

        /// <summary>
        /// Gets the total count of items in all groups.
        /// </summary>
        public int TotalValueCount => this._dict.Values.Sum(x => x.Count);

        /// <summary>
        /// Gets the collection of values stored under the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
		public virtual ICollection<TValue> this[TKey key]
        {
            get
            {
                if (!_dict.ContainsKey(key))
                {
                    if (!_isReadonly)
                        _dict[key] = CreateCollection(null);
                    else
                        return null;
                }

                return _dict[key];
            }
        }

        /// <summary>
        /// Gets the collection of keys.
        /// </summary>
        public virtual ICollection<TKey> Keys => _dict.Keys;

        /// <summary>
        /// Gets all value collections.
        /// </summary>
        public virtual ICollection<ICollection<TValue>> Values => _dict.Values;

        public IEnumerable<TValue> Find(TKey key, Func<TValue, bool> predicate)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(predicate, nameof(predicate));

            if (_dict.ContainsKey(key))
            {
                return _dict[key].Where(predicate);
            }

            return Enumerable.Empty<TValue>();
        }

        /// <summary>
        /// Adds the specified value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public virtual void Add(TKey key, TValue value)
        {
            CheckNotReadonly();

            this[key].Add(value);
        }

        /// <summary>
        /// Adds the specified values to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public virtual void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (values == null || !values.Any())
                return;

            CheckNotReadonly();

            this[key].AddRange(values);
        }

        /// <summary>
        /// Removes the specified value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>True</c> if such a value existed and was removed; otherwise <c>false</c>.</returns>
        public virtual bool Remove(TKey key, TValue value)
        {
            CheckNotReadonly();

            if (!_dict.ContainsKey(key))
                return false;

            bool result = _dict[key].Remove(value);
            if (_dict[key].Count == 0)
                _dict.Remove(key);

            return result;
        }

        /// <summary>
        /// Removes all values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>True</c> if any such values existed; otherwise <c>false</c>.</returns>
        public virtual bool RemoveAll(TKey key)
        {
            CheckNotReadonly();
            return _dict.Remove(key);
        }

        /// <summary>
        /// Removes all values.
        /// </summary>
        public virtual void Clear()
        {
            CheckNotReadonly();
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
		public virtual IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, ICollection<TValue>> pair in _dict)
                yield return pair;
        }

        private void CheckNotReadonly()
        {
            if (_isReadonly)
                throw new NotSupportedException("Multimap is read-only.");
        }

        #region Static members

        public static Multimap<TKey, TValue> CreateFromLookup(ILookup<TKey, TValue> source)
        {
            Guard.NotNull(source, nameof(source));

            var map = new Multimap<TKey, TValue>();

            foreach (IGrouping<TKey, TValue> group in source)
            {
                map.AddRange(group.Key, group);
            }

            return map;
        }

        #endregion
    }

    public class MultiMapConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var canConvert = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Multimap<,>);
            return canConvert;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // typeof TKey
            var keyType = objectType.GetGenericArguments()[0];

            // typeof TValue
            var valueType = objectType.GetGenericArguments()[1];

            // typeof IEnumerable<KeyValuePair<TKey, ICollection<TValue>>
            var sequenceType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(keyType, typeof(IEnumerable<>).MakeGenericType(valueType)));

            // serialize JArray to sequenceType
            var list = serializer.Deserialize(reader, sequenceType);

            if (keyType == typeof(string))
            {
                // call constructor Multimap(IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> items, IEqualityComparer<TKey> comparer)
                // TBD: we always assume string keys to be case insensitive. Serialize it somehow and fetch here!
                return Activator.CreateInstance(objectType, new object[] { list, StringComparer.OrdinalIgnoreCase });
            }
            else
            {
                // call constructor Multimap(IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> items)
                return Activator.CreateInstance(objectType, new object[] { list });
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            {
                var enumerable = value as IEnumerable;
                foreach (var item in enumerable)
                {
                    // Json.Net uses a converter for KeyValuePair here
                    serializer.Serialize(writer, item);
                }
            }
            writer.WriteEndArray();
        }
    }
}