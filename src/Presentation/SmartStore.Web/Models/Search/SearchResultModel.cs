using System;
using System.Collections.Generic;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Models.Search
{
	public class SearchResultModel : ModelBase, ISearchResultModel
	{
		public SearchResultModel(CatalogSearchQuery query)
		{
			Query = query;
			HitGroups = new List<HitGroup>();
		}

		public CatalogSearchQuery Query
		{
			get;
			private set;
		}

		public CatalogSearchResult SearchResult
		{
			get;
			set;
		}

		/// <summary>
		/// Contains the original/misspelled search term, when
		/// the search did not match any results and the spell checker
		/// suggested at least one term.
		/// </summary>
		public string AttemptedTerm
		{
			get;
			set;
		}

		public string Term
		{
			get;
			set;
		}

		public ProductSummaryModel TopProducts
		{
			get;
			set;
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

		public string Error
		{
			get;
			set;
		}

		#region Nested classes

		public class HitGroup : IOrdered
		{
			public HitGroup(SearchResultModel parent)
			{
				Guard.NotNull(parent, nameof(parent));

				Parent = parent;
				Hits = new List<HitItem>();
			}

			public string Name { get; set; }
			public string DisplayName { get; set; }
			public int Ordinal { get; set; }
			public IList<HitItem> Hits { get; private set; }
			public SearchResultModel Parent { get; private set; }
		}

		public class HitItem
		{
			public string Label { get; set; }
			public string Tag { get; set; }
			public string Url { get; set; }
			public bool NoHighlight { get; set; }
		}

		#endregion
	}
}