using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Rules;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Payments
{
    public class PaymentMethodEditModel : TabbableModel, ILocalizedModel<PaymentMethodLocalizedModel>
    {
        public PaymentMethodEditModel()
        {
            Locales = new List<PaymentMethodLocalizedModel>();
        }

        public IList<PaymentMethodLocalizedModel> Locales { get; set; }
        public string IconUrl { get; set; }

        [SmartResourceDisplayName("Common.SystemName")]
        public string SystemName { get; set; }

        [SmartResourceDisplayName("Common.FriendlyName")]
        public string FriendlyName { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ShortDescription")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.FullDescription")]
        [AllowHtml]
        public string FullDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.RoundOrderTotalEnabled")]
        public bool RoundOrderTotalEnabled { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        [UIHint("RuleSets")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("scope", RuleScope.Cart)]
        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.Requirements")]
        public int[] SelectedRuleSetIds { get; set; }
    }


    public class PaymentMethodLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Common.FriendlyName")]
        public string FriendlyName { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ShortDescription")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.FullDescription")]
        [AllowHtml]
        public string FullDescription { get; set; }
    }
}