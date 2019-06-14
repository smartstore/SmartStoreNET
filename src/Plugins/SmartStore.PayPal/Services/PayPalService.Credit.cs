using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.PayPal.Settings;

namespace SmartStore.PayPal.Services
{
    public partial class PayPalService
    {
        public PayPalResponse GetFinancingOptions(PayPalApiSettingsBase settings, PayPalSessionData session, decimal amount)
        {
            var data = new Dictionary<string, object>();
            var store = _services.StoreContext.CurrentStore;
            var currencyCode = store.PrimaryStoreCurrency.CurrencyCode;
            var merchantCountry = _countryService.Value.GetCountryById(_companyInfoSettings.Value.CountryId) ?? _countryService.Value.GetAllCountries().FirstOrDefault();

            var transactionAmount = new Dictionary<string, object>();
            transactionAmount.Add("value", amount.FormatInvariant());
            transactionAmount.Add("currency_code", currencyCode);

            data.Add("financing_country_code", merchantCountry.TwoLetterIsoCode);
            data.Add("transaction_amount", transactionAmount);

            var result = CallApi("POST", "/v1/credit/calculated-financing-options", session.AccessToken, settings, JsonConvert.SerializeObject(data));
            return result;
        }
    }
}