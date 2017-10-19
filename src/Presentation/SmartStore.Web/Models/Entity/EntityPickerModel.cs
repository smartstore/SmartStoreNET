using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Entity
{
	public class EntityPickerModel : ModelBase
	{
		public string AllString { get; set; }
		public string PublishedString { get; set; }
		public string UnpublishedString { get; set; }

		public string Entity { get; set; }
		public bool HighligtSearchTerm { get; set; }
		public string DisableIf { get; set; }
		public string DisableIds { get; set; }
		public string SearchTerm { get; set; }
		public string ReturnField { get; set; }
		public int MaxReturnValues { get; set; }
		public int PageIndex { get; set; }
		public int PageSize { get; set; }

		public List<SearchResultModel> SearchResult { get; set; }

		#region Products

		[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
		public string ProductName { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
		public int CategoryId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
		public int ManufacturerId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
		public int StoreId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
		public int ProductTypeId { get; set; }

		public IList<SelectListItem> AvailableCategories { get; set; }
		public IList<SelectListItem> AvailableManufacturers { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
		public IList<SelectListItem> AvailableProductTypes { get; set; }

		#endregion

		public class SearchResultModel : EntityModelBase
		{
			public string ReturnValue { get; set; }
			public string Title { get; set; }
			public string Summary { get; set; }
			public string SummaryTitle { get; set; }
			public bool? Published { get; set; }
			public bool Disable { get; set; }
			public string ImageUrl { get; set; }
			public string LabelText { get; set; }
			public string LabelClassName { get; set; }
		}
	}
}