using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Models.ShoppingCart
{
    public partial class OffCanvasCartModel : ModelBase
    {
        // product counts
        public int ShoppingCartItems { get; set; }
        public int WishlistItems { get; set; }
        public int CompareItems { get; set; }

        // settings
        public bool ShoppingCartEnabled { get; set; }
        public bool WishlistEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }
    }
}