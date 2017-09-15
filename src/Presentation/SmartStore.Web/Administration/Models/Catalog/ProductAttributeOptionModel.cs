using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
	[Validator(typeof(ProductAttributeOptionModelValidator))]
	public class ProductAttributeOptionModel : EntityModelBase, ILocalizedModel<ProductAttributeOptionLocalizedModel>
	{
		public ProductAttributeOptionModel()
		{
			IsListTypeAttribute = true;
			Locales = new List<ProductAttributeOptionLocalizedModel>();
		}

		public int ProductId { get; set; }
		public int ProductVariantAttributeId { get; set; }
		public int ProductAttributeOptionsSetId { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name")]
		public string Name { get; set; }
		public string NameString { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Alias")]
		public string Alias { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb")]
		[UIHint("Color")]
		public string Color { get; set; }
		public bool IsListTypeAttribute { get; set; }

		[UIHint("Picture")]
		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Picture")]
		public int PictureId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment")]
		public decimal PriceAdjustment { get; set; }
		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment")]
		public string PriceAdjustmentString { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment")]
		public decimal WeightAdjustment { get; set; }
		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment")]
		public string WeightAdjustmentString { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected")]
		public bool IsPreSelected { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ValueTypeId")]
		public int ValueTypeId { get; set; }
		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ValueTypeId")]
		public string TypeName { get; set; }
		public string TypeNameClass { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct")]
		public int LinkedProductId { get; set; }
		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct")]
		public string LinkedProductName { get; set; }
		public string LinkedProductTypeName { get; set; }
		public string LinkedProductTypeLabelHint { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity")]
		public int Quantity { get; set; }
		public string QuantityInfo { get; set; }

		public IList<ProductAttributeOptionLocalizedModel> Locales { get; set; }
	}

	public class ProductAttributeOptionLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name")]
		public string Name { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Alias")]
		public string Alias { get; set; }
	}
}