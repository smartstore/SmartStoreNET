using System.Collections.Generic;
using SmartStore.Admin.Models.Common;
using SmartStore.Core.Domain.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{

    public class ShipmentModel : EntityModelBase
    {
        public ShipmentModel()
        {
            this.Items = new List<ShipmentItemModel>();
			MerchantCompanyInfo = new CompanyInformationSettings();
        }

		public int StoreId { get; set; }
		public string ShippingMethod { get; set; }
		public Address ShippingAddress { get; set; }
		public CompanyInformationSettings MerchantCompanyInfo { get; set; }
		
		[SmartResourceDisplayName("Admin.Orders.Shipments.OrderID")]
        public int OrderId { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.TotalWeight")]
        public string TotalWeight { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.TrackingNumber")]
        public string TrackingNumber { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.ShippedDate")]
        public string ShippedDate { get; set; }
        public bool CanShip { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.DeliveryDate")]
        public string DeliveryDate { get; set; }
        public bool CanDeliver { get; set; }

        public List<ShipmentItemModel> Items { get; set; }

        public bool DisplayPdfPackagingSlip { get; set; }

        #region Nested classes

        public class ShipmentItemModel : EntityModelBase
        {
            public int OrderItemId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
			public string ProductTypeName { get; set; }
			public string ProductTypeLabelHint { get; set; }
            public string Sku { get; set; }
            public string AttributeInfo { get; set; }

            //weight of one item (product)
            public string ItemWeight { get; set; }
            public string ItemDimensions { get; set; }

            public int QuantityToAdd { get; set; }
            public int QuantityOrdered { get; set; }
            public int QuantityInThisShipment { get; set; }
            public int QuantityInAllShipments { get; set; }
        }
        #endregion
    }
}