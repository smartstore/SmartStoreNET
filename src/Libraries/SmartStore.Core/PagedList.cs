using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core
{
    public abstract class PagedListBase : IPageable
    {
        protected PagedListBase()
        {
            this.PageIndex = 0;
            this.PageSize = 0;
            this.TotalCount = 1;
        }

        protected PagedListBase(IPageable pageable)
        {
            this.Init(pageable);
        }

        protected PagedListBase(int pageIndex, int pageSize, int totalItemsCount)
        {
            Guard.PagingArgsValid(pageIndex, pageSize, "pageIndex", "pageSize");

            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalCount = totalItemsCount;
        }

        public void LoadPagedList<T>(IPagedList<T> pagedList)
        {
            this.Init(pagedList);
        }

        protected void Init(IPageable pageable)
        {
            Guard.NotNull(pageable, "pageable");

            this.PageIndex = pageable.PageIndex;
            this.PageSize = pageable.PageSize;
            this.TotalCount = pageable.TotalCount;
        }

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
            get => this.PageIndex + 1;
            set => this.PageIndex = value - 1;
        }

        public int TotalPages
        {
            get
            {
                if (this.PageSize == 0)
                    return 0;

                var total = this.TotalCount / this.PageSize;

                if (this.TotalCount % this.PageSize > 0)
                    total++;

                return total;
            }
        }

        public bool HasPreviousPage => this.PageIndex > 0;

        public bool HasNextPage => (this.PageIndex < (this.TotalPages - 1));

        public int FirstItemIndex => (this.PageIndex * this.PageSize) + 1;

        public int LastItemIndex => Math.Min(this.TotalCount, ((this.PageIndex * this.PageSize) + this.PageSize));

        public bool IsFirstPage => (this.PageIndex <= 0);

        public bool IsLastPage => (this.PageIndex >= (this.TotalPages - 1));

        public virtual IEnumerator GetEnumerator()
        {
            return Enumerable.Empty<int>().GetEnumerator();
        }
    }

    public class PagedList : PagedListBase
    {
        public PagedList(int pageIndex, int pageSize, int totalItemsCount)
            : base(pageIndex, pageSize, totalItemsCount)
        {
        }

        public static PagedList<T> Create<T>(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            return new PagedList<T>(source, pageIndex, pageSize);
        }

        public static PagedList<T> Create<T>(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            return new PagedList<T>(source, pageIndex, pageSize, totalCount);
        }
    }

}
