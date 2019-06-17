using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.PayPal.Settings;

namespace SmartStore.PayPal.Services
{
    public partial class PayPalService
    {
        public FinancingOptions GetFinancingOptions(PayPalApiSettingsBase settings, PayPalSessionData session, decimal amount)
        {
            var result = new FinancingOptions();
            var dc = decimal.Zero;
            var data = new Dictionary<string, object>();
            var store = _services.StoreContext.CurrentStore;
            var currencyCode = store.PrimaryStoreCurrency.CurrencyCode;
            var merchantCountry = _countryService.Value.GetCountryById(_companyInfoSettings.Value.CountryId) ?? _countryService.Value.GetAllCountries().FirstOrDefault();
            var allCurrencies = _currencyService.GetAllCurrencies(true).ToDictionarySafe(x => x.CurrencyCode, x => x);

            var transactionAmount = new Dictionary<string, object>();
            transactionAmount.Add("value", amount.FormatInvariant());
            transactionAmount.Add("currency_code", currencyCode);

            data.Add("financing_country_code", merchantCountry.TwoLetterIsoCode);
            data.Add("transaction_amount", transactionAmount);

            var response = CallApi("POST", "/v1/credit/calculated-financing-options", session.AccessToken, settings, JsonConvert.SerializeObject(data));
            if (!response.Success)
            {
                return null;
            }

            foreach (var fo in response.Json.qualifying_financing_options)
            {
                var option = new FinancingOptions.Option();

                if (decimal.TryParse(((string)fo.credit_financing.apr).EmptyNull(), NumberStyles.Number, CultureInfo.InvariantCulture, out dc))
                {
                    option.AnnualPercentageRate = dc;
                }
                if (decimal.TryParse(((string)fo.credit_financing.nominal_rate).EmptyNull(), NumberStyles.Number, CultureInfo.InvariantCulture, out dc))
                {
                    option.NominalRate = dc;
                }

                option.Term = ((string)fo.credit_financing.term).ToInt();
                option.MinAmount = Parse((string)fo.min_amount.value, (string)fo.min_amount.currency_code, allCurrencies);
                option.MonthlyPayment = Parse((string)fo.monthly_payment.value, (string)fo.monthly_payment.currency_code, allCurrencies);
                option.TotalInterest = Parse((string)fo.total_interest.value, (string)fo.total_interest.currency_code, allCurrencies);
                option.TotalCost = Parse((string)fo.total_cost.value, (string)fo.total_cost.currency_code, allCurrencies);

                result.Qualified.Add(option);
            }

            return result;
        }
    }


    public class FinancingOptions
    {
        public FinancingOptions()
        {
            Qualified = new List<Option>();
        }

        public List<Option> Qualified { get; set; }
        public PayPalPromotion Promotion { get; set; }
        public string Lender { get; set; }

        public class Option
        {
            public decimal AnnualPercentageRate { get; set; }
            public decimal NominalRate { get; set; }
            public int Term { get; set; }
            public Money MinAmount { get; set; }
            public Money MonthlyPayment { get; set; }
            public Money TotalInterest { get; set; }
            public Money TotalCost { get; set; }
        }
    }
}