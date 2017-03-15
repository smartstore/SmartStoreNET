﻿using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Catalog;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using System;

namespace SmartStore.Web.Models.ShoppingCart
{
    public partial class MiniShoppingCartModel : ModelBase
    {
        public MiniShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
        }

        public IList<ShoppingCartItemModel> Items { get; set; }
        public int TotalProducts { get; set; }
        public string SubTotal { get; set; }
        public bool DisplayCheckoutButton { get; set; }
        public bool CurrentCustomerIsGuest { get; set; }
        public bool AnonymousCheckoutAllowed { get; set; }
        public bool ShowProductImages { get; set; }
        public int ThumbSize { get; set; }

        #region Nested Classes

        public partial class ShoppingCartItemModel : EntityModelBase, IQuantityInput
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                BundleItems = new List<ShoppingCartItemBundleItem>();
                AllowedQuantities = new List<SelectListItem>();
            }

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ShortDesc { get; set; }

            public string ProductSeName { get; set; }

			public string ProductUrl { get; set; }
            
            public int EnteredQuantity { get; set; }

            public string QuantityUnitName { get; set; }        // TODO: get rid of this property, won't be displayed anyway

            public List<SelectListItem> AllowedQuantities { get; set; }

            public int MinOrderAmount { get; set; }

            public int MaxOrderAmount { get; set; }

            public int QuantityStep { get; set; }

            public QuantityControlType QuantiyControlType { get; set; }

            public string UnitPrice { get; set; }

            public string AttributeInfo { get; set; }

            public PictureModel Picture { get; set; }

            public IList<ShoppingCartItemBundleItem> BundleItems { get; set; }

			public DateTime CreatedOnUtc { get; set; }

        }

        public partial class ShoppingCartItemBundleItem : ModelBase 
        {
            public string PictureUrl { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
			public string ProductUrl { get; set; }
        }

        #endregion
    }
}