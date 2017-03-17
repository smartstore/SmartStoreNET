using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
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

		public string Name { get; set; }
		public string FullName { get; set; }
        public string Description { get; set; }
		public string BottomDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
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

            public string Name { get; set; }
            public string SeName { get; set; }
            public PictureModel PictureModel { get; set; }
        }

		#endregion
    }
}