using System;
using SmartStore.Admin.Models.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Affiliates
{
    public class AffiliateModel : EntityModelBase
    {
        public AffiliateModel()
        {
            Address = new AddressModel();
        }

        [SmartResourceDisplayName("Admin.Affiliates.Fields.ID")]
        public override int Id { get; set; }

        [SmartResourceDisplayName("Admin.Affiliates.Fields.URL")]
        public string Url { get; set; }

        [SmartResourceDisplayName("Admin.Affiliates.Fields.Active")]
        public bool Active { get; set; }

        public AddressModel Address { get; set; }

        public int GridPageSize { get; set; }
        public bool UsernamesEnabled { get; set; }

        #region Nested classes

        public class AffiliatedOrderModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Affiliates.Orders.Order")]
            public override int Id { get; set; }

            [SmartResourceDisplayName("Admin.Affiliates.Orders.OrderStatus")]
            public string OrderStatus { get; set; }

            [SmartResourceDisplayName("Admin.Affiliates.Orders.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [SmartResourceDisplayName("Admin.Affiliates.Orders.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [SmartResourceDisplayName("Admin.Affiliates.Orders.OrderTotal")]
            public string OrderTotal { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public class AffiliatedCustomerModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
            public string Email { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
            public string Username { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
            public string FullName { get; set; }
        }

        #endregion
    }
}