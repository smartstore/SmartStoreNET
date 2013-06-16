using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class CatalogSettingsModel
    {
        public CatalogSettingsModel()
        {
            this.AvailableDefaultViewModes = new List<SelectListItem>();
        }

		public int ActiveStoreScopeConfiguration { get; set; }
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductSku")]
        public StoreDependingSetting<bool> ShowProductSku { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerPartNumber")]
        public StoreDependingSetting<bool> ShowManufacturerPartNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowGtin")]
        public StoreDependingSetting<bool> ShowGtin { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowWeight")]
        public StoreDependingSetting<bool> ShowWeight { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDimensions")]
        public StoreDependingSetting<bool> ShowDimensions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowProductSorting")]
        public StoreDependingSetting<bool> AllowProductSorting { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowProductViewModeChanging")]
        public StoreDependingSetting<bool> AllowProductViewModeChanging { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductsFromSubcategories")]
        public StoreDependingSetting<bool> ShowProductsFromSubcategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowCategoryProductNumber")]
        public StoreDependingSetting<bool> ShowCategoryProductNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowCategoryProductNumberIncludingSubcategories")]
        public StoreDependingSetting<bool> ShowCategoryProductNumberIncludingSubcategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.CategoryBreadcrumbEnabled")]
        public StoreDependingSetting<bool> CategoryBreadcrumbEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowShareButton")]
        public StoreDependingSetting<bool> ShowShareButton { get; set; }

        //codehint: sm-add
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowBasePriceInProductLists")]
        public StoreDependingSetting<bool> ShowBasePriceInProductLists { get; set; }

        //codehint: sm-add
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductLists")]
        public StoreDependingSetting<bool> ShowDeliveryTimesInProductLists { get; set; }

        //codehint: sm-add
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductReviewsInProductLists")]
        public StoreDependingSetting<bool> ShowProductReviewsInProductLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductReviewsMustBeApproved")]
        public StoreDependingSetting<bool> ProductReviewsMustBeApproved { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowAnonymousUsersToReviewProduct")]
        public StoreDependingSetting<bool> AllowAnonymousUsersToReviewProduct { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NotifyStoreOwnerAboutNewProductReviews")]
        public StoreDependingSetting<bool> NotifyStoreOwnerAboutNewProductReviews { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.EmailAFriendEnabled")]
        public StoreDependingSetting<bool> EmailAFriendEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AskQuestionEnabled")]
        public StoreDependingSetting<bool> AskQuestionEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowAnonymousUsersToEmailAFriend")]
        public StoreDependingSetting<bool> AllowAnonymousUsersToEmailAFriend { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyViewedProductsNumber")]
        public StoreDependingSetting<int> RecentlyViewedProductsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyViewedProductsEnabled")]
        public StoreDependingSetting<bool> RecentlyViewedProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsNumber")]
        public StoreDependingSetting<int> RecentlyAddedProductsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsEnabled")]
        public StoreDependingSetting<bool> RecentlyAddedProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.CompareProductsEnabled")]
        public StoreDependingSetting<bool> CompareProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowBestsellersOnHomepage")]
        public StoreDependingSetting<bool> ShowBestsellersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NumberOfBestsellersOnHomepage")]
        public StoreDependingSetting<int> NumberOfBestsellersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.SearchPageProductsPerPage")]
        public StoreDependingSetting<int> SearchPageProductsPerPage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteEnabled")]
        public StoreDependingSetting<bool> ProductSearchAutoCompleteEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteNumberOfProducts")]
        public StoreDependingSetting<int> ProductSearchAutoCompleteNumberOfProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsAlsoPurchasedEnabled")]
        public StoreDependingSetting<bool> ProductsAlsoPurchasedEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsAlsoPurchasedNumber")]
        public StoreDependingSetting<int> ProductsAlsoPurchasedNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.EnableDynamicPriceUpdate")]
        public StoreDependingSetting<bool> EnableDynamicPriceUpdate { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NumberOfProductTags")]
        public StoreDependingSetting<int> NumberOfProductTags { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsByTagPageSize")]
        public StoreDependingSetting<int> ProductsByTagPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsByTagAllowCustomersToSelectPageSize")]
        public StoreDependingSetting<bool> ProductsByTagAllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultPageSizeOptions")]
        public StoreDependingSetting<string> DefaultPageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsByTagPageSizeOptions")]
        public StoreDependingSetting<string> ProductsByTagPageSizeOptions { get; set; }

        //codehint: sm-add begin
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchAllowCustomersToSelectPageSize")]
        public StoreDependingSetting<bool> ProductSearchAllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchPageSizeOptions")]
        public StoreDependingSetting<string> ProductSearchPageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsPageSize")]
        public StoreDependingSetting<int> RecentlyAddedProductsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsAllowCustomersToSelectPageSize")]
        public StoreDependingSetting<bool> RecentlyAddedProductsAllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsPageSizeOptions")]
        public StoreDependingSetting<string> RecentlyAddedProductsPageSizeOptions { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DisplayAllImagesNumber")]
		public StoreDependingSetting<int> DisplayAllImagesNumber { get; set; }
        //codehint: sm-add end

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeShortDescriptionInCompareProducts")]
        public StoreDependingSetting<bool> IncludeShortDescriptionInCompareProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeFullDescriptionInCompareProducts")]
        public StoreDependingSetting<bool> IncludeFullDescriptionInCompareProducts { get; set; }
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IgnoreDiscounts")]
        public StoreDependingSetting<bool> IgnoreDiscounts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IgnoreFeaturedProducts")]
        public StoreDependingSetting<bool> IgnoreFeaturedProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultViewMode")]
        public StoreDependingSetting<string> DefaultViewMode { get; set; }

        public IList<SelectListItem> AvailableDefaultViewModes { get; private set; }
    }
}