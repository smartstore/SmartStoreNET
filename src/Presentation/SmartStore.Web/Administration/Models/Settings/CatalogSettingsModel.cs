using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(CatalogSettingsValidator))]
    public class CatalogSettingsModel
    {
        public CatalogSettingsModel()
        {
            AvailableDefaultViewModes = new List<SelectListItem>();
        }

        #region General

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

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDiscountSign")]
        public bool ShowDiscountSign { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.PriceDisplayStyle")]
        public PriceDisplayStyle PriceDisplayStyle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DisplayTextForZeroPrices")]
        public bool DisplayTextForZeroPrices { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IgnoreDiscounts")]
        public bool IgnoreDiscounts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ApplyPercentageDiscountOnTierPrice")]
        public bool ApplyPercentageDiscountOnTierPrice { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ApplyTierPricePercentageToAttributePriceAdjustments")]
        public bool ApplyTierPricePercentageToAttributePriceAdjustments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IgnoreFeaturedProducts")]
        public bool IgnoreFeaturedProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.CompareProductsEnabled")]
        public bool CompareProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeShortDescriptionInCompareProducts")]
        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeFullDescriptionInCompareProducts")]
        public bool IncludeFullDescriptionInCompareProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowBestsellersOnHomepage")]
        public bool ShowBestsellersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NumberOfBestsellersOnHomepage")]
        public int NumberOfBestsellersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.EnableHtmlTextCollapser")]
        public bool EnableHtmlTextCollapser { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.HtmlTextCollapsedHeight")]
        public int HtmlTextCollapsedHeight { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDefaultQuantityUnit")]
        public bool ShowDefaultQuantityUnit { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDefaultDeliveryTime")]
        public bool ShowDefaultDeliveryTime { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowPopularProductTagsOnHomepage")]
        public bool ShowPopularProductTagsOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturersOnHomepage")]
        public bool ShowManufacturersOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturersInOffCanvas")]
        public bool ShowManufacturersInOffCanvas { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.MaxItemsToDisplayInCatalogMenu")]
        public int MaxItemsToDisplayInCatalogMenu { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ManufacturerItemsToDisplayOnHomepage")]
        public int ManufacturerItemsToDisplayOnHomepage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ManufacturerItemsToDisplayInOffcanvasMenu")]
        public int ManufacturerItemsToDisplayInOffCanvasMenu { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerPictures")]
        public bool ShowManufacturerPictures { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.HideManufacturerDefaultPictures")]
        public bool HideManufacturerDefaultPictures { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.SortManufacturersAlphabetically")]
        public bool SortManufacturersAlphabetically { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.HideCategoryDefaultPictures")]
        public bool HideCategoryDefaultPictures { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.HideProductDefaultPictures")]
        public bool HideProductDefaultPictures { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductCondition")]
        public bool ShowProductCondition { get; set; }

        #endregion

        #region Product lists

        #region Navigation

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductsFromSubcategories")]
        public bool ShowProductsFromSubcategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeFeaturedProductsInNormalLists")]
        public bool IncludeFeaturedProductsInNormalLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowCategoryProductNumber")]
        public bool ShowCategoryProductNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowCategoryProductNumberIncludingSubcategories")]
        public bool ShowCategoryProductNumberIncludingSubcategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.CategoryBreadcrumbEnabled")]
        public bool CategoryBreadcrumbEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.SubCategoryDisplayType")]
        public SubCategoryDisplayType SubCategoryDisplayType { get; set; }
        public SelectList AvailableSubCategoryDisplayTypes { get; set; }

        #endregion

        #region Product list

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowProductSorting")]
        public bool AllowProductSorting { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultViewMode")]
        public string DefaultViewMode { get; set; }
        public IList<SelectListItem> AvailableDefaultViewModes { get; private set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultSortOrderMode")]
        public ProductSortingEnum DefaultSortOrder { get; set; }
        public SelectList AvailableSortOrderModes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowProductViewModeChanging")]
        public bool AllowProductViewModeChanging { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultProductListPageSize")]
        public int DefaultProductListPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultPageSizeOptions")]
        public string DefaultPageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.PriceDisplayType")]
        public PriceDisplayType PriceDisplayType { get; set; }
        public SelectList AvailablePriceDisplayTypes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.GridStyleListColumnSpan")]
        public GridColumnSpan GridStyleListColumnSpan { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowSubCategoriesInSubPages")]
        public bool ShowSubCategoriesInSubPages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowDescriptionInSubPages")]
        public bool ShowDescriptionInSubPages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.IncludeFeaturedProductsInSubPages")]
        public bool IncludeFeaturedProductsInSubPages { get; set; }

        #endregion

        #region Products

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowShortDescriptionInGridStyleLists")]
        public bool ShowShortDescriptionInGridStyleLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerInGridStyleLists")]
        public bool ShowManufacturerInGridStyleLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerLogoInLists")]
        public bool ShowManufacturerLogoInLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductOptionsInLists")]
        public bool ShowProductOptionsInLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DeliveryTimesInLists")]
        public DeliveryTimesPresentation DeliveryTimesInLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowBasePriceInProductLists")]
        public bool ShowBasePriceInProductLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowColorSquaresInLists")]
        public bool ShowColorSquaresInLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.HideBuyButtonInLists")]
        public bool HideBuyButtonInLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.LabelAsNewForMaxDays")]
        public int? LabelAsNewForMaxDays { get; set; }

        #endregion 

        #region Product tags

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.NumberOfProductTags")]
        public int NumberOfProductTags { get; set; }

        #endregion

        #endregion

        #region Customers 

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductReviewsInProductLists")]
        public bool ShowProductReviewsInProductLists { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowProductReviewsInProductDetail")]
        public bool ShowProductReviewsInProductDetail { get; set; }

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

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.AllowDifferingEmailAddressForEmailAFriend")]
        public bool AllowDifferingEmailAddressForEmailAFriend { get; set; }

        #endregion

        #region Product detail

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyViewedProductsEnabled")]
        public bool RecentlyViewedProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyViewedProductsNumber")]
        public int RecentlyViewedProductsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsEnabled")]
        public bool RecentlyAddedProductsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.RecentlyAddedProductsNumber")]
        public int RecentlyAddedProductsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowShareButton")]
        public bool ShowShareButton { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.PageShareCode")]
        public string PageShareCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsAlsoPurchasedEnabled")]
        public bool ProductsAlsoPurchasedEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ProductsAlsoPurchasedNumber")]
        public int ProductsAlsoPurchasedNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DisplayAllImagesNumber")]
        public int DisplayAllImagesNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerInProductDetail")]
        public bool ShowManufacturerInProductDetail { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowManufacturerPicturesInProductDetail")]
        public bool ShowManufacturerPicturesInProductDetail { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DeliveryTimesInProductDetail")]
        public DeliveryTimesPresentation DeliveryTimesInProductDetail { get; set; }

        [UIHint("DeliveryTimes")]
        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DeliveryTimeIdForEmptyStock")]
        public int? DeliveryTimeIdForEmptyStock { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.EnableDynamicPriceUpdate")]
        public bool EnableDynamicPriceUpdate { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.BundleItemShowBasePrice")]
        public bool BundleItemShowBasePrice { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowVariantCombinationPriceAdjustment")]
        public bool ShowVariantCombinationPriceAdjustment { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowLoginForPriceNote")]
        public bool ShowLoginForPriceNote { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueQuantity")]
        public bool ShowLinkedAttributeValueQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.ShowLinkedAttributeValueImage")]
        public bool ShowLinkedAttributeValueImage { get; set; }

        #endregion
    }

    public partial class CatalogSettingsValidator : AbstractValidator<CatalogSettingsModel>
    {
        public CatalogSettingsValidator()
        {
            RuleFor(x => x.LabelAsNewForMaxDays).LessThan(1000);
        }
    }
}