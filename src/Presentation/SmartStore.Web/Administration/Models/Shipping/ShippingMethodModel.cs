using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Rules;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Shipping
{
    [Validator(typeof(ShippingMethodValidator))]
    public class ShippingMethodModel : TabbableModel, ILocalizedModel<ShippingMethodLocalizedModel>
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

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.IgnoreCharges")]
        public bool IgnoreCharges { get; set; }

        public IList<ShippingMethodLocalizedModel> Locales { get; set; }

        // Store mapping
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        [UIHint("RuleSets")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("scope", RuleScope.Cart)]
        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Requirements")]
        public int[] SelectedRuleSetIds { get; set; }

        [SmartResourceDisplayName("Admin.Rules.NumberOfRules")]
        public int NumberOfRules { get; set; }
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

    public partial class ShippingMethodValidator : AbstractValidator<ShippingMethodModel>
    {
        public ShippingMethodValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}