using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Models.Catalog
{
    public partial class CategorySummaryModel : EntityModelBase
    {
        public LocalizedValue<string> Name { get; set; }
        public string Url { get; set; }
        public PictureModel PictureModel { get; set; } = new PictureModel();

        // TODO: Badges
    }

    public partial class CategoryModel : EntityModelBase, ISearchResultModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
            SubCategories = new List<CategorySummaryModel>();
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
        public IList<CategorySummaryModel> SubCategories { get; set; }

        public ProductSummaryModel FeaturedProducts { get; set; }
        public ProductSummaryModel Products { get; set; }
    }
}