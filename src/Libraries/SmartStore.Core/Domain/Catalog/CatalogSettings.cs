using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Catalog
{
	public enum SubCategoryDisplayType
	{
		Hide = 0,
		AboveProductList = 5,
		Bottom = 10
	}

	public enum GridColumnSpan
	{
		Max2Cols = 2,
		Max3Cols = 3,
		Max4Cols = 4,
		Max5Cols = 5,
		Max6Cols = 6
	}

    public class CatalogSettings : ISettings
    {
        public CatalogSettings()
        {
            FileUploadAllowedExtensions = new List<string>();
			AllowProductSorting = true;
            DefaultSortOrder = ProductSortingEnum.Relevance;
			AllowProductViewModeChanging = true;
			DefaultViewMode = "grid";
			CategoryBreadcrumbEnabled = true;
			ShowShareButton = true;
			PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style addthis_32x32_style\"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\">var addthis_config = {\"data_track_addressbar\":false};</script><script type=\"text/javascript\" src=\"//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-50f6c18f03ecbb2f\"></script><!-- AddThis Button END -->";
			DefaultProductRatingValue = 5;
			NotifyStoreOwnerAboutNewProductReviews = true;
			EmailAFriendEnabled = true;
			AskQuestionEnabled = true;
			RecentlyViewedProductsNumber = 8;
			RecentlyViewedProductsEnabled = true;
			RecentlyAddedProductsNumber = 100;
			RecentlyAddedProductsEnabled = true;
			CompareProductsEnabled = true;
			SubCategoryDisplayType = SubCategoryDisplayType.AboveProductList;
			NumberOfBestsellersOnHomepage = 12;
            ShowPopularProductTagsOnHomepage = false;
            ShowManufacturersOnHomepage = true;
            ShowManufacturerPictures = true;
            ManufacturerItemsToDisplayOnHomepage = 18;
            ShowManufacturersInOffCanvas = true;
            ManufacturerItemsToDisplayInOffcanvasMenu = 20;
            ShowManufacturerInProductDetail = true;
            ShowManufacturerPicturesInProductDetail = true;
			ProductsAlsoPurchasedEnabled = true;
			ProductsAlsoPurchasedNumber = 12;
			NumberOfProductTags = 15;
			DefaultProductListPageSize = 24;
			UseSmallProductBoxOnHomePage = true;
			DisplayTierPricesWithDiscounts = true;
			DefaultPageSizeOptions = "12,24,36,48,72,120";
			AllowCustomersToSelectPageSize = true;
			MaximumBackInStockSubscriptions = 200;
			FileUploadMaximumSizeBytes = 1024 * 200; //200KB
			DisplayAllImagesNumber = 6;
			ShowShortDescriptionInGridStyleLists = true;
			ShowManufacturerInGridStyleLists = true;
			ShowManufacturerLogoInLists = false;
			ShowProductOptionsInLists = true;
			ShowColorSquaresInLists = true;
			ShowDiscountSign = true;
			ShowVariantCombinationPriceAdjustment = true;
			ShowLinkedAttributeValueImage = true;
			EnableDynamicPriceUpdate = true;
            ShowProductReviewsInProductDetail = true;
            EnableHtmlTextCollapser = true;
			HtmlTextCollapsedHeight = 260;
			MostRecentlyUsedCategoriesMaxSize = 6;
			MostRecentlyUsedManufacturersMaxSize = 4;
			IncludeShortDescriptionInCompareProducts = true;
			GridStyleListColumnSpan = GridColumnSpan.Max4Cols;
			ShowProductsFromSubcategories = true;
            ApplyTierPricePercentageToAttributePriceAdjustments = false;
            AllowDifferingEmailAddressForEmailAFriend = false;
			AllowAnonymousUsersToEmailAFriend = false;
			AllowAnonymousUsersToReviewProduct = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display product SKU
        /// </summary>
        public bool ShowProductSku { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display manufacturer part number of a product
        /// </summary>
        public bool ShowManufacturerPartNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display GTIN of a product
        /// </summary>
        public bool ShowGtin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display weight of a product
        /// </summary>
        public bool ShowWeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display dimensions of a product
        /// </summary>
        public bool ShowDimensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the delivery time of a product
        /// </summary>
        public bool ShowDeliveryTimesInProductLists { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the delivery time of a product
        /// </summary>
        public bool ShowDeliveryTimesInProductDetail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the base price of a product
        /// </summary>
        public bool ShowBasePriceInProductLists { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display price adjustment of a product variant combination
        /// </summary>
        public bool ShowVariantCombinationPriceAdjustment { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to display quantity of linked product at attribute values
		/// </summary>
		public bool ShowLinkedAttributeValueQuantity { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to display the image of linked product at attribute values
		/// </summary>
		public bool ShowLinkedAttributeValueImage { get; set; }

        /// <summary>
		/// Gets or sets a value indicating how many menu items will be displayed
		/// </summary>
        public int? MaxItemsToDisplayInCatalogMenu { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product sorting is enabled
        /// </summary>
        public bool AllowProductSorting { get; set; }

        /// <summary>
        /// Gets or sets the default sort order in product lists
        /// </summary>
        public ProductSortingEnum DefaultSortOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
        /// </summary>
        public bool AllowProductViewModeChanging { get; set; }

        /// <summary>
        /// Gets or sets the default view mode for product lists
        /// </summary>
        public string DefaultViewMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a category details page should include products from subcategories
        /// </summary>
        public bool ShowProductsFromSubcategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether number of products should be displayed beside each category
        /// </summary>
        public bool ShowCategoryProductNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we include subcategories (when 'ShowCategoryProductNumber' is 'true')
        /// </summary>
        public bool ShowCategoryProductNumberIncludingSubcategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether category breadcrumb is enabled
        /// </summary>
        public bool CategoryBreadcrumbEnabled { get; set; }
        
		/// <summary>
		/// Gets or sets a value indicating whether and where to display a list of subcategories
		/// </summary>
		public SubCategoryDisplayType SubCategoryDisplayType { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether a 'Share button' is enabled
        /// </summary>
        public bool ShowShareButton { get; set; }

        /// <summary>
        /// Gets or sets a share code (e.g. AddThis button code)
        /// </summary>
        public string PageShareCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display reviews in product lists
        /// </summary>
        public bool ShowProductReviewsInProductLists { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display reviews in product detail
        /// </summary>
        public bool ShowProductReviewsInProductDetail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating product reviews must be approved
        /// </summary>
        public bool ProductReviewsMustBeApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default rating value of the product reviews
        /// </summary>
        public int DefaultProductRatingValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow anonymous users write product reviews.
        /// </summary>
        public bool AllowAnonymousUsersToReviewProduct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notification of a store owner about new product reviews is enabled
        /// </summary>
        public bool NotifyStoreOwnerAboutNewProductReviews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product 'Email a friend' feature is enabled
        /// </summary>
        public bool EmailAFriendEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'ask product question' feature is enabled
        /// </summary>
        public bool AskQuestionEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to enter a differing email address 
        /// </summary>
        public bool AllowDifferingEmailAddressForEmailAFriend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow anonymous users to email a friend.
        /// </summary>
        public bool AllowAnonymousUsersToEmailAFriend { get; set; }

        /// <summary>
        /// Gets or sets a number of "Recently viewed products"
        /// </summary>
        public int RecentlyViewedProductsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Recently viewed products" feature is enabled
        /// </summary>
        public bool RecentlyViewedProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a number of "Recently added products"
        /// </summary>
        public int RecentlyAddedProductsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Recently added products" feature is enabled
        /// </summary>
        public bool RecentlyAddedProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Compare products" feature is enabled
        /// </summary>
        public bool CompareProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show bestsellers on home page
        /// </summary>
        public bool ShowBestsellersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a number of bestsellers on home page
        /// </summary>
        public int NumberOfBestsellersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show manufacturers on home page
        /// </summary>
        public bool ShowManufacturersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show manufacturers in offcanvas menu
        /// </summary>
        public bool ShowManufacturersInOffCanvas { get; set; }
        
        /// <summary>
        /// Gets or sets the value indicating how many manufacturers on home page
        /// </summary>
        public int ManufacturerItemsToDisplayOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets the value indicating how many manufacturers in offcanvas menu
        /// </summary>
        public int ManufacturerItemsToDisplayInOffcanvasMenu { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to show manufacturer pictures or names on home page
        /// </summary>
        public bool ShowManufacturerPictures { get; set; }

        /// <summary>
		/// Gets or sets a value indicating whether to display manufacturer detail links in product detail pages
		/// </summary>
		public bool ShowManufacturerInProductDetail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display pictures or textual links to manufacturer pages in product detail pages
        /// </summary>
        public bool ShowManufacturerPicturesInProductDetail { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to hide manufacturer default pictures
		/// </summary>
		public bool HideManufacturerDefaultPictures { get; set; }

        /// <summary>
		/// Gets or sets a value indicating whether to hide manufacturer default pictures
		/// </summary>
		public bool SortManufacturersAlphabetically { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to hide category default pictures
        /// </summary>
        public bool HideCategoryDefaultPictures { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to hide product default pictures
		/// </summary>
		public bool HideProductDefaultPictures { get; set; }

        /// <summary>
        /// Gets or sets "List of products purchased by other customers who purchased the above" option is enable
        /// </summary>
        public bool ProductsAlsoPurchasedEnabled { get; set; }

        /// <summary>
        /// Gets or sets a number of products also purchased by other customers to display
        /// </summary>
        public int ProductsAlsoPurchasedNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether dynamic price update is enabled
        /// </summary>
        public bool EnableDynamicPriceUpdate { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether base price should be rendered for bundle items
		/// </summary>
		public bool BundleItemShowBasePrice { get; set; }

        /// <summary>
        /// Gets or sets a number of product tags that appear in the tag cloud
        /// </summary>
        public int NumberOfProductTags { get; set; }

        /// <summary>
        /// Gets or sets a number of products per page on a product list page
        /// </summary>
        public int DefaultProductListPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select page size in product listings
        /// </summary>
        public bool AllowCustomersToSelectPageSize { get; set; }
		
		public int DisplayAllImagesNumber { get; set; }

		public bool ShowManufacturerInGridStyleLists { get; set; }

		/// <summary>
		/// Whether to show brand logo instead of textual name in product lists
		/// </summary>
		public bool ShowManufacturerLogoInLists { get; set; }

		public bool ShowShortDescriptionInGridStyleLists { get; set; }

		public bool ShowProductOptionsInLists { get; set; }

		public bool ShowColorSquaresInLists { get; set; }

		public bool HideBuyButtonInLists { get; set; }

        public int? LabelAsNewForMaxDays { get; set; }

        public bool ShowDefaultQuantityUnit { get; set; }

        public bool ShowDefaultDeliveryTime { get; set; }

        public bool ShowPopularProductTagsOnHomepage { get; set; }

        public bool ShowDiscountSign { get; set; }

		/// <summary>
		/// Gets or sets the available customer selectable default page size options
		/// </summary>
		public string DefaultPageSizeOptions { get; set; }

		/// <summary>
		/// Gets or sets the price display type for prices in product lists
		/// </summary>
		public PriceDisplayType PriceDisplayType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include "Short description" in compare products
        /// </summary>
        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include "Full description" in compare products
        /// </summary>
        public bool IncludeFullDescriptionInCompareProducts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use small product boxes on home page
        /// </summary>
        public bool UseSmallProductBoxOnHomePage { get; set; }

        /// <summary>
        /// An option indicating whether products on category and manufacturer pages should include featured products as well
        /// </summary>
        public bool IncludeFeaturedProductsInNormalLists { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether tier prices should be displayed with applied discounts (if available)
        /// </summary>
        public bool DisplayTierPricesWithDiscounts { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to ignore discounts (side-wide)
        /// </summary>
        public bool IgnoreDiscounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore featured products (side-wide)
        /// </summary>
        public bool IgnoreFeaturedProducts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating maximum number of 'back in stock' subscription
        /// </summary>
        public int MaximumBackInStockSubscriptions { get; set; }

        /// <summary>
        /// Gets or sets a maximum file upload size in bytes for product attributes ('File Upload' type)
        /// </summary>
        public int FileUploadMaximumSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets a list of allowed file extensions for customer uploaded files
        /// </summary>
        public List<string> FileUploadAllowedExtensions { get; set; }
        
		/// <summary>
		/// Gets or sets a value indicating if html long text should be collapsed
		/// </summary>
		public bool EnableHtmlTextCollapser { get; set; }

		/// <summary>
		/// Gets or sets the height of collapsed text
		/// </summary>
		public int HtmlTextCollapsedHeight { get; set; }

		/// <summary>
		/// Gets or sets an identifier for a delivery time dislayed when stock is empty
		/// </summary>
		public int? DeliveryTimeIdForEmptyStock { get; set; }

		/// <summary>
		/// Gets or sets how many items to display maximally in the most recently used category list
		/// </summary>
		public int MostRecentlyUsedCategoriesMaxSize { get; set; }

		/// <summary>
		/// Gets or sets how many items to display maximally in the most recently used manufacturer list
		/// </summary>
		public int MostRecentlyUsedManufacturersMaxSize { get; set; }

		/// <summary>
		/// Gets or sets how many columns per row should be displayed at most in grid style lists on largest screen resolution.
		/// </summary>
		public GridColumnSpan GridStyleListColumnSpan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether percental values of tierprices should be applied to price adjustments of attributes
        /// </summary>
        public bool ApplyTierPricePercentageToAttributePriceAdjustments { get; set; }
    }
}