using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections;
using System.Collections.ObjectModel;

namespace SmartStore.Core
{
    public class PagedList<T> : IPagedList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
		private IQueryable<T> _query;
		private bool _queryIsPagedAlready;

		private int _pageIndex;
		private int _pageSize;
		private int? _totalCount;

		private List<T> _list;

		/// <param name="pageIndex">The 0-based page index</param>
		public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
		{
			Guard.NotNull(source, "source");

			Init(source.AsQueryable(), pageIndex, pageSize, null);
		}

		/// <param name="pageIndex">The 0-based page index</param>
		public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Guard.NotNull(source, "source");

            Init(source.AsQueryable(), pageIndex, pageSize, totalCount);
        }

		private void Init(IQueryable<T> source, int pageIndex, int pageSize, int? totalCount)
		{
			Guard.NotNull(source, nameof(source));
			Guard.PagingArgsValid(pageIndex, pageSize, "pageIndex", "pageSize");

			_query = source;
			_queryIsPagedAlready = totalCount.HasValue;

			_pageIndex = pageIndex;
			_pageSize = pageSize;
			_totalCount = totalCount;
		}

		private void EnsureIsLoaded()
		{
			if (_list == null)
			{
				if (_totalCount == null)
				{
					_totalCount = _query.Count();
				}

				if (_queryIsPagedAlready)
				{
					_list = _query.ToList();
				}
				else
				{
					_list = ApplyPaging(_query).ToList();
				}
			}
		}

		#region IPageable Members

		public IQueryable<T> SourceQuery
		{
			get { return _query; }
		}

		public IQueryable<T> ApplyPaging(IQueryable<T> query)
		{
			if (_pageIndex == 0 && _pageSize == int.MaxValue)
			{
				// Paging unnecessary
				return query;
			}
			else
			{
				var skip = _pageIndex * _pageSize;
				if (query.Provider is IDbAsyncQueryProvider)
				{
					return query.Skip(() => skip).Take(() => _pageSize);
				}
				else
				{
					return query.Skip(skip).Take(_pageSize);
				}
			}
		}

		public IPagedList<T> Load(bool force = false)
		{
			if (force && _list != null)
			{
				_list.Clear();
				_list = null;
			}

			EnsureIsLoaded();

			return this;
		}

		public int PageIndex
        {
            get { return _pageIndex; }
            set { _pageIndex = value; }
        }

        public int PageSize
        {
			get { return _pageSize; }
			set { _pageSize = value; }
		}

        public int TotalCount
        {
            get
			{
				if (!_totalCount.HasValue)
				{
					_totalCount = _query.Count();
				}

				return _totalCount.Value;
			}
            set
			{
				_totalCount = value;
			}
        }

        public int PageNumber
        {
            get
            {
                return this.PageIndex + 1;
            }
            set
            {
                this.PageIndex = value - 1;
            }
        }

        public int TotalPages
        {
            get
            {
                var total = this.TotalCount / this.PageSize;

                if (this.TotalCount % this.PageSize > 0)
                    total++;

                return total;
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                return this.PageIndex > 0;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (this.PageIndex < (this.TotalPages - 1));
            }
        }

        public int FirstItemIndex
        {
            get
            {
                return (this.PageIndex * this.PageSize) + 1;
            }
        }

        public int LastItemIndex
        {
            get
            {
                return Math.Min(this.TotalCount, ((this.PageIndex * this.PageSize) + this.PageSize));
            }
        }

        public bool IsFirstPage
        {
            get
            {
                return (this.PageIndex <= 0);
            }
        }

        public bool IsLastPage
        {
            get
            {
                return (this.PageIndex >= (this.TotalPages - 1));
            }
        }

		#endregion

		#region IList<T> Members

		public void Add(T item)
		{
			EnsureIsLoaded();
			_list.Add(item);
		}

		public void Clear()
		{
			if (_list != null)
			{
				_list.Clear();
				_list = null;
			}
		}

		public bool Contains(T item)
		{
			EnsureIsLoaded();
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			EnsureIsLoaded();
			_list.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			if (_list != null)
			{
				return _list.Remove(item);
			}

			return false;
		}

		public int Count
		{
			get
			{
				EnsureIsLoaded();
				return _list.Count;
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public int IndexOf(T item)
		{
			EnsureIsLoaded();
			return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			EnsureIsLoaded();
			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			if (_list != null)
			{
				_list.RemoveAt(index);
			}
		}

		public T this[int index]
		{
			get
			{
				EnsureIsLoaded();
				return _list[index];
			}
			set
			{
				EnsureIsLoaded();
				_list[index] = value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			EnsureIsLoaded();
			return _list.GetEnumerator();
		}

		#endregion

		#region Utils

		public void AddRange(IEnumerable<T> collection)
		{
			EnsureIsLoaded();
			_list.AddRange(collection);
		}

		public ReadOnlyCollection<T> AsReadOnly()
		{
			EnsureIsLoaded();
			return _list.AsReadOnly();
		}

		#endregion
	}
}
