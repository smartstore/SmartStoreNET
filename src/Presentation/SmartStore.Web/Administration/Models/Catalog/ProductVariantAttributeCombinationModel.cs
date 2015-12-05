using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class ProductVariantAttributeCombinationModel : EntityModelBase
    {
        public ProductVariantAttributeCombinationModel()
        {
            ProductVariantAttributes = new List<ProductVariantAttributeModel>();
            AssignedPictureIds = new int[] { }; // init as empty array
            AssignablePictures = new List<PictureSelectItemModel>();
			AvailableDeliveryTimes = new List<SelectListItem>();
            Warnings = new List<string>();
			DisplayOrder = 0;
        }
        
        [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders")]
        public bool AllowOutOfStockOrders { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Sku")]
		public string Sku { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Gtin")]
		public string Gtin { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.ManufacturerPartNumber")]
		public string ManufacturerPartNumber { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
		public decimal? Price { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DeliveryTime")]
        public int? DeliveryTimeId { get; set; }

		public IList<SelectListItem> AvailableDeliveryTimes { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Pictures")]
        public int[] AssignedPictureIds { get; set; }

        public IList<PictureSelectItemModel> AssignablePictures { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Length")]
        public decimal? Length { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Width")]
        public decimal? Width { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Height")]
        public decimal? Height { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.BasePriceAmount")]
        public decimal? BasePriceAmount { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.BasePriceBaseAmount")]
        public int? BasePriceBaseAmount { get; set; }

		[SmartResourceDisplayName("Common.IsActive")]
		public bool IsActive { get; set; }

        public IList<ProductVariantAttributeModel> ProductVariantAttributes { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Attributes")]
        [AllowHtml]
        public string AttributesXml { get; set; }

		[SmartResourceDisplayName("Common.Product")]
		public string ProductUrl { get; set; }
		public string ProductUrlTitle { get; set; }

		public long DisplayOrder { get; set; }

        [AllowHtml]
        public IList<string> Warnings { get; set; }

        public int ProductId { get; set; }

        #region Nested classes

        public class PictureSelectItemModel : EntityModelBase
        {
            public string PictureUrl { get; set; }
            public bool IsAssigned { get; set; }
        }

        public class ProductVariantAttributeModel : EntityModelBase
        {
            public ProductVariantAttributeModel()
            {
                Values = new List<ProductVariantAttributeValueModel>();
            }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ProductVariantAttributeValueModel> Values { get; set; }
        }

        public class ProductVariantAttributeValueModel : EntityModelBase
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }
        #endregion
    }
}