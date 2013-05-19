using System;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Validators.Affiliates;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Affiliates
{
    [Validator(typeof(AffiliateValidator))]
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

            [SmartResourceDisplayName("Admin.Affiliates.Orders.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public class AffiliatedCustomerModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Affiliates.Customers.Name")]
            public string Name { get; set; }
        }

        #endregion
    }
}