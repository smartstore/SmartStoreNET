using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Infrastructure;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Plugins;
using Autofac;
using System.Linq;

namespace SmartStore.PayPal.Services
{

    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public class PayPalExpressApiService : IPayPalExpressApiService
    {
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGiftCardService _giftCardService;
        private readonly ICommonServices _services;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShippingService _shippingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly PluginHelper _helper;

        public PayPalExpressApiService( 
            ISettingService settingService,
			IStoreService storeService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            ICustomerService customerService, 
            IWorkContext workContext,
            IStoreContext storeContext,
            IGiftCardService giftCardService,
            ICommonServices services,
            IStateProvinceService stateProvinceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShippingService shippingService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IPriceCalculationService priceCalculationService,
            IComponentContext ctx)
        {
            _settingService = settingService;
            _storeService = storeService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _customerService = customerService;
            _workContext = workContext;
            _storeContext = storeContext;
            _giftCardService = giftCardService;
            _services = services;
            _stateProvinceService = stateProvinceService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shippingService = shippingService;
            _genericAttributeService = genericAttributeService;
            _countryService = countryService;
            _priceCalculationService = priceCalculationService;

			_helper = new PluginHelper(ctx, "SmartStore.PayPal");
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest request)
        {
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);
            var doPayment = DoExpressCheckoutPayment(request);
            var result = new ProcessPaymentResult();

            if (doPayment.Ack == AckCodeType.Success)
            {
                if (PayPalHelper.GetPaymentAction(settings) == PaymentActionCodeType.Authorization)
                {
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                }
                result.AuthorizationTransactionId = request.PaypalToken;
                result.CaptureTransactionId = doPayment.DoExpressCheckoutPaymentResponseDetails.PaymentInfo.FirstOrDefault().TransactionID;
                result.CaptureTransactionResult = doPayment.Ack.ToString();
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
            }

            return result;
        }

        public void PostProcessPayment(PostProcessPaymentRequest request) 
        { 
        
        }

        public CapturePaymentResult Capture(CapturePaymentRequest request)
        {
            var result = new CapturePaymentResult();
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);

            string authorizationId = request.Order.AuthorizationTransactionId;
            var req = new DoCaptureReq();
            req.DoCaptureRequest = new DoCaptureRequestType();
            req.DoCaptureRequest.Version = PayPalHelper.GetApiVersion();
            req.DoCaptureRequest.AuthorizationID = authorizationId;
            req.DoCaptureRequest.Amount = new BasicAmountType();
            req.DoCaptureRequest.Amount.Value = Math.Round(request.Order.OrderTotal, 2).ToString("N", new CultureInfo("en-us"));
            req.DoCaptureRequest.Amount.currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            req.DoCaptureRequest.CompleteType = CompleteCodeType.Complete;

            using (var service = new PayPalAPIAASoapBinding())
            {
                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType();
                service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = settings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = settings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = settings.Signature;
                service.RequesterCredentials.Credentials.Subject = "";

                DoCaptureResponseType response = service.DoCapture(req);

                string error = "";
                bool success = PayPalHelper.CheckSuccess(_helper, response, out error);
                if (success)
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    result.CaptureTransactionId = response.DoCaptureResponseDetails.PaymentInfo.TransactionID;
                    result.CaptureTransactionResult = response.Ack.ToString();
                }
                else
                {
                    result.AddError(error);
                }
            }
            return result;
        }

        public VoidPaymentResult Void(VoidPaymentRequest request)
        {
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);
            var result = new VoidPaymentResult();

            string transactionId = request.Order.AuthorizationTransactionId;
            if (String.IsNullOrEmpty(transactionId))
                transactionId = request.Order.CaptureTransactionId;

            var req = new DoVoidReq();
            req.DoVoidRequest = new DoVoidRequestType();
            req.DoVoidRequest.Version = PayPalHelper.GetApiVersion();
            req.DoVoidRequest.AuthorizationID = transactionId;


            using (var service = new PayPalAPIAASoapBinding())
            {
                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType();
                service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = settings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = settings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = settings.Signature;
                service.RequesterCredentials.Credentials.Subject = "";

                DoVoidResponseType response = service.DoVoid(req);

                string error = "";
                bool success = PayPalHelper.CheckSuccess(_helper, response, out error);
                if (success)
                {
                    result.NewPaymentStatus = PaymentStatus.Voided;
                    //result.VoidTransactionID = response.RefundTransactionID;
                }
                else
                {
                    result.AddError(error);
                }
            }
            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest request)
        {

            var result = new RefundPaymentResult();
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);

            string transactionId = request.Order.CaptureTransactionId;

            var req = new RefundTransactionReq();
            req.RefundTransactionRequest = new RefundTransactionRequestType();
            //NOTE: Specify amount in partial refund
            req.RefundTransactionRequest.RefundType = RefundType.Full;
            req.RefundTransactionRequest.RefundTypeSpecified = true;
            req.RefundTransactionRequest.Version = PayPalHelper.GetApiVersion();
            req.RefundTransactionRequest.TransactionID = transactionId;

            using (var service = new PayPalAPISoapBinding())
            {
                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType();
                service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = settings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = settings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = settings.Signature;
                service.RequesterCredentials.Credentials.Subject = "";

                RefundTransactionResponseType response = service.RefundTransaction(req);

                string error = string.Empty;
                bool Success = PayPalHelper.CheckSuccess(_helper, response, out error);
                if (Success)
                {
                    result.NewPaymentStatus = PaymentStatus.Refunded;
                    //cancelPaymentResult.RefundTransactionID = response.RefundTransactionID;
                }
                else
                {
                    result.AddError(error);
                }
            }

            return result;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest request)
        {
        
            var result = new CancelRecurringPaymentResult();
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);
            var order = request.Order;

            var req = new ManageRecurringPaymentsProfileStatusReq();
            req.ManageRecurringPaymentsProfileStatusRequest = new ManageRecurringPaymentsProfileStatusRequestType();
            req.ManageRecurringPaymentsProfileStatusRequest.Version = PayPalHelper.GetApiVersion();
            var details = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
            req.ManageRecurringPaymentsProfileStatusRequest.ManageRecurringPaymentsProfileStatusRequestDetails = details;

            details.Action = StatusChangeActionType.Cancel;
            //Recurring payments profile ID returned in the CreateRecurringPaymentsProfile response
            details.ProfileID = order.SubscriptionTransactionId;

            using (var service = new PayPalAPIAASoapBinding())
            {
                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType();
                service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = settings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = settings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = settings.Signature;
                service.RequesterCredentials.Credentials.Subject = "";

                var response = service.ManageRecurringPaymentsProfileStatus(req);

                string error = "";
                if (!PayPalHelper.CheckSuccess(_helper, response, out error))
                {
                    result.AddError(error);
                }
            }

            return result;
        }

        public SetExpressCheckoutResponseType SetExpressCheckout(PayPalProcessPaymentRequest processPaymentRequest,
            IList<Core.Domain.Orders.OrganizedShoppingCartItem> cart)
        {
            var result = new SetExpressCheckoutResponseType();
            var currentStore = _storeContext.CurrentStore;
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);

            var req = new SetExpressCheckoutReq
            {
                SetExpressCheckoutRequest = new SetExpressCheckoutRequestType
                {
                    Version = PayPalHelper.GetApiVersion(),
                    SetExpressCheckoutRequestDetails = new SetExpressCheckoutRequestDetailsType()
                }
            };

            var details = new SetExpressCheckoutRequestDetailsType
            {
                PaymentAction = PayPalHelper.GetPaymentAction(settings),
                PaymentActionSpecified = true,
                CancelURL = _services.WebHelper.GetStoreLocation(currentStore.SslEnabled) + "cart",
                ReturnURL = _services.WebHelper.GetStoreLocation(currentStore.SslEnabled) + "Plugins/PayPalExpress/GetDetails",
                //CallbackURL = _webHelper.GetStoreLocation(currentStore.SslEnabled) + "Plugins/PayPalExpress/ShippingOptions?CustomerID=" + _workContext.CurrentCustomer.Id.ToString(),
                //CallbackTimeout = _payPalExpressPaymentSettings.CallbackTimeout.ToString() 
                ReqConfirmShipping = settings.ConfirmedShipment.ToString(),
                NoShipping = settings.NoShipmentAddress.ToString()
            };

            // populate cart
            decimal itemTotal = decimal.Zero;
            var cartItems = new List<PaymentDetailsItemType>();
            foreach (OrganizedShoppingCartItem item in cart)
            {
                decimal shoppingCartUnitPriceWithDiscountBase = _priceCalculationService.GetUnitPrice(item, true);
                decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                decimal priceIncludingTier = shoppingCartUnitPriceWithDiscount;
                cartItems.Add(new PaymentDetailsItemType()
                {
                    Name = item.Item.Product.Name,
                    Number = item.Item.Product.Sku,
                    Quantity = item.Item.Quantity.ToString(),
                    Amount = new BasicAmountType()  // this is the per item cost
                    {
                        currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)),
                        Value = (priceIncludingTier).ToString("N", new CultureInfo("en-us"))
                    }
                });
                itemTotal += (item.Item.Quantity * priceIncludingTier);
            };

            decimal shippingTotal = decimal.Zero;
            if (cart.RequiresShipping())
            {
                decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                if (shoppingCartShippingBase.HasValue && shoppingCartShippingBase > 0)
                {
                    shippingTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                }
                else
                {
                    shippingTotal = settings.DefaultShippingPrice;
                }
            }

            //This is the default if the Shipping Callback fails
            //var shippingOptions = new List<ShippingOptionType>();
            //shippingOptions.Add(new ShippingOptionType()
            //{
            //    ShippingOptionIsDefault = "true",
            //    ShippingOptionName = "Standard Shipping",
            //    ShippingOptionAmount = new BasicAmountType()
            //    {
            //        Value = shippingTotal.ToString(), //This is the default value used for shipping if the Instant Update API returns an error or does not answer within the callback time
            //        currencyID = PaypalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))
            //    }
            //});
            //details.FlatRateShippingOptions = shippingOptions.ToArray();
            //details.TotalType = TotalType.EstimatedTotal;

            // get total tax
            SortedDictionary<decimal, decimal> taxRates = null;
            decimal shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out taxRates);
            decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);
            decimal discount = -processPaymentRequest.Discount;


            // Add discounts to PayPal order
            if (discount != 0)
            {
                cartItems.Add(new PaymentDetailsItemType()
                {
                    Name = "Threadrock Discount",
                    Quantity = "1",
                    Amount = new BasicAmountType() // this is the total discount
                    {
                        currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)),
                        Value = discount.ToString("N", new CultureInfo("en-us"))
                    }
                });

                itemTotal += discount;
            }

            // get customer
            int customerId = Convert.ToInt32(_workContext.CurrentCustomer.Id.ToString());
            var customer = _customerService.GetCustomerById(customerId);

            if (!cart.IsRecurring())
            {
                //we don't apply gift cards for recurring products
                var giftCards = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer);
                if (giftCards != null)
                {
                    foreach (var gc in giftCards)
                    {
                        if (itemTotal > decimal.Zero)
                        {
                            decimal remainingAmount = gc.GetGiftCardRemainingAmount();
                            decimal amountCanBeUsed = decimal.Zero;
                            if (itemTotal > remainingAmount)
                                amountCanBeUsed = remainingAmount;
                            else
                                amountCanBeUsed = itemTotal - .01M;

                            decimal amountToSubtract = -amountCanBeUsed;

                            cartItems.Add(new PaymentDetailsItemType()
                            {
                                Name = "Giftcard Applied",
                                Quantity = "1",
                                Amount = new BasicAmountType()
                                {
                                    currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)),
                                    Value = amountToSubtract.ToString("N", new CultureInfo("en-us"))
                                }
                            });

                            //reduce subtotal
                            itemTotal += amountToSubtract;
                        }
                    }
                }
            }

            // populate payment details
            var paymentDetails = new PaymentDetailsType
            {
                ItemTotal = new BasicAmountType
                {
                    Value = Math.Round(itemTotal, 2).ToString("N", new CultureInfo("en-us")),
                    currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))
                },
                ShippingTotal = new BasicAmountType
                {
                    Value = Math.Round(shippingTotal, 2).ToString("N", new CultureInfo("en-us")),
                    currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))
                },
                TaxTotal = new BasicAmountType
                {
                    Value = Math.Round(shoppingCartTax, 2).ToString("N", new CultureInfo("en-us")),
                    currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))
                },
                OrderTotal = new BasicAmountType
                {
                    Value = Math.Round(itemTotal + shoppingCartTax + shippingTotal, 2).ToString("N", new CultureInfo("en-us")),
                    currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))
                },
                Custom = processPaymentRequest.OrderGuid.ToString(),
                ButtonSource = SmartStoreVersion.CurrentFullVersion,
                PaymentAction = PayPalHelper.GetPaymentAction(settings),
                PaymentDetailsItem = cartItems.ToArray()
            };
            details.PaymentDetails = new[] { paymentDetails };
            //details.MaxAmount = new BasicAmountType()  // this is the per item cost
            //{
            //    currencyID = PaypalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)),
            //    Value = (decimal.Parse(paymentDetails.OrderTotal.Value) + 30).ToString()
            //};

            details.ShippingMethodSpecified = true;

            req.SetExpressCheckoutRequest.SetExpressCheckoutRequestDetails.Custom = processPaymentRequest.OrderGuid.ToString();
            req.SetExpressCheckoutRequest.SetExpressCheckoutRequestDetails = details;


            using (var service = new PayPalAPIAASoapBinding())
            {
                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType
                {
                    Credentials = new UserIdPasswordType
                    {
                        Username = settings.ApiAccountName,
                        Password = settings.ApiAccountPassword,
                        Signature = settings.Signature,
                        Subject = ""
                    }
                };

                result = service.SetExpressCheckout(req);
                result.Token = result.Any.InnerText;
            }
            return result;
        }

        public GetExpressCheckoutDetailsResponseType GetExpressCheckoutDetails(string token)
        {
            var result = new GetExpressCheckoutDetailsResponseType();
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);

            using (var service = new PayPalAPIAASoapBinding())
            {
                var req = new GetExpressCheckoutDetailsReq();
                req.GetExpressCheckoutDetailsRequest = new GetExpressCheckoutDetailsRequestType
                {
                    Token = token,
                    Version = PayPalHelper.GetApiVersion()
                };

                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType
                {
                    Credentials = new UserIdPasswordType
                    {
                        Username = settings.ApiAccountName,
                        Password = settings.ApiAccountPassword,
                        Signature = settings.Signature,
                        Subject = ""
                    }
                };

                result = service.GetExpressCheckoutDetails(req);
            }
            return result;
        }

        public ProcessPaymentRequest SetCheckoutDetails(ProcessPaymentRequest processPaymentRequest, GetExpressCheckoutDetailsResponseDetailsType checkoutDetails)
        {
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);
            int customerId = Convert.ToInt32(_workContext.CurrentCustomer.Id.ToString());
            var customer = _customerService.GetCustomerById(customerId);

            _workContext.CurrentCustomer = customer;

            //var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            // get/update billing address
            string billingFirstName = checkoutDetails.PayerInfo.PayerName.FirstName;
            string billingLastName = checkoutDetails.PayerInfo.PayerName.LastName;
            string billingEmail = checkoutDetails.PayerInfo.Payer;
            string billingAddress1 = checkoutDetails.PayerInfo.Address.Street1;
            string billingAddress2 = checkoutDetails.PayerInfo.Address.Street2;
            string billingPhoneNumber = checkoutDetails.PayerInfo.ContactPhone;
            string billingCity = checkoutDetails.PayerInfo.Address.CityName;
            int? billingStateProvinceId = null;
            var billingStateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(checkoutDetails.PayerInfo.Address.StateOrProvince);
            if (billingStateProvince != null)
                billingStateProvinceId = billingStateProvince.Id;
            string billingZipPostalCode = checkoutDetails.PayerInfo.Address.PostalCode;
            int? billingCountryId = null;
            var billingCountry = _countryService.GetCountryByTwoLetterIsoCode(checkoutDetails.PayerInfo.Address.Country.ToString());
            if (billingCountry != null)
                billingCountryId = billingCountry.Id;

            var billingAddress = customer.Addresses.ToList().FindAddress(
                billingFirstName, billingLastName, billingPhoneNumber,
                billingEmail, string.Empty, string.Empty, billingAddress1, billingAddress2, billingCity,
                billingStateProvinceId, billingZipPostalCode, billingCountryId);

            if (billingAddress == null)
            {
                billingAddress = new Core.Domain.Common.Address()
                {
                    FirstName = billingFirstName,
                    LastName = billingLastName,
                    PhoneNumber = billingPhoneNumber,
                    Email = billingEmail,
                    FaxNumber = string.Empty,
                    Company = string.Empty,
                    Address1 = billingAddress1,
                    Address2 = billingAddress2,
                    City = billingCity,
                    StateProvinceId = billingStateProvinceId,
                    ZipPostalCode = billingZipPostalCode,
                    CountryId = billingCountryId,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                customer.Addresses.Add(billingAddress);
            }

            //set default billing address
            customer.BillingAddress = billingAddress;
            _customerService.UpdateCustomer(customer);

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, null);

            bool shoppingCartRequiresShipping = cart.RequiresShipping();
            if (shoppingCartRequiresShipping)
            {
                var paymentDetails = checkoutDetails.PaymentDetails.FirstOrDefault();
                string[] shippingFullname = paymentDetails.ShipToAddress.Name.Trim().Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                string shippingFirstName = shippingFullname[0];
                string shippingLastName = string.Empty;
                if (shippingFullname.Length > 1)
                    shippingLastName = shippingFullname[1];
                string shippingEmail = checkoutDetails.PayerInfo.Payer;
                string shippingAddress1 = paymentDetails.ShipToAddress.Street1;
                string shippingAddress2 = paymentDetails.ShipToAddress.Street2;
                string shippingPhoneNumber = paymentDetails.ShipToAddress.Phone;
                string shippingCity = paymentDetails.ShipToAddress.CityName;
                int? shippingStateProvinceId = null;
                var shippingStateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(paymentDetails.ShipToAddress.StateOrProvince);
                if (shippingStateProvince != null)
                    shippingStateProvinceId = shippingStateProvince.Id;
                int? shippingCountryId = null;
                string shippingZipPostalCode = paymentDetails.ShipToAddress.PostalCode;
                var shippingCountry = _countryService.GetCountryByTwoLetterIsoCode(paymentDetails.ShipToAddress.Country.ToString());
                if (shippingCountry != null)
                    shippingCountryId = shippingCountry.Id;

                var shippingAddress = customer.Addresses.ToList().FindAddress(
                    shippingFirstName, shippingLastName, shippingPhoneNumber,
                    shippingEmail, string.Empty, string.Empty,
                    shippingAddress1, shippingAddress2, shippingCity,
                    shippingStateProvinceId, shippingZipPostalCode, shippingCountryId);

                if (shippingAddress == null)
                {
                    shippingAddress = new Core.Domain.Common.Address()
                    {
                        FirstName = shippingFirstName,
                        LastName = shippingLastName,
                        PhoneNumber = shippingPhoneNumber,
                        Email = shippingEmail,
                        FaxNumber = string.Empty,
                        Company = string.Empty,
                        Address1 = shippingAddress1,
                        Address2 = shippingAddress2,
                        City = shippingCity,
                        StateProvinceId = shippingStateProvinceId,
                        ZipPostalCode = shippingZipPostalCode,
                        CountryId = shippingCountryId,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                    customer.Addresses.Add(shippingAddress);
                }

                customer.ShippingAddress = shippingAddress;
                _customerService.UpdateCustomer(customer);
            }

            bool isShippingSet = false;
            GetShippingOptionResponse getShippingOptionResponse = _shippingService.GetShippingOptions(cart, customer.ShippingAddress);

            if (checkoutDetails.UserSelectedOptions != null)
            {
                if (getShippingOptionResponse.Success && getShippingOptionResponse.ShippingOptions.Count > 0)
                {
                    foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                    {
                        if (checkoutDetails.UserSelectedOptions.ShippingOptionName.Contains(shippingOption.Name) &&
                            checkoutDetails.UserSelectedOptions.ShippingOptionName.Contains(shippingOption.Description))
                        {
                            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption);
                            isShippingSet = true;
                            break;
                        }

                    }
                }

                if (!isShippingSet)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = checkoutDetails.UserSelectedOptions.ShippingOptionName;
                    decimal shippingPrice = settings.DefaultShippingPrice;
                    decimal.TryParse(checkoutDetails.UserSelectedOptions.ShippingOptionAmount.Value, out shippingPrice);
                    shippingOption.Rate = shippingPrice;
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption);
                }
            }

            processPaymentRequest.PaypalPayerId = checkoutDetails.PayerInfo.PayerID;


            return processPaymentRequest;
        }

        /// <summary>
        /// Verifies IPN
        /// </summary>
        /// <param name="formString">Form string</param>
        /// <param name="values">Values</param>
        /// <returns>Result</returns>
        public bool VerifyIPN(string formString, out Dictionary<string, string> values)
        {
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);
            var req = (HttpWebRequest)WebRequest.Create(PayPalHelper.GetPaypalUrl(settings));
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = HttpContext.Current.Request.UserAgent;

            string formContent = string.Format("{0}&cmd=_notify-validate", formString);
            req.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
            {
                sw.Write(formContent);
            }

            string response = null;
            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                response = HttpUtility.UrlDecode(sr.ReadToEnd());
            }
            bool success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string l in formString.Split('&'))
            {
                string line = HttpUtility.UrlDecode(l).Trim();
                int equalPox = line.IndexOf('=');
                if (equalPox >= 0)
                    values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
            }

            return success;
        }

        public DoExpressCheckoutPaymentResponseType DoExpressCheckoutPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new DoExpressCheckoutPaymentResponseType();
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);

            // populate payment details
            var paymentDetails = new PaymentDetailsType
            {
                OrderTotal = new BasicAmountType
                {
                    Value = Math.Round(processPaymentRequest.OrderTotal, 2).ToString("N", new CultureInfo("en-us")),
                    currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))
                },
                Custom = processPaymentRequest.OrderGuid.ToString(),
                ButtonSource = SmartStoreVersion.CurrentFullVersion
            };

            // build the request
            var req = new DoExpressCheckoutPaymentReq
            {
                DoExpressCheckoutPaymentRequest = new DoExpressCheckoutPaymentRequestType
                {
                    Version = PayPalHelper.GetApiVersion(),
                    DoExpressCheckoutPaymentRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType
                    {
                        Token = processPaymentRequest.PaypalToken,
                        PayerID = processPaymentRequest.PaypalPayerId,
                        PaymentAction = PayPalHelper.GetPaymentAction(settings),
                        PaymentActionSpecified = true,
                        PaymentDetails = new PaymentDetailsType[]
                        {
                            paymentDetails
                        }
                    }
                }
            };

            //execute request
            using (var service = new PayPalAPIAASoapBinding())
            {
                service.Url = PayPalHelper.GetPaypalServiceUrl(settings);

                service.RequesterCredentials = new CustomSecurityHeaderType
                {
                    Credentials = new UserIdPasswordType
                    {
                        Username = settings.ApiAccountName,
                        Password = settings.ApiAccountPassword,
                        Signature = settings.Signature,
                        Subject = ""
                    }
                };

                result = service.DoExpressCheckoutPayment(req);
            }
            return result;
        }

    }
}

