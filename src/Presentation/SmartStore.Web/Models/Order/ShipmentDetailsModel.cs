using System;
using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Models.Order
{
    public partial class ShipmentDetailsModel : EntityModelBase
    {
        public ShipmentDetailsModel()
        {
            ShipmentStatusEvents = new List<ShipmentStatusEventModel>();
            Items = new List<ShipmentItemModel>();
        }

        public string TrackingNumber { get; set; }
        public string TrackingNumberUrl { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public IList<ShipmentStatusEventModel> ShipmentStatusEvents { get; set; }
        public bool ShowSku { get; set; }
        public IList<ShipmentItemModel> Items { get; set; }

        public OrderDetailsModel Order { get; set; }

		#region Nested Classes

        public partial class ShipmentItemModel : EntityModelBase
        {
            public string Sku { get; set; }
            public int ProductId { get; set; }
            public LocalizedValue<string> ProductName { get; set; }
            public string ProductSeName { get; set; }
			public string ProductUrl { get; set; }
            public string AttributeInfo { get; set; }

            public int QuantityOrdered { get; set; }
            public int QuantityShipped { get; set; }
        }

        public partial class ShipmentStatusEventModel : ModelBase
        {
            public string EventName { get; set; }
            public string Location { get; set; }
            public string Country { get; set; }
            public DateTime? Date { get; set; }
        }

		#endregion
    }
}