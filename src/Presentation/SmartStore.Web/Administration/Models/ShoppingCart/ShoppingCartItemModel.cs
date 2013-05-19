using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.ShoppingCart
{
    public class ShoppingCartItemModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.CurrentCarts.Product")]
        public int ProductVariantId { get; set; }
        [SmartResourceDisplayName("Admin.CurrentCarts.Product")]
        public string FullProductName { get; set; }

        [SmartResourceDisplayName("Admin.CurrentCarts.UnitPrice")]
        public string UnitPrice { get; set; }
        [SmartResourceDisplayName("Admin.CurrentCarts.Quantity")]
        public int Quantity { get; set; }
        [SmartResourceDisplayName("Admin.CurrentCarts.Total")]
        public string Total { get; set; }
        [SmartResourceDisplayName("Admin.CurrentCarts.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }
    }
}