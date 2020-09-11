using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Collections
{
    public sealed class SyncedCollection<T> : ICollection<T>
    {
        // INFO: Don't call it SynchronizedCollection because of framework dupe.
        private readonly ICollection<T> _col;

        public SyncedCollection(ICollection<T> wrappedCollection)
            : this(wrappedCollection, new object())
        {
        }

        public SyncedCollection(ICollection<T> wrappedCollection, object syncRoot)
        {
            Guard.NotNull(wrappedCollection, nameof(wrappedCollection));
            Guard.NotNull(syncRoot, nameof(syncRoot));

            _col = wrappedCollection;
            SyncRoot = syncRoot;
        }

        public object SyncRoot { get; }

        public bool ReadLockFree { get; set; }

        public void AddRange(IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                _col.AddRange(collection);
            }
        }

        public void Insert(int index, T item)
        {
            if (_col is List<T> list)
            {
                lock (SyncRoot)
                {
                    list.Insert(index, item);
                }
            }

            throw new NotSupportedException();
        }

        public void InsertRange(int index, IEnumerable<T> values)
        {
            if (_col is List<T> list)
            {
                lock (SyncRoot)
                {
                    list.InsertRange(index, values);
                }
            }

            throw new NotSupportedException();
        }

        public int RemoveRange(IEnumerable<T> values)
        {
            int numRemoved = 0;

            lock (SyncRoot)
            {
                foreach (var value in values)
                {
                    if (_col.Remove(value))
                        numRemoved++;
                }
            }

            return numRemoved;
        }

        public void RemoveRange(int index, int count)
        {
            if (_col is List<T> list)
            {
                lock (SyncRoot)
                {
                    list.RemoveRange(index, count);
                }
            }

            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                if (_col is List<T> list)
                {
                    list.RemoveAt(index);
                }
                else
                {
                    var item = _col.ElementAtOrDefault(index);
                    if (item != null)
                    {
                        _col.Remove(item);
                    }
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (ReadLockFree)
                {
                    return _col.ElementAt(index);
                }
                else
                {
                    lock (SyncRoot)
                    {
                        return _col.ElementAt(index);
                    }
                }
            }
        }

        #region ICollection<T>

        public int Count
        {
            get
            {
                if (ReadLockFree)
                {
                    return _col.Count();
                }
                else
                {
                    lock (SyncRoot)
                    {
                        return _col.Count();
                    }
                }
            }
        }

        public bool IsReadOnly => _col.IsReadOnly;

        public void Add(T item)
        {
            lock (SyncRoot)
            {
                _col.Add(item);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                _col.Clear();
            }
        }

        public bool Contains(T item)
        {
            if (ReadLockFree)
            {
                return _col.Contains(item);
            }
            else
            {
                lock (SyncRoot)
                {
                    return _col.Contains(item);
                }
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                _col.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                return _col.Remove(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (ReadLockFree)
            {
                return _col.GetEnumerator();
            }
            else
            {
                lock (SyncRoot)
                {
                    return _col.GetEnumerator();
                }
            }
        }

        #endregion
    }
}
