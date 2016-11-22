using System;
using System.Collections.Generic;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Models.Search
{
	public class InstantSearchResultModel : ModelBase
	{
		public InstantSearchResultModel()
		{
			TopProducts = new List<ProductOverviewModel>();
		}

		public CatalogSearchResult SearchResult
		{
			get;
			set;
		}

		public string Term
		{
			get;
			set;
		}

		public IList<ProductOverviewModel> TopProducts
		{
			get;
			private set;
		}

		public int TotalProductsCount
		{
			get;
			set;
		}
	}
}