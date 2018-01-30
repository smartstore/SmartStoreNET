using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Admin.Models.Settings
{
	public class ShoppingCartSettingsModel : ILocalizedModel<ShoppingCartSettingsLocalizedModel>
	{
		public ShoppingCartSettingsModel()
		{
			Locales = new List<ShoppingCartSettingsLocalizedModel>();
		}

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.DisplayCartAfterAddingProduct")]
        public bool DisplayCartAfterAddingProduct { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.DisplayWishlistAfterAddingProduct")]
        public bool DisplayWishlistAfterAddingProduct { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MaximumShoppingCartItems")]
        public int MaximumShoppingCartItems { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MaximumWishlistItems")]
        public int MaximumWishlistItems { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.AllowOutOfStockItemsToBeAddedToWishlist")]
        public bool AllowOutOfStockItemsToBeAddedToWishlist { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesOnShoppingCart")]
        public bool ShowProductImagesOnShoppingCart { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnShoppingCart")]
		public bool ShowProductBundleImagesOnShoppingCart { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesOnWishList")]
        public bool ShowProductImagesOnWishList { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnWishList")]
		public bool ShowProductBundleImagesOnWishList { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowDiscountBox")]
        public bool ShowDiscountBox { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowGiftCardBox")]
        public bool ShowGiftCardBox { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.CrossSellsNumber")]
        public int CrossSellsNumber { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.EmailWishlistEnabled")]
        public bool EmailWishlistEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.AllowAnonymousUsersToEmailWishlist")]
        public bool AllowAnonymousUsersToEmailWishlist { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartEnabled")]
        public bool MiniShoppingCartEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesInMiniShoppingCart")]
        public bool ShowProductImagesInMiniShoppingCart { get; set; }
		
        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint")]
        public bool ShowConfirmOrderLegalHint { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowDeliveryTimes")]
        public bool ShowDeliveryTimes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowShortDesc")]
        public bool ShowShortDesc { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowWeight")]
        public bool ShowWeight { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowBasePrice")]
        public bool ShowBasePrice { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowLinkedAttributeValueQuantity")]
		public bool ShowLinkedAttributeValueQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowCommentBox")]
        public bool ShowCommentBox { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowEsdRevocationWaiverBox")]
		public bool ShowEsdRevocationWaiverBox { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.NewsLetterSubscription")]
		public CheckoutNewsLetterSubscription NewsLetterSubscription { get; set; }
		public SelectList AvailableNewsLetterSubscriptions { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOver")]
		public CheckoutThirdPartyEmailHandOver ThirdPartyEmailHandOver { get; set; }
		public SelectList AvailableThirdPartyEmailHandOver { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOverLabel")]
		public string ThirdPartyEmailHandOverLabel { get; set; }

		public IList<ShoppingCartSettingsLocalizedModel> Locales { get; set; }
	}


	public class ShoppingCartSettingsLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOverLabel")]
		public string ThirdPartyEmailHandOverLabel { get; set; }
	}
}