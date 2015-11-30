using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Common
{
    public partial class ShopBarModel : ModelBase
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerEmailUsername { get; set; }
        public bool IsCustomerImpersonated { get; set; }


        public bool DisplayAdminLink { get; set; }

        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }
        public string ShoppingCartAmount { get; set; }

        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }

        public bool CompareProductsEnabled { get; set; }
        public int CompareItems { get; set; }

        //TODO: werden nicht benötigt raus damit 
        public bool AllowPrivateMessages { get; set; }
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }
    }
}