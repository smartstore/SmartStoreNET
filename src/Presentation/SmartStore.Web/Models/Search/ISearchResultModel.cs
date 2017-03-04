using System;
using SmartStore.Services.Search;

namespace SmartStore.Web.Models.Search
{
	public interface ISearchResultModel
	{
		CatalogSearchResult SearchResult { get; }
	}
}