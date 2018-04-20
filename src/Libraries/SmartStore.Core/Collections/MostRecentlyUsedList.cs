using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Collections
{
	public class MostRecentlyUsedList<T> : IEnumerable<T>
	{
		private readonly int _maxSize;
		private readonly List<T> _mru;

		public MostRecentlyUsedList(int maxSize)
		{
			_maxSize = maxSize;
			_mru = new List<T>();
		}

		public MostRecentlyUsedList(IEnumerable<T> collection, int maxSize)
		{
			_maxSize = maxSize;
			_mru = collection.ToList();

			Normalize();
		}

		public MostRecentlyUsedList(string collection, int maxSize)
		{
			_maxSize = maxSize;
			_mru = collection.SplitSafe(Delimiter).Cast<T>().ToList();

			Normalize();
		}

		public MostRecentlyUsedList(IEnumerable<T> collection, T newItem, int maxSize)
		{
			_maxSize = maxSize;
			_mru = collection.ToList();

			Add(newItem);
		}

		public MostRecentlyUsedList(string collection, T newItem, int maxSize)
		{
			_maxSize = maxSize;
			_mru = collection.SplitSafe(Delimiter).Cast<T>().Distinct().ToList();

			Add(newItem);
		}

		public static string Delimiter { get { return ";"; } }

		private void Normalize()
		{
			if (_maxSize >= 0)
			{
				while (_mru.Count > _maxSize)
					_mru.RemoveAt(_mru.Count - 1);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _mru.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _mru.GetEnumerator();
		}

		public override string ToString()
		{
			return string.Join(Delimiter, _mru);
		}

		public T this[int key]
		{
			get
			{
				return _mru[key];
			}
			set
			{
				_mru[key] = value;
			}
		}

		public int Count { get { return _mru.Count; } }

		public void Add(T item)
		{
			int i = _mru.IndexOf(item);
			if (i > -1)
				_mru.RemoveAt(i);

			_mru.Insert(0, item);

			Normalize();
		}
	}
}
