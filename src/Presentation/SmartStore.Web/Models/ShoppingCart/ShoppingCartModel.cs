using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.ShoppingCart
{
    public partial class ShoppingCartModel : ModelBase
    {
        public ShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            EstimateShipping = new EstimateShippingModel();
            DiscountBox = new DiscountBoxModel();
            GiftCardBox = new GiftCardBoxModel();
            CheckoutAttributes = new List<CheckoutAttributeModel>();
            OrderReviewData = new OrderReviewDataModel();

            ButtonPaymentMethodActionNames = new List<string>();
            ButtonPaymentMethodControllerNames = new List<string>();
            ButtonPaymentMethodRouteValues = new List<RouteValueDictionary>();
        }

        public bool ShowSku { get; set; }
        public bool ShowProductImages { get; set; }
        public bool IsEditable { get; set; }
        public IList<ShoppingCartItemModel> Items { get; set; }

        public string CheckoutAttributeInfo { get; set; }
        public IList<CheckoutAttributeModel> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }
        public string MinOrderSubtotalWarning { get; set; }
        public bool TermsOfServiceEnabled { get; set; }
        public EstimateShippingModel EstimateShipping { get; set; }
        public DiscountBoxModel DiscountBox { get; set; }
        public GiftCardBoxModel GiftCardBox { get; set; }
        public OrderReviewDataModel OrderReviewData { get; set; }

        //codehint: sm-add
        public int MediaDimensions { get; set; }
        public bool DisplayDeliveryTime { get; set; }
        public bool DisplayShortDesc { get; set; }
        

        public IList<string> ButtonPaymentMethodActionNames { get; set; }
        public IList<string> ButtonPaymentMethodControllerNames { get; set; }
        public IList<RouteValueDictionary> ButtonPaymentMethodRouteValues { get; set; }

		#region Nested Classes

        public partial class ShoppingCartItemModel : EntityModelBase
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                AllowedQuantities = new List<SelectListItem>();
                Warnings = new List<string>();
            }
            public string Sku { get; set; }

            public PictureModel Picture {get;set;}

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int Quantity { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }
            
            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public IList<string> Warnings { get; set; }

            //codehint: BEGIN sm-add
            public bool IsShipEnabled { get; set; }

            public DeliveryTime DeliveryTime { get; set; }

            public string ShortDesc { get; set; }
            //codehint: END sm-add

        }

        public partial class CheckoutAttributeModel : EntityModelBase
        {
            public CheckoutAttributeModel()
            {
                Values = new List<CheckoutAttributeValueModel>();
            }

            public string Name { get; set; }

            public string DefaultValue { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<CheckoutAttributeValueModel> Values { get; set; }
        }

        public partial class CheckoutAttributeValueModel : EntityModelBase
        {
            public string Name { get; set; }

            public string PriceAdjustment { get; set; }

            public bool IsPreSelected { get; set; }
        }

        public partial class DiscountBoxModel: ModelBase
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public string CurrentCode { get; set; }
        }

        public partial class GiftCardBoxModel : ModelBase
        {
            public bool Display { get; set; }
            public string Message { get; set; }
        }

        public partial class OrderReviewDataModel : ModelBase
        {
            public OrderReviewDataModel()
            {
                this.BillingAddress = new AddressModel();
                this.ShippingAddress = new AddressModel();
            }
            public bool Display { get; set; }

            public AddressModel BillingAddress { get; set; }

            public bool IsShippable { get; set; }
            public AddressModel ShippingAddress { get; set; }
            public string ShippingMethod { get; set; }

            public string PaymentMethod { get; set; }
        }
		#endregion
    }
}