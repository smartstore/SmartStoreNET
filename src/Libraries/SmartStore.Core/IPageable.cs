using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;

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
		// codehint: sm-delete (members of IPageable now)
	}

}
