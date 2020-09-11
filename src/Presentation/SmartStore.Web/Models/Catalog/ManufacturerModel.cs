using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ManufacturerModel : EntityModelBase, ISearchResultModel
    {
        public ManufacturerModel()
        {
            PictureModel = new PictureModel();
        }

        public LocalizedValue<string> Name { get; set; }
        public LocalizedValue<string> Description { get; set; }
        public LocalizedValue<string> BottomDescription { get; set; }
        public LocalizedValue<string> MetaKeywords { get; set; }
        public LocalizedValue<string> MetaDescription { get; set; }
        public LocalizedValue<string> MetaTitle { get; set; }
        public string SeName { get; set; }

        public PictureModel PictureModel { get; set; }
        public ProductSummaryModel FeaturedProducts { get; set; }
        public ProductSummaryModel Products { get; set; }

        public CatalogSearchResult SearchResult
        {
            get;
            set;
        }
    }
}