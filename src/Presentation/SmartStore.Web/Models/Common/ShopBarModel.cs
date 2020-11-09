using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class ShopBarModel : ModelBase
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerEmailUsername { get; set; }
        public bool IsCustomerImpersonated { get; set; }
        public bool DisplayAdminLink { get; set; }
        public bool PublicStoreNavigationAllowed { get; set; }

        public bool ShoppingCartEnabled { get; set; }
        public int CartItemsCount { get; set; }

        public bool WishlistEnabled { get; set; }
        public int WishlistItemsCount { get; set; }

        public bool CompareProductsEnabled { get; set; }
        public int CompareItemsCount { get; set; }
    }
}