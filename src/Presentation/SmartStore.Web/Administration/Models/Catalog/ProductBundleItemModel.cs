using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class ProductBundleItemModel : EntityModelBase, ILocalizedModel<ProductBundleItemLocalizedModel>
    {
        public ProductBundleItemModel()
        {
            Locales = new List<ProductBundleItemLocalizedModel>();
            Attributes = new List<ProductBundleItemAttributeModel>();
        }

        public IList<ProductBundleItemLocalizedModel> Locales { get; set; }
        public IList<ProductBundleItemAttributeModel> Attributes { get; set; }

        public int ProductId { get; set; }
        public int BundleProductId { get; set; }
        public bool IsPerItemPricing { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Name")]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Quantity")]
        public int Quantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Discount")]
        public decimal? Discount { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.DiscountPercentage")]
        public bool DiscountPercentage { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.FilterAttributes")]
        public bool FilterAttributes { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.HideThumbnail")]
        public bool HideThumbnail { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Visible")]
        public bool Visible { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
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


    public class ProductBundleItemAttributeModel : EntityModelBase
    {
        public ProductBundleItemAttributeModel()
        {
            Values = new List<SelectListItem>();
            PreSelect = new List<SelectListItem>();
        }

        public static string AttributeControlPrefix => "attribute_";
        public static string PreSelectControlPrefix => "preselect_";

        public string AttributeControlId => AttributeControlPrefix + Id.ToString();
        public string PreSelectControlId => PreSelectControlPrefix + Id.ToString();

        public string Name { get; set; }

        public IList<SelectListItem> Values { get; set; }
        public IList<SelectListItem> PreSelect { get; set; }
    }
}