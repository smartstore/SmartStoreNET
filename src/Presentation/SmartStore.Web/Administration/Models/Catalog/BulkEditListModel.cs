using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class BulkEditListModel : ModelBase
    {
        public BulkEditListModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
			AvailableStores = new List<SelectListItem>();
			AvailableProductTypes = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchProductName")]
        [AllowHtml]
        public string SearchProductName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
		public int SearchStoreId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
		public int SearchProductTypeId { get; set; }
		public IList<SelectListItem> AvailableProductTypes { get; set; }

		public int GridPageSize { get; set; }
        
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
    }
}