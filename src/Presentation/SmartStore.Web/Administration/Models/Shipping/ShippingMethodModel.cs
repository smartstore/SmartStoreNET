using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Shipping;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Shipping
{
    [Validator(typeof(ShippingMethodValidator))]
    public class ShippingMethodModel : EntityModelBase, ILocalizedModel<ShippingMethodLocalizedModel>
    {
        public ShippingMethodModel()
        {
            Locales = new List<ShippingMethodLocalizedModel>();
        }
        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.IgnoreCharges")]
		public bool IgnoreCharges { get; set; }

        public IList<ShippingMethodLocalizedModel> Locales { get; set; }
    }

    public class ShippingMethodLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

    }
}