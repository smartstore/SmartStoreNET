using System;
using System.Globalization;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Price formatter
    /// </summary>
    public partial class PriceFormatter : IPriceFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly TaxSettings _taxSettings;

        public PriceFormatter(IWorkContext workContext,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            TaxSettings taxSettings)
        {
            _workContext = workContext;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _taxSettings = taxSettings;
        }

        #region Methods

        public string FormatPrice(decimal price)
        {
            return FormatPrice(price, true, _workContext.WorkingCurrency);
        }

        public string FormatPrice(decimal price, bool showCurrency, Currency targetCurrency)
        {
            var language = _workContext.WorkingLanguage;
            bool priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            return FormatPrice(price, showCurrency, targetCurrency, language, priceIncludesTax);
        }

        public string FormatPrice(decimal price, bool showCurrency, bool showTax)
        {
            var targetCurrency = _workContext.WorkingCurrency;
            var language = _workContext.WorkingLanguage;
            bool priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            return FormatPrice(price, showCurrency, targetCurrency, language, priceIncludesTax, showTax);
        }

        public string FormatPrice(decimal price, bool showCurrency, string currencyCode, bool showTax, Language language)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode };
            var priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            return FormatPrice(price, showCurrency, currency, language, priceIncludesTax, showTax);
        }

        public string FormatPrice(decimal price, bool showCurrency, string currencyCode, Language language, bool priceIncludesTax)
        {
            bool showTax = _taxSettings.DisplayTaxSuffix;
            return FormatPrice(price, showCurrency, currencyCode, language, priceIncludesTax, showTax);
        }

        public string FormatPrice(decimal price, bool showCurrency, string currencyCode, Language language, bool priceIncludesTax, bool showTax)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode };
            return FormatPrice(price, showCurrency, currency, language, priceIncludesTax, showTax);
        }

        public string FormatPrice(decimal price, bool showCurrency, Currency targetCurrency, Language language, bool priceIncludesTax)
        {
            bool showTax = _taxSettings.DisplayTaxSuffix;
            return FormatPrice(price, showCurrency, targetCurrency, language, priceIncludesTax, showTax);
        }

        public string FormatPrice(decimal price, bool showCurrency, Currency targetCurrency, Language language, bool priceIncludesTax, bool showTax)
        {
            var formatted = new Money(price, targetCurrency).ToString(showCurrency);

            if (showTax)
            {
                // Show tax suffix
                var resKey = "Products." + (priceIncludesTax ? "InclTaxSuffix" : "ExclTaxSuffix");
                var taxFormatStr = _localizationService.GetResource(resKey, language.Id, false).NullEmpty() ?? (priceIncludesTax ? "{0} incl. tax" : "{0} excl. tax");

                formatted = string.Format(taxFormatStr, formatted);
            }

            return formatted;
        }



        public string FormatShippingPrice(decimal price, bool showCurrency)
        {
            var targetCurrency = _workContext.WorkingCurrency;
            var language = _workContext.WorkingLanguage;
            var priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            return FormatShippingPrice(price, showCurrency, targetCurrency, language, priceIncludesTax);
        }

        public string FormatShippingPrice(decimal price, bool showCurrency, Currency targetCurrency, Language language, bool priceIncludesTax)
        {
            bool showTax = _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix;
            return FormatShippingPrice(price, showCurrency, targetCurrency, language, priceIncludesTax, showTax);
        }

        public string FormatShippingPrice(decimal price, bool showCurrency, string currencyCode, Language language, bool priceIncludesTax, bool showTax)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode };
            return FormatPrice(price, showCurrency, currency, language, priceIncludesTax, showTax);
        }

        public string FormatShippingPrice(decimal price, bool showCurrency, Currency targetCurrency, Language language, bool priceIncludesTax, bool showTax)
        {
            return FormatPrice(price, showCurrency, targetCurrency, language, priceIncludesTax, showTax);
        }

        public string FormatShippingPrice(decimal price, bool showCurrency, string currencyCode, Language language, bool priceIncludesTax)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode };
            return FormatShippingPrice(price, showCurrency, currency, language, priceIncludesTax);
        }



        public string FormatPaymentMethodAdditionalFee(decimal price, bool showCurrency)
        {
            var targetCurrency = _workContext.WorkingCurrency;
            var language = _workContext.WorkingLanguage;
            var priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            return FormatPaymentMethodAdditionalFee(price, showCurrency, targetCurrency, language, priceIncludesTax);
        }

        public string FormatPaymentMethodAdditionalFee(decimal price, bool showCurrency, Currency targetCurrency, Language language, bool priceIncludesTax)
        {
            bool showTax = _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix;
            return FormatPaymentMethodAdditionalFee(price, showCurrency, targetCurrency, language, priceIncludesTax, showTax);
        }

        public string FormatPaymentMethodAdditionalFee(decimal price, bool showCurrency, string currencyCode, Language language, bool priceIncludesTax, bool showTax)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode };
            return FormatPrice(price, showCurrency, currency, language, priceIncludesTax, showTax);
        }

        public string FormatPaymentMethodAdditionalFee(decimal price, bool showCurrency, Currency targetCurrency, Language language, bool priceIncludesTax, bool showTax)
        {
            return FormatPrice(price, showCurrency, targetCurrency, language, priceIncludesTax, showTax);
        }

        public string FormatPaymentMethodAdditionalFee(decimal price, bool showCurrency, string currencyCode, Language language, bool priceIncludesTax)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode };
            return FormatPaymentMethodAdditionalFee(price, showCurrency, currency, language, priceIncludesTax);
        }



        public string FormatTaxRate(decimal taxRate)
        {
            return taxRate.ToString("G29");
        }

        #endregion
    }
}
