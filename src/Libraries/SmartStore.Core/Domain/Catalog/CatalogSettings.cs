using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Catalog
{

    public class CatalogSettings : ISettings
    {
        public CatalogSettings()
        {
            FileUploadAllowedExtensions = new List<string>();
			AllowProductSorting = true;
			AllowProductViewModeChanging = true;
			DefaultViewMode = "grid";
			CategoryBreadcrumbEnabled = true;
			ShowShareButton = true;
			PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style \"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\">var addthis_config = {\"data_track_addressbar\":false};</script><script type=\"text/javascript\" src=\"//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-50f6c18f03ecbb2f\"></script><!-- AddThis Button END -->";
			DefaultProductRatingValue = 5;
			NotifyStoreOwnerAboutNewProductReviews = true;
			EmailAFriendEnabled = true;
			AskQuestionEnabled = true;
			RecentlyViewedProductsNumber = 6;
			RecentlyViewedProductsEnabled = true;
			RecentlyAddedProductsNumber = 10;
			RecentlyAddedProductsEnabled = true;
			CompareProductsEnabled = true;
			ProductSearchAutoCompleteEnabled = true;
			ProductSearchAutoCompleteNumberOfProducts = 10;
			ProductSearchTermMinimumLength = 3;
			NumberOfBestsellersOnHomepage = 6;
			SearchPageProductsPerPage = 6;
			ProductsAlsoPurchasedEnabled = true;
			ProductsAlsoPurchasedNumber = 6;
			NumberOfProductTags = 15;
			ProductsByTagPageSize = 12;
			UseSmallProductBoxOnHomePage = true;
			DisplayTierPricesWithDiscounts = true;
			DefaultPageSizeOptions = "12, 18, 36, 72, 150";
			ProductsByTagAllowCustomersToSelectPageSize = true;
			ProductsByTagPageSizeOptions = "12, 18, 36, 72, 150";
			MaximumBackInStockSubscriptions = 200;
			FileUploadMaximumSizeBytes = 1024 * 200; //200KB
			ManufacturersBlockItemsToDisplay = 5;
			DisplayAllImagesNumber = 6;	// codehint: sm-add
			ShowColorSquaresInLists = true;
			ShowDiscountSign = true;
			ShowVariantCombinationPriceAdjustment = true;
			ShowLinkedAttributeValueImage = true;
			EnableDynamicPriceUpdate = true;
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
        /// Gets or sets a value indicating whether product sorting is enabled
        /// </summary>
        public bool AllowProductSorting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
        /// </summary>
        public bool AllowProductViewModeChanging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
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
        /// Gets or sets a value indicating whether a 'Share button' is enabled
        /// </summary>
        public bool ShowShareButton { get; set; }

        /// <summary>
        /// Gets or sets a share code (e.g. AddThis button code)
        /// </summary>
        public string PageShareCode { get; set; }

        /// codehint: sm-add
        /// <summary>
        /// Gets or sets a value indicating whether to display reviews in product lists
        /// </summary>
        public bool ShowProductReviewsInProductLists { get; set; }

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
        /// Gets or sets a value indicating whether autocomplete is enabled
        /// </summary>
        public bool ProductSearchAutoCompleteEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product images in the auto complete search
        /// </summary>
        public bool ShowProductImagesInSearchAutoComplete { get; set; }

        /// <summary>
        /// Gets or sets a number of products to return when using "autocomplete" feature
        /// </summary>
        public int ProductSearchAutoCompleteNumberOfProducts { get; set; }

        /// <summary>
        /// Gets or sets a minimum search term length
        /// </summary>
        public int ProductSearchTermMinimumLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show bestsellers on home page
        /// </summary>
        public bool ShowBestsellersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a number of bestsellers on home page
        /// </summary>
        public int NumberOfBestsellersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a number of products per page on search products page
        /// </summary>
        public int SearchPageProductsPerPage { get; set; }

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
        /// Gets or sets a number of products per page on 'products by tag' page
        /// </summary>
        public int ProductsByTagPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size for 'products by tag'
        /// </summary>
        public bool ProductsByTagAllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options for 'products by tag'
        /// </summary>
        public string ProductsByTagPageSizeOptions { get; set; }

        //codehint: sm-add begin
        public int ProductSearchPageSize { get; set; }

        public bool ProductSearchAllowCustomersToSelectPageSize { get; set; }

        public string ProductSearchPageSizeOptions { get; set; }

        public int RecentlyAddedProductsPageSize { get; set; }

        public bool RecentlyAddedProductsAllowCustomersToSelectPageSize { get; set; }

        public string RecentlyAddedProductsPageSizeOptions { get; set; }
		
		public int DisplayAllImagesNumber { get; set; }

        public bool ShowColorSquaresInLists { get; set; }

		public bool HideBuyButtonInLists { get; set; }

        public int? LabelAsNewForMaxDays { get; set; }

        public bool ShowDiscountSign { get; set; }

		public bool SuppressSkuSearch { get; set; }

        //codehint: sm-add end

        /// <summary>
        /// Gets or sets the available customer selectable default page size options
        /// </summary>
        public string DefaultPageSizeOptions { get; set; }

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
        /// Gets or set the default value to use for Category page size options (for new Categories)
        /// </summary>
        /// <remarks>Obsolete</remarks>
        public string DefaultCategoryPageSizeOptions { get; set; }

        /// <summary>
        /// Gets or set the default value to use for Manufacturer page size opitons (for new Manufacturers)
        /// </summary>
        /// <remarks>Obsolete</remarks>
        public string DefaultManufacturerPageSizeOptions { get; set; }

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
        /// Gets or sets the value indicating how many manufacturers to display in manufacturers block
        /// </summary>
        public int ManufacturersBlockItemsToDisplay { get; set; }
    }
}