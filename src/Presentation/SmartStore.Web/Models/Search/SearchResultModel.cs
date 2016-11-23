using System;
using System.Collections.Generic;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Models.Search
{
	public class SearchResultModel : ModelBase
	{
		public SearchResultModel()
		{
			TopProducts = new List<ProductOverviewModel>();
			HitGroups = new List<HitGroup>();
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

		public IList<HitGroup> HitGroups
		{
			get;
			private set;
		}

		#region Nested classes

		public class HitGroup : IOrdered
		{
			public HitGroup()
			{
				Hits = new List<HitItem>();
			}

			public string Name { get; set; }
			public string DisplayName { get; set; }
			public int Ordinal { get; set; }
			public IList<HitItem> Hits { get; private set; }
		}

		public class HitItem
		{
			public string Label { get; set; }
			public string Url { get; set; }
			public int? HitCount { get; set; }
			public object Custom { get; set; }
			public bool NoHighlight { get; set; }
		}

		#endregion
	}
}