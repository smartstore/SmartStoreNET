using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(CurrencyValidator))]
    public class CurrencyModel : EntityModelBase, ILocalizedModel<CurrencyLocalizedModel>
    {
        public CurrencyModel()
        {
            Locales = new List<CurrencyLocalizedModel>();
            RoundOrderTotalPaymentMethods = new Dictionary<string, string>();
            RoundNumDecimals = 2;

            AvailableDomainEndings = new List<SelectListItem>
            {
                new SelectListItem { Text = ".com", Value = ".com" },
                new SelectListItem { Text = ".uk", Value = ".uk" },
                new SelectListItem { Text = ".de", Value = ".de" },
                new SelectListItem { Text = ".ch", Value = ".ch" }
            };
        }
        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.CurrencyCode")]
        [AllowHtml]
        public string CurrencyCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.DisplayLocale")]
        [AllowHtml]
        public string DisplayLocale { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Rate")]
        public decimal Rate { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.CustomFormatting")]
        [AllowHtml]
        public string CustomFormatting { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public bool IsPrimaryStoreCurrency { get; set; }
        public bool IsPrimaryExchangeRateCurrency { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.PrimaryStoreCurrencyStores")]
        public IList<SelectListItem> PrimaryStoreCurrencyStores { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.PrimaryExchangeRateCurrencyStores")]
        public IList<SelectListItem> PrimaryExchangeRateCurrencyStores { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.DomainEndings")]
        public string DomainEndings { get; set; }
        public string[] DomainEndingsArray { get; set; }
        public IList<SelectListItem> AvailableDomainEndings { get; set; }

        public IList<CurrencyLocalizedModel> Locales { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        #region Rounding

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderItemsEnabled")]
        public bool RoundOrderItemsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundNumDecimals")]
        public int RoundNumDecimals { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderTotalEnabled")]
        public bool RoundOrderTotalEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderTotalDenominator")]
        public decimal RoundOrderTotalDenominator { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderTotalRule")]
        public CurrencyRoundingRule RoundOrderTotalRule { get; set; }

        public Dictionary<string, string> RoundOrderTotalPaymentMethods { get; set; }

        #endregion Rounding
    }

    public class CurrencyLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }

    public partial class CurrencyValidator : AbstractValidator<CurrencyModel>
    {
        public CurrencyValidator(Localizer T)
        {
            RuleFor(x => x.Name).NotEmpty().Length(1, 50);
            RuleFor(x => x.CurrencyCode).NotEmpty().Length(1, 5);
            RuleFor(x => x.Rate).GreaterThan(0);
            RuleFor(x => x.CustomFormatting).Length(0, 50);
            RuleFor(x => x.DisplayLocale)
                .Must(x =>
                {
                    try
                    {
                        if (String.IsNullOrEmpty(x))
                            return true;
                        var culture = new CultureInfo(x);
                        return culture != null;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage(T("Admin.Configuration.Currencies.Fields.DisplayLocale.Validation"));

            RuleFor(x => x.RoundNumDecimals)
                .InclusiveBetween(0, 8)
                .When(x => x.RoundOrderItemsEnabled)
                .WithMessage(T("Admin.Configuration.Currencies.Fields.RoundOrderItemsEnabled.Validation"));
        }
    }
}