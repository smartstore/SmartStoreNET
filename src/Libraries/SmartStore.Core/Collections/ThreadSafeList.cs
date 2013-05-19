using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using SmartStore.Utilities.Threading;

namespace SmartStore.Collections
{

    [Serializable]
    public class ThreadSafeList<T> : IList<T>, IList
    {
        #region Fields

        private List<T> _list;

        [NonSerialized]
        private readonly ReaderWriterLockSlim _rwLock;

        #endregion

        #region Ctor

        public ThreadSafeList()
            : this(new ReaderWriterLockSlim())
        {
        }

        public ThreadSafeList(ReaderWriterLockSlim rwLock)
        {
            _list = new List<T>();
            _rwLock = rwLock ?? new ReaderWriterLockSlim();
        }

        public ThreadSafeList(int capacity)
            : this(capacity, null)
        {
        }

        public ThreadSafeList(int capacity, ReaderWriterLockSlim rwLock)
        {
            _list = new List<T>(capacity);
            _rwLock = rwLock ?? new ReaderWriterLockSlim();
        }

        public ThreadSafeList(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        public ThreadSafeList(IEnumerable<T> collection, ReaderWriterLockSlim rwLock)
        {
            _list = new List<T>(collection);
            _rwLock = rwLock ?? new ReaderWriterLockSlim();
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                using (_rwLock.GetReadLock())
                {
                    return _list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                using (_rwLock.GetReadLock())
                {
                    return _list[index];
                }
            }
            set
            {
                using (_rwLock.GetWriteLock())
                {
                    _list[index] = value;
                }
            }
        }

        public ReaderWriterLockSlim Lock
        {
            get
            {
                return _rwLock;
            }
        }

        #endregion

        #region Methods

        public void Add(T item)
        {
            using (_rwLock.GetWriteLock())
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            using (_rwLock.GetWriteLock())
            {
                _list.Clear();
            }
        }

        public bool Contains(T item)
        {
            using (_rwLock.GetReadLock())
            {
                return _list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (_rwLock.GetWriteLock())
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            using (_rwLock.GetReadLock())
            {
                return _list.AsReadOnly().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            using (_rwLock.GetReadLock())
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            using (_rwLock.GetWriteLock())
            {
                _list.Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            using (_rwLock.GetWriteLock())
            {
                return _list.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            using (_rwLock.GetWriteLock())
            {
                _list.RemoveAt(index);
            }
        }

        private static void VerifyValueType(object value)
        {
            if (!IsCompatibleObject(value))
            {
                throw Error.Argument("value", "Argument '{0}' is of wrong type. It must be '{1}'.", "value", typeof(T));
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T) && ((value != null) || typeof(T).IsValueType))
            {
                return false;
            }
            return true;
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            VerifyValueType(value);
            this.Add((T)value);
            return this.Count - 1;
        }

        bool IList.Contains(object value)
        {
            return (IsCompatibleObject(value) && this.Contains((T)value));
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return this.IndexOf((T)value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            VerifyValueType(value);
            this.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                this.Remove((T)value);
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                VerifyValueType(value);
                this[index] = (T)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            using (_rwLock.GetWriteLock())
            {
                ((ICollection)_list).CopyTo(array, index);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection)_list).SyncRoot;
            }
        }

        #endregion
    }

}
