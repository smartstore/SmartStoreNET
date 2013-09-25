using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Specialized;
using SmartStore.Collections;

namespace SmartStore
{

    public static class EnumerableExtensions
    {

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
        //[DebuggerStepThrough]
        public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T t in source)
            {
                action(t);
            }
        }

        ///// <summary>
        ///// Performs an action on each item while iterating through a list. 
        ///// This is a handy shortcut for <c>foreach(item in list) { ... }</c>
        ///// </summary>
        ///// <typeparam name="T">The type of the items.</typeparam>
        ///// <param name="source">The enumerator instance that this extension operates on.</param>
        ///// <param name="action">The action delegate which is called on each item while iterating.</param>
        //public static void Each<T>(this IEnumerator<T> source, Action<T> action)
        //{
        //    while (source.MoveNext())
        //        action(source.Current);
        //}

        /// <summary>
        /// Casts the objects within a list into another type.
        /// </summary>
        /// <typeparam name="T">The type of the source objects.</typeparam>
        /// <typeparam name="U">The target type of the objects.</typeparam>
        /// <param name="source">The list, which holds the objects.</param>
        /// <param name="converter">The delegate function which is responsible for converting each object.</param>
        public static IEnumerable<TTarget> Transform<TSource, TTarget>(this IEnumerable<TSource> source, Converter<TSource, TTarget> converter)
        {
            foreach (TSource s in source)
                yield return converter(s);
        }

        /// <summary>
        /// Shorthand extension method for converting enumerables into the arrays
        /// </summary>
        /// <typeparam name="TSource">The type of the source array.</typeparam>
        /// <typeparam name="TTarget">The type of the target array.</typeparam>
        /// <param name="self">The collection to convert.</param>
        /// <param name="converter">The converter.</param>
        /// <returns>target array instance</returns>
        public static TTarget[] ToArray<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, TTarget> converter)
        {
            Guard.ArgumentNotNull(() => source);
            Guard.ArgumentNotNull(() => converter);

            return source.Select(converter).ToArray();
        }

        public static bool Exists<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            return source.Count(func) > 0;
        }

        public static IEnumerable<T> CastValid<T>(this IEnumerable source)
        {
            return source.Cast<object>().Where(o => o is T).Cast<T>();
        }

        public static bool HasItems(this IEnumerable source)
        {
            return source != null && source.GetEnumerator().MoveNext();
        }

        public static int GetCount(this IEnumerable source)
        {
            return source.AsQueryable().GetCount();
        }

        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            return (source == null || !source.HasItems());
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source)
        {
            return new ReadOnlyCollection<T>(source.ToList());
        }

        public static IEnumerable<T> OrderByOrdinal<T>(this IEnumerable<T> source)
            where T : IOrdered
        {
            return source.OrderByOrdinal(false);
        }

        public static IEnumerable<T> OrderByOrdinal<T>(this IEnumerable<T> source, bool descending)
            where T : IOrdered
        {
            if (!descending)
                return source.OrderBy(x => x.Ordinal);
            else
                return source.OrderByDescending(x => x.Ordinal);
        }

        public static bool TryGetItem<T>(this IEnumerable<T> source, Expression<Func<T, bool>> predicate, out T item)
        {
            item = default(T);

            try
            {
                item = source.AsQueryable().First(predicate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Multimap

        public static Multimap<TKey, TValue> ToMultimap<TSource, TKey, TValue>(
                                                this IEnumerable<TSource> source,
                                                Func<TSource, TKey> keySelector,
                                                Func<TSource, TValue> valueSelector)
        {
            Guard.ArgumentNotNull(() => source);
            Guard.ArgumentNotNull(() => keySelector);
            Guard.ArgumentNotNull(() => valueSelector);

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
            Guard.ArgumentNotNull(initial, "initial");
            if (other == null)
                return;

            foreach (var item in other.AllKeys)
            {
                initial.Add(item, other[item]);
            }
        }

        #endregion

        #region AsSerializable

        /// <summary>
        /// Convenience API to allow an IEnumerable[T] (such as returned by Linq2Sql, NHibernate, EF etc.) 
        /// to be serialized by DataContractSerializer.
        /// </summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="source">The original collection.</param>
        /// <returns>A serializable enumerable wrapper.</returns>
        public static IEnumerable<T> AsSerializable<T>(this IEnumerable<T> source) where T : class
        {
            return new IEnumerableWrapper<T>(source);
        }

        /// <summary>
        /// This wrapper allows IEnumerable<T> to be serialized by DataContractSerializer.
        /// It implements the minimal amount of surface needed for serialization.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        class IEnumerableWrapper<T> : IEnumerable<T>
            where T : class
        {
            IEnumerable<T> _collection;

            // The DataContractSerilizer needs a default constructor to ensure the object can be
            // deserialized. We have a dummy one since we don't actually need deserialization.
            public IEnumerableWrapper()
            {
                throw new NotImplementedException();
            }

            internal IEnumerableWrapper(IEnumerable<T> collection)
            {
                this._collection = collection;
            }

            // The DataContractSerilizer needs an Add method to ensure the object can be
            // deserialized. We have a dummy one since we don't actually need deserialization.
            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this._collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this._collection).GetEnumerator();
            }
        }

        #endregion
    }

}
