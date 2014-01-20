using System;
using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Catalog
{
	public class ProductBundleItemModel : EntityModelBase, ILocalizedModel<ProductBundleItemLocalizedModel>
	{
		public ProductBundleItemModel()
		{
			Locales = new List<ProductBundleItemLocalizedModel>();
		}

		public IList<ProductBundleItemLocalizedModel> Locales { get; set; }

		public int ProductId { get; set; }
		public int ParentBundledProductId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.OverrideName")]
		public bool OverrideName { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.OverrideShortDescription")]
		public bool OverrideShortDescription { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.ShortDescription")]
		public string ShortDescription { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Quantity")]
		public int Quantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Discount")]
		public decimal? Discount { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.HideThumbnail")]
		public bool HideThumbnail { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Published")]
		public bool Published { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Common.CreatedOn")]
		public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Common.UpdatedOn")]
		public DateTime UpdatedOn { get; set; }
	}


	public class ProductBundleItemLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.ShortDescription")]
		public string ShortDescription { get; set; }
	}
}