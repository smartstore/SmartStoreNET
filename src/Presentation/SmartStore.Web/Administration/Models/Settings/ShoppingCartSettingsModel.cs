using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class ShoppingCartSettingsModel
    {
		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.DisplayCartAfterAddingProduct")]
        public StoreDependingSetting<bool> DisplayCartAfterAddingProduct { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.DisplayWishlistAfterAddingProduct")]
        public StoreDependingSetting<bool> DisplayWishlistAfterAddingProduct { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MaximumShoppingCartItems")]
        public StoreDependingSetting<int> MaximumShoppingCartItems { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MaximumWishlistItems")]
        public StoreDependingSetting<int> MaximumWishlistItems { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.AllowOutOfStockItemsToBeAddedToWishlist")]
        public StoreDependingSetting<bool> AllowOutOfStockItemsToBeAddedToWishlist { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesOnShoppingCart")]
        public StoreDependingSetting<bool> ShowProductImagesOnShoppingCart { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesOnWishList")]
        public StoreDependingSetting<bool> ShowProductImagesOnWishList { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowDiscountBox")]
        public StoreDependingSetting<bool> ShowDiscountBox { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowGiftCardBox")]
        public StoreDependingSetting<bool> ShowGiftCardBox { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.CrossSellsNumber")]
        public StoreDependingSetting<int> CrossSellsNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.EmailWishlistEnabled")]
        public StoreDependingSetting<bool> EmailWishlistEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.AllowAnonymousUsersToEmailWishlist")]
        public StoreDependingSetting<bool> AllowAnonymousUsersToEmailWishlist { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartEnabled")]
        public StoreDependingSetting<bool> MiniShoppingCartEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesInMiniShoppingCart")]
        public StoreDependingSetting<bool> ShowProductImagesInMiniShoppingCart { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartProductNumber")]
        public StoreDependingSetting<int> MiniShoppingCartProductNumber { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint")]
        public StoreDependingSetting<bool> ShowConfirmOrderLegalHint { get; set; }

    }
}