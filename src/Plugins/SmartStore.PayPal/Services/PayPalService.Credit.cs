using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Settings;

namespace SmartStore.PayPal.Services
{
    public partial class PayPalService
    {
        public FinancingOptions GetFinancingOptions(PayPalInstalmentsSettings settings, PayPalSessionData session, PayPalPromotion promotion, decimal amount)
        {
            var result = new FinancingOptions
            {
                Promotion = promotion,
                Lender = settings.Lender
            };

            var store = _services.StoreContext.CurrentStore;
            var sourceCurrency = store.PrimaryStoreCurrency;
            var targetCurrency = _services.WorkContext.WorkingCurrency;

            result.NetLoanAmount = new Money(_currencyService.ConvertCurrency(amount, sourceCurrency, targetCurrency, store), targetCurrency);

            if (promotion == PayPalPromotion.FinancingExample)
            {
                //result.Qualified.Add(new FinancingOptions.Option
                //{
                //    AnnualPercentageRate = 9.87M,
                //    NominalRate = 9.75M,
                //    Term = 24,
                //    MinAmount = new Money(_currencyService.ConvertCurrency(65.5M, sourceCurrency, targetCurrency, store), targetCurrency),
                //    MonthlyPayment = new Money(_currencyService.ConvertCurrency(24.5M, sourceCurrency, targetCurrency, store), targetCurrency),
                //    TotalInterest = new Money(_currencyService.ConvertCurrency(8.5M, sourceCurrency, targetCurrency, store), targetCurrency),
                //    TotalCost = new Money(_currencyService.ConvertCurrency(324.0M, sourceCurrency, targetCurrency, store), targetCurrency),
                //});

                var response = EnsureAccessToken(session, settings);
                if (response.Success)
                {
                    var dc = decimal.Zero;
                    var data = new Dictionary<string, object>();
                    var transactionAmount = new Dictionary<string, object>();
                    transactionAmount.Add("value", amount.FormatInvariant());
                    transactionAmount.Add("currency_code", store.PrimaryStoreCurrency.CurrencyCode);

                    var merchantCountry = _countryService.Value.GetCountryById(_companyInfoSettings.Value.CountryId) ?? _countryService.Value.GetAllCountries().FirstOrDefault();
                    data.Add("financing_country_code", merchantCountry.TwoLetterIsoCode);
                    data.Add("transaction_amount", transactionAmount);

                    response = CallApi("POST", "/v1/credit/calculated-financing-options", settings, session, JsonConvert.SerializeObject(data));

                    if (response.Success && response.Json.financing_options != null)
                    {
                        foreach (var fo in response.Json.financing_options[0].qualifying_financing_options)
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
                            option.MinAmount = Parse((string)fo.min_amount.value, sourceCurrency, targetCurrency, store);
                            option.MonthlyPayment = Parse((string)fo.monthly_payment.value, sourceCurrency, targetCurrency, store);
                            option.TotalInterest = Parse((string)fo.total_interest.value, sourceCurrency, targetCurrency, store);
                            option.TotalCost = Parse((string)fo.total_cost.value, sourceCurrency, targetCurrency, store);

                            result.Qualified.Add(option);
                        }
                    }
                }
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
        public Money NetLoanAmount { get; set; }

        public class Option
        {
            public decimal AnnualPercentageRate { get; set; }
            public decimal NominalRate { get; set; }
            public decimal BorrowingRate { get; set; }
            public int Term { get; set; }
            public Money MinAmount { get; set; }
            public Money MonthlyPayment { get; set; }
            public Money TotalInterest { get; set; }
            public Money TotalCost { get; set; }
        }
    }
}