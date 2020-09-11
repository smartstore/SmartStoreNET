using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SmartStore.Core
{
    /// <summary>
    /// A collection of objects that has been split into pages.
    /// </summary>
    public interface IPageable : IEnumerable
    {
        /// <summary>
        /// The 0-based current page index
        /// </summary>
        [DataMember]
        int PageIndex { get; set; }

        /// <summary>
        /// The number of items in each page.
        /// </summary>
        [DataMember]
        int PageSize { get; set; }

        /// <summary>
        /// The total number of items.
        /// </summary>
        [DataMember]
        int TotalCount { get; set; }


        /// <summary>
        /// The 1-based current page index
        /// </summary>
        [DataMember]
        int PageNumber { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        [DataMember]
        int TotalPages { get; }

        /// <summary>
        /// Whether there are pages before the current page.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Whether there are pages after the current page.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// The 1-based index of the first item in the page.
        /// </summary>
        [DataMember]
        int FirstItemIndex { get; }

        /// <summary>
        /// The 1-based index of the last item in the page.
        /// </summary>
        [DataMember]
        int LastItemIndex { get; }

        /// <summary>
        /// Whether the page is the first page
        /// </summary>
        bool IsFirstPage { get; }

        /// <summary>
        /// Whether the page is the last page
        /// </summary>
        bool IsLastPage { get; }
    }


    /// <summary>
    /// Paged list interface
    /// </summary>
    public interface IPagedList<T> : IPageable, IList<T>
    {
        /// <summary>
        /// Gets underlying query without any paging applied
        /// </summary>
        IQueryable<T> SourceQuery { get; }

        /// <summary>
        /// Allows modification of the underlying query before it is executed.
        /// </summary>
        /// <param name="alterer">The alteration function. The underlying query is passed, the modified query should be returned.</param>
        /// <returns>The current instance for chaining</returns>
        IPagedList<T> AlterQuery(Func<IQueryable<T>, IQueryable<T>> alterer);

        /// <summary>
        /// Applies the initial paging arguments to the passed query
        /// </summary>
        /// <param name="query">The query</param>
        /// <returns>A query with applied paging args</returns>
        IQueryable<T> ApplyPaging(IQueryable<T> query);

        /// <summary>
        /// Loads the data synchronously.
        /// </summary>
        /// <param name="force">When <c>true</c>, always reloads data. When <c>false</c>, first checks to see whether data has been loaded already and skips if so.</param>
        /// <returns>Returns itself for chaining.</returns>
        IPagedList<T> Load(bool force = false);

        /// <summary>
        /// Loads the data asynchronously.
        /// </summary>
        /// <param name="force">When <c>true</c>, always reloads data. When <c>false</c>, first checks to see whether data has been loaded already and skips if so.</param>
        /// <returns>Returns itself for chaining.</returns>
        Task<IPagedList<T>> LoadAsync(bool force = false);

        /// <summary>
        /// The total number of items asynchronously.
        /// </summary>
        Task<int> GetTotalCountAsync();
    }

}
