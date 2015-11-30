using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public class PagedList<T> : List<T>, IPagedList<T> 
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            Guard.ArgumentNotNull(source, "source");

			if (pageIndex == 0 && pageSize == int.MaxValue)		// avoid unnecessary SQL
				Init(source, pageIndex, pageSize, source.Count());
			else
				Init(source.Skip(pageIndex * pageSize).Take(pageSize), pageIndex, pageSize, source.Count());
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            Guard.ArgumentNotNull(source, "source");

            Init(source.Skip(pageIndex * pageSize).Take(pageSize), pageIndex, pageSize, source.Count);
        }
		
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Guard.ArgumentNotNull(source, "source");
            Init(source, pageIndex, pageSize, totalCount);
        }

        private void Init(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Guard.PagingArgsValid(pageIndex, pageSize,"pageIndex", "pageSize");

            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;

            this.AddRange(source);
        }

        #region IPageable Members

        // codehint: sm-add/edit

        public int PageIndex
        {
            get;
            set;
        }

        public int PageSize
        {
            get;
            set;
        }

        public int TotalCount
        {
            get;
            set;
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

    }
}
