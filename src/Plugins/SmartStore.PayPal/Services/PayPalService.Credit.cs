using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Orders;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Customers;

namespace SmartStore.PayPal.Services
{
    public partial class PayPalService
    {
        public FinancingOptions GetFinancingOptions(
            PayPalInstalmentsSettings settings,
            PayPalSessionData session,
            string origin,
            decimal amount,
            PayPalPromotion? promotion = null)
        {
            Guard.NotNull(settings, nameof(settings));
            Guard.NotNull(session, nameof(session));
            Guard.NotEmpty(origin, nameof(origin));

            var store = _services.StoreContext.CurrentStore;

            // Get promotion type.
            if (!promotion.HasValue)
            {
                switch (origin)
                {
                    case "productpage":
                        promotion = settings.ProductPagePromotion;
                        break;
                    case "cart":
                        promotion = settings.CartPagePromotion;
                        break;
                    case "paymentinfo":
                        promotion = settings.PaymentListPromotion;
                        break;
                }
            }

            if (!promotion.HasValue || settings.ClientId.IsEmpty() || settings.Secret.IsEmpty())
            {
                return null;
            }

            // Get financing amount.
            if (origin == "cart" || origin == "paymentinfo")
            {
                var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);
                decimal? cartTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, usePaymentMethodAdditionalFee: false);
                if (!cartTotal.HasValue)
                {
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, out _, out _, out _, out amount);
                }
                else
                {
                    amount = cartTotal.Value;
                }
            }

            if (!settings.IsAmountFinanceable(amount))
            {
                return null;
            }

            // Get financing options from API.
            var result = new FinancingOptions(origin)
            {
                Promotion = promotion,
                Lender = settings.Lender
            };

            var sourceCurrency = store.PrimaryStoreCurrency;
            var targetCurrency = _services.WorkContext.WorkingCurrency;

            result.NetLoanAmount = new Money(_currencyService.ConvertCurrency(amount, sourceCurrency, targetCurrency, store), targetCurrency);

            if (promotion == PayPalPromotion.FinancingExample)
            {
                var response = EnsureAccessToken(session, settings);
                if (response.Success)
                {
                    var index = 0;
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

                            option.MonthlyPayment = Parse((string)fo.monthly_payment.value, sourceCurrency, targetCurrency, store);

                            if (option.MonthlyPayment.Amount > decimal.Zero)
                            {
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
                                option.TotalInterest = Parse((string)fo.total_interest.value, sourceCurrency, targetCurrency, store);
                                option.TotalCost = Parse((string)fo.total_cost.value, sourceCurrency, targetCurrency, store);

                                // PayPal review: do not display last instalment.
                                //var instalments = fo.estimated_installments as JArray;
                                //var lastInstalment = instalments?.LastOrDefault()?.SelectToken("total_payment.value")?.ToString();
                                //option.LastInstalment = Parse(lastInstalment, sourceCurrency, targetCurrency, store);

                                //if (option.LastInstalment.Amount == decimal.Zero)
                                //{
                                //    option.LastInstalment = new Money(option.MonthlyPayment.Amount, targetCurrency);
                                //}

                                result.Qualified.Add(option);
                            }
                        }

                        result.Qualified = result.Qualified
                            .OrderBy(x => x.Term)
                            .ThenBy(x => x.MonthlyPayment.Amount)
                            .ToList();

                        result.Qualified.Each(x => x.Index = ++index);
                    }
                }
            }

            return result;
        }
    }


    public class FinancingOptions
    {
        public FinancingOptions(string origin)
        {
            Origin = origin;
            Qualified = new List<Option>();
        }

        public string Origin { get; private set; }
        public PayPalPromotion? Promotion { get; set; }
        public string Lender { get; set; }
        public Money NetLoanAmount { get; set; }
        public List<Option> Qualified { get; set; }

        public Option GetDefaultOption(bool preferZeroRate = true)
        {
            Option option = null;

            try
            {
                if (preferZeroRate)
                {
                    option = Qualified
                        .Where(x => x.AnnualPercentageRate == decimal.Zero)
                        .OrderByDescending(x => x.MonthlyPayment.Amount)
                        .FirstOrDefault();
                }

                if (option == null)
                {
                    option = Qualified
                        .OrderByDescending(x => x.AnnualPercentageRate)
                        .ThenBy(x => x.MonthlyPayment.Amount)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ex.Dump();
            }

            return option;
        }

        public class Option
        {
            public int Index { get; set; }
            public decimal AnnualPercentageRate { get; set; }
            public decimal NominalRate { get; set; }
            public int Term { get; set; }
            public Money MinAmount { get; set; }
            public Money MonthlyPayment { get; set; }
            //public Money LastInstalment { get; set; }
            public Money TotalInterest { get; set; }
            public Money TotalCost { get; set; }

            public override string ToString()
            {
                return $"{Term} months (effective {AnnualPercentageRate}, nominal {NominalRate}): monthly {MonthlyPayment.ToString()}, total {TotalCost.ToString()}, interest {TotalInterest.ToString()}";
            }
        }
    }
}