using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Entity
{
    public class EntityPickerModel : ModelBase
    {
        public EntityPickerModel()
        {
            PageSize = 96;
        }

        public string EntityType { get; set; }
        public bool HighligtSearchTerm { get; set; }
        public string DisableIf { get; set; }
        public string DisableIds { get; set; }
        public string SearchTerm { get; set; }
        public string ReturnField { get; set; }
        public int MaxItems { get; set; }
        public string Selected { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int LanguageId { get; set; }

        public List<SearchResultModel> SearchResult { get; set; }

        #region Search properties

        [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
        public string ProductName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public int CategoryId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
        public int ManufacturerId { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int StoreId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int ProductTypeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Customers.CustomerSearchType")]
        public string CustomerSearchType { get; set; }

        #endregion

        public class SearchResultModel : EntityModelBase
        {
            public string ReturnValue { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public string SummaryTitle { get; set; }
            public bool? Published { get; set; }
            public bool Disable { get; set; }
            public bool Selected { get; set; }
            public string ImageUrl { get; set; }
            public string LabelText { get; set; }
            public string LabelClassName { get; set; }
        }
    }

    public class EntityPickerProduct
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public bool Published { get; set; }
        public int ProductTypeId { get; set; }
        public int? MainPictureId { get; set; }
    }
}