using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
	public class CatalogSettingsModel
    {
        public CatalogSettingsModel()
        {
            this.AvailableDefaultViewModes = new List<SelectListItem>();
        }
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductSku")]
        public bool ShowProductSku { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerPartNumber")]
        public bool ShowManufacturerPartNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowGtin")]
        public bool ShowGtin { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowWeight")]
        public bool ShowWeight { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDimensions")]
        public bool ShowDimensions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowProductSorting")]
        public bool AllowProductSorting { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowProductViewModeChanging")]
        public bool AllowProductViewModeChanging { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductsFromSubcategories")]
        public bool ShowProductsFromSubcategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowCategoryProductNumber")]
        public bool ShowCategoryProductNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowCategoryProductNumberIncludingSubcategories")]
        public bool ShowCategoryProductNumberIncludingSubcategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.CategoryBreadcrumbEnabled")]
        public bool CategoryBreadcrumbEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowShareButton")]
        public bool ShowShareButton { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowBasePriceInProductLists")]
        public bool ShowBasePriceInProductLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowVariantCombinationPriceAdjustment")]
        public bool ShowVariantCombinationPriceAdjustment { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueQuantity")]
		public bool ShowLinkedAttributeValueQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueImage")]
		public bool ShowLinkedAttributeValueImage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductLists")]
        public bool ShowDeliveryTimesInProductLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail")]
        public bool ShowDeliveryTimesInProductDetail { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductReviewsInProductLists")]
        public bool ShowProductReviewsInProductLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductReviewsMustBeApproved")]
        public bool ProductReviewsMustBeApproved { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowAnonymousUsersToReviewProduct")]
        public bool AllowAnonymousUsersToReviewProduct { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NotifyStoreOwnerAboutNewProductReviews")]
        public bool NotifyStoreOwnerAboutNewProductReviews { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.EmailAFriendEnabled")]
        public bool EmailAFriendEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AskQuestionEnabled")]
        public bool AskQuestionEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowAnonymousUsersToEmailAFriend")]
        public bool AllowAnonymousUsersToEmailAFriend { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyViewedProductsNumber")]
        public int RecentlyViewedProductsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyViewedProductsEnabled")]
        public bool RecentlyViewedProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsNumber")]
        public int RecentlyAddedProductsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsEnabled")]
        public bool RecentlyAddedProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.CompareProductsEnabled")]
        public bool CompareProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowBestsellersOnHomepage")]
        public bool ShowBestsellersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NumberOfBestsellersOnHomepage")]
        public int NumberOfBestsellersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.SearchPageProductsPerPage")]
        public int SearchPageProductsPerPage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteEnabled")]
        public bool ProductSearchAutoCompleteEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteNumberOfProducts")]
        public int ProductSearchAutoCompleteNumberOfProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsAlsoPurchasedEnabled")]
        public bool ProductsAlsoPurchasedEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsAlsoPurchasedNumber")]
        public int ProductsAlsoPurchasedNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.EnableDynamicPriceUpdate")]
        public bool EnableDynamicPriceUpdate { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.BundleItemShowBasePrice")]
		public bool BundleItemShowBasePrice { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NumberOfProductTags")]
        public int NumberOfProductTags { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsByTagPageSize")]
        public int ProductsByTagPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsByTagAllowCustomersToSelectPageSize")]
        public bool ProductsByTagAllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultPageSizeOptions")]
        public string DefaultPageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsByTagPageSizeOptions")]
        public string ProductsByTagPageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchAllowCustomersToSelectPageSize")]
        public bool ProductSearchAllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductSearchPageSizeOptions")]
        public string ProductSearchPageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsPageSize")]
        public int RecentlyAddedProductsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsAllowCustomersToSelectPageSize")]
        public bool RecentlyAddedProductsAllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsPageSizeOptions")]
        public string RecentlyAddedProductsPageSizeOptions { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DisplayAllImagesNumber")]
		public int DisplayAllImagesNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowColorSquaresInLists")]
        public bool ShowColorSquaresInLists { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.HideBuyButtonInLists")]
		public bool HideBuyButtonInLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeShortDescriptionInCompareProducts")]
        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeFullDescriptionInCompareProducts")]
        public bool IncludeFullDescriptionInCompareProducts { get; set; }
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IgnoreDiscounts")]
        public bool IgnoreDiscounts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IgnoreFeaturedProducts")]
        public bool IgnoreFeaturedProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultViewMode")]
        public string DefaultViewMode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.LabelAsNewForMaxDays")]
        public int? LabelAsNewForMaxDays { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDiscountSign")]
        public bool ShowDiscountSign { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.SuppressSkuSearch")]
		public bool SuppressSkuSearch { get; set; }

        public IList<SelectListItem> AvailableDefaultViewModes { get; private set; }
    }
}