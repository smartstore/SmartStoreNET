using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    [Validator(typeof(CheckoutAttributeValidator))]
    public class CheckoutAttributeModel : EntityModelBase, ILocalizedModel<CheckoutAttributeLocalizedModel>
    {
        public CheckoutAttributeModel()
        {
            Locales = new List<CheckoutAttributeLocalizedModel>();
            AvailableTaxCategories = new List<SelectListItem>();
        }

		[SmartResourceDisplayName("Common.IsActive")]
		public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TextPrompt")]
        [AllowHtml]
        public string TextPrompt { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.ShippableProductRequired")]
        public bool ShippableProductRequired { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TaxCategory")]
        public int? TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }
        [SmartResourceDisplayName("Admin.Catalog.Attributes.AttributeControlType")]
        [AllowHtml]
        public string AttributeControlTypeName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        public IList<CheckoutAttributeLocalizedModel> Locales { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }
	}

	public class CheckoutAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TextPrompt")]
        [AllowHtml]
        public string TextPrompt { get; set; }
    }
}