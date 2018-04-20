using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Models.Catalog
{
    public partial class CategoryModel : EntityModelBase, ISearchResultModel
    {
        public CategoryModel()
        {
			PictureModel = new PictureModel();
            SubCategories = new List<SubCategoryModel>();
        }

		public CatalogSearchResult SearchResult
		{
			get;
			set;
		}

		public LocalizedValue<string> Name { get; set; }
		public LocalizedValue<string> FullName { get; set; }
        public LocalizedValue<string> Description { get; set; }
		public LocalizedValue<string> BottomDescription { get; set; }
        public LocalizedValue<string> MetaKeywords { get; set; }
        public LocalizedValue<string> MetaDescription { get; set; }
        public LocalizedValue<string> MetaTitle { get; set; }
        public string SeName { get; set; }
        
        public PictureModel PictureModel { get; set; }

        public bool DisplayCategoryBreadcrumb { get; set; }

		public SubCategoryDisplayType SubCategoryDisplayType { get; set; }
        
        public IList<SubCategoryModel> SubCategories { get; set; }

        public ProductSummaryModel FeaturedProducts { get; set; }
        public ProductSummaryModel Products { get; set; }
        

		#region Nested Classes

        public partial class SubCategoryModel : EntityModelBase
        {
            public SubCategoryModel()
            {
                PictureModel = new PictureModel();
            }

            public LocalizedValue<string> Name { get; set; }
            public string SeName { get; set; }
            public PictureModel PictureModel { get; set; }
        }

		#endregion
    }
}