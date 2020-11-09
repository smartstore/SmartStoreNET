using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.ShoppingCart
{
    public class ShoppingCartItemModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Common.Store")]
        public string Store { get; set; }
        [SmartResourceDisplayName("Admin.CurrentCarts.Product")]
        public int ProductId { get; set; }
        [SmartResourceDisplayName("Admin.CurrentCarts.Product")]
        public string ProductName { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductTypeLabelHint { get; set; }

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