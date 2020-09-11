using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Collections
{
    public sealed class TrimmedBuffer<T> : ICollection<T>
    {
        private readonly int _maxSize;
        private readonly List<T> _list;

        public TrimmedBuffer(int maxSize)
            : this((IEnumerable<T>)null, default(T), maxSize)
        {
        }

        public TrimmedBuffer(IEnumerable<T> collection, int maxSize)
            : this(collection, default(T), maxSize)
        {
        }

        public TrimmedBuffer(string collection, int maxSize)
            : this(collection.NullEmpty()?.Convert<IEnumerable<T>>()?.Distinct(), default(T), maxSize)
        {
        }

        public TrimmedBuffer(string collection, T newItem, int maxSize)
            : this(collection.NullEmpty()?.Convert<IEnumerable<T>>()?.Distinct(), newItem, maxSize)
        {
        }

        public TrimmedBuffer(IEnumerable<T> collection, T newItem, int maxSize)
        {
            Guard.IsTrue(maxSize >= 0, nameof(maxSize));

            _maxSize = maxSize;
            _list = new List<T>(collection ?? Enumerable.Empty<T>());

            if (newItem != null)
            {
                Add(newItem);
            }

            Trim();
        }

        private void Trim()
        {
            if (_maxSize >= 0)
            {
                while (_list.Count > _maxSize)
                    _list.RemoveAt(_list.Count - 1);
            }
        }

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public void Add(T item)
        {
            var i = _list.IndexOf(item);
            if (i > -1)
                _list.RemoveAt(i);

            _list.Insert(0, item);

            Trim();
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(";", _list);
        }
    }
}
