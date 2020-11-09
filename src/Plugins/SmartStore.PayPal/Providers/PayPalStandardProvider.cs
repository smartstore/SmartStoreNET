using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
    [SystemName("Payments.PayPalStandard")]
    [FriendlyName("PayPal Standard")]
    [DisplayOrder(1)]
    public partial class PayPalStandardProvider : PaymentPluginBase, IConfigurable
    {
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICommonServices _services;
        private readonly ILogger _logger;

        public PayPalStandardProvider(
            IOrderTotalCalculationService orderTotalCalculationService,
            ICommonServices services,
            ILogger logger)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _services = services;
            _logger = logger;
        }

        public static string SystemName => "Payments.PayPalStandard";

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;

            var settings = _services.Settings.LoadSetting<PayPalStandardPaymentSettings>(processPaymentRequest.StoreId);

            if (settings.BusinessEmail.IsEmpty() || settings.PdtToken.IsEmpty())
            {
                result.AddError(T("Plugins.Payments.PayPalStandard.InvalidCredentials"));
            }

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Paid)
                return;

            var store = _services.StoreService.GetStoreById(postProcessPaymentRequest.Order.StoreId);
            var settings = _services.Settings.LoadSetting<PayPalStandardPaymentSettings>(postProcessPaymentRequest.Order.StoreId);

            var builder = new StringBuilder();
            builder.Append(settings.GetPayPalUrl());

            string orderNumber = postProcessPaymentRequest.Order.GetOrderNumber();
            string cmd = (settings.PassProductNamesAndTotals ? "_cart" : "_xclick");

            builder.AppendFormat("?cmd={0}&business={1}", cmd, HttpUtility.UrlEncode(settings.BusinessEmail));

            if (settings.PassProductNamesAndTotals)
            {
                builder.AppendFormat("&upload=1");

                int index = 0;
                decimal cartTotal = decimal.Zero;
                //var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);

                var lineItems = GetLineItems(postProcessPaymentRequest, out cartTotal);

                AdjustLineItemAmounts(lineItems, postProcessPaymentRequest);

                foreach (var item in lineItems.OrderBy(x => (int)x.Type))
                {
                    ++index;
                    builder.AppendFormat("&item_name_" + index + "={0}", HttpUtility.UrlEncode(item.Name));
                    builder.AppendFormat("&amount_" + index + "={0}", item.AmountRounded.ToString("0.00", CultureInfo.InvariantCulture));
                    builder.AppendFormat("&quantity_" + index + "={0}", item.Quantity);
                }

                #region old code

                //var cartItems = postProcessPaymentRequest.Order.OrderItems;
                //int x = 1;
                //foreach (var item in cartItems)
                //{
                //	var unitPriceExclTax = item.UnitPriceExclTax;
                //	var priceExclTax = item.PriceExclTax;
                //	//round
                //	var unitPriceExclTaxRounded = Math.Round(unitPriceExclTax, 2);

                //	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(item.Product.Name));
                //	builder.AppendFormat("&amount_" + x + "={0}", unitPriceExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
                //	builder.AppendFormat("&quantity_" + x + "={0}", item.Quantity);
                //	x++;
                //	cartTotal += priceExclTax;
                //}

                ////the checkout attributes that have a dollar value and send them to Paypal as items to be paid for
                //foreach (var val in caValues)
                //{
                //	var attPrice = _taxService.GetCheckoutAttributePrice(val, false, postProcessPaymentRequest.Order.Customer);
                //	//round
                //	var attPriceRounded = Math.Round(attPrice, 2);
                //	if (attPrice > decimal.Zero) //if it has a price
                //	{
                //		var ca = val.CheckoutAttribute;
                //		if (ca != null)
                //		{
                //			var attName = ca.Name; //set the name
                //			builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(attName)); //name
                //			builder.AppendFormat("&amount_" + x + "={0}", attPriceRounded.ToString("0.00", CultureInfo.InvariantCulture)); //amount
                //			builder.AppendFormat("&quantity_" + x + "={0}", 1); //quantity
                //			x++;
                //			cartTotal += attPrice;
                //		}
                //	}
                //}

                ////order totals

                ////shipping
                //var orderShippingExclTax = postProcessPaymentRequest.Order.OrderShippingExclTax;
                //var orderShippingExclTaxRounded = Math.Round(orderShippingExclTax, 2);
                //if (orderShippingExclTax > decimal.Zero)
                //{
                //	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(_localizationService.GetResource("Plugins.Payments.PayPalStandard.ShippingFee")));
                //	builder.AppendFormat("&amount_" + x + "={0}", orderShippingExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
                //	builder.AppendFormat("&quantity_" + x + "={0}", 1);
                //	x++;
                //	cartTotal += orderShippingExclTax;
                //}

                ////payment method additional fee
                //var paymentMethodAdditionalFeeExclTax = postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax;
                //var paymentMethodAdditionalFeeExclTaxRounded = Math.Round(paymentMethodAdditionalFeeExclTax, 2);
                //if (paymentMethodAdditionalFeeExclTax > decimal.Zero)
                //{
                //	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(_localizationService.GetResource("Plugins.Payments.PayPalStandard.PaymentMethodFee")));
                //	builder.AppendFormat("&amount_" + x + "={0}", paymentMethodAdditionalFeeExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
                //	builder.AppendFormat("&quantity_" + x + "={0}", 1);
                //	x++;
                //	cartTotal += paymentMethodAdditionalFeeExclTax;
                //}

                ////tax
                //var orderTax = postProcessPaymentRequest.Order.OrderTax;
                //var orderTaxRounded = Math.Round(orderTax, 2);
                //if (orderTax > decimal.Zero)
                //{
                //	//builder.AppendFormat("&tax_1={0}", orderTax.ToString("0.00", CultureInfo.InvariantCulture));

                //	//add tax as item
                //	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(_localizationService.GetResource("Plugins.Payments.PayPalStandard.SalesTax")));
                //	builder.AppendFormat("&amount_" + x + "={0}", orderTaxRounded.ToString("0.00", CultureInfo.InvariantCulture)); //amount
                //	builder.AppendFormat("&quantity_" + x + "={0}", 1); //quantity

                //	cartTotal += orderTax;
                //	x++;
                //}

                #endregion

                if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
                {
                    // Take the difference between what the order total is and what it should be and use that as the "discount".
                    // The difference equals the amount of the gift card and/or reward points used.
                    decimal discountTotal = cartTotal - postProcessPaymentRequest.Order.OrderTotal;
                    discountTotal = Math.Round(discountTotal, 2);

                    // Gift card or rewared point amount applied to cart in Smartstore - shows in Paypal as "discount"
                    builder.AppendFormat("&discount_amount_cart={0}", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
                }
            }
            else
            {
                // Pass order total
                string totalItemName = "{0} {1}".FormatWith(T("Checkout.OrderNumber"), orderNumber);
                builder.AppendFormat("&item_name={0}", HttpUtility.UrlEncode(totalItemName));
                var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);
                builder.AppendFormat("&amount={0}", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            builder.AppendFormat("&custom={0}", postProcessPaymentRequest.Order.OrderGuid);
            builder.AppendFormat("&charset={0}", "utf-8");
            builder.Append(string.Format("&no_note=1&currency_code={0}", HttpUtility.UrlEncode(store.PrimaryStoreCurrency.CurrencyCode)));
            builder.AppendFormat("&invoice={0}", HttpUtility.UrlEncode(orderNumber));
            builder.AppendFormat("&rm=2", new object[0]);

            Address address = null;

            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                address = postProcessPaymentRequest.Order.ShippingAddress ?? postProcessPaymentRequest.Order.BillingAddress;

                // 0 means the buyer is prompted to include a shipping address.
                builder.AppendFormat("&no_shipping={0}", settings.IsShippingAddressRequired ? "2" : "1");
            }
            else
            {
                address = postProcessPaymentRequest.Order.BillingAddress;

                builder.AppendFormat("&no_shipping=1", new object[0]);
            }

            var returnUrl = _services.WebHelper.GetStoreLocation(store.SslEnabled) + "Plugins/SmartStore.PayPal/PayPalStandard/PDTHandler";
            var cancelReturnUrl = _services.WebHelper.GetStoreLocation(store.SslEnabled) + "Plugins/SmartStore.PayPal/PayPalStandard/CancelOrder";
            builder.AppendFormat("&return={0}&cancel_return={1}", HttpUtility.UrlEncode(returnUrl), HttpUtility.UrlEncode(cancelReturnUrl));

            //Instant Payment Notification (server to server message)
            if (settings.EnableIpn)
            {
                string ipnUrl;
                if (String.IsNullOrWhiteSpace(settings.IpnUrl))
                    ipnUrl = _services.WebHelper.GetStoreLocation(store.SslEnabled) + "Plugins/SmartStore.PayPal/PayPalStandard/IPNHandler";
                else
                    ipnUrl = settings.IpnUrl;
                builder.AppendFormat("&notify_url={0}", ipnUrl);
            }

            // Address
            builder.AppendFormat("&address_override={0}", settings.UsePayPalAddress ? "0" : "1");
            builder.AppendFormat("&first_name={0}", HttpUtility.UrlEncode(address.FirstName));
            builder.AppendFormat("&last_name={0}", HttpUtility.UrlEncode(address.LastName));
            builder.AppendFormat("&address1={0}", HttpUtility.UrlEncode(address.Address1));
            builder.AppendFormat("&address2={0}", HttpUtility.UrlEncode(address.Address2));
            builder.AppendFormat("&city={0}", HttpUtility.UrlEncode(address.City));
            //if (!String.IsNullOrEmpty(address.PhoneNumber))
            //{
            //    //strip out all non-digit characters from phone number;
            //    string billingPhoneNumber = System.Text.RegularExpressions.Regex.Replace(address.PhoneNumber, @"\D", string.Empty);
            //    if (billingPhoneNumber.Length >= 10)
            //    {
            //        builder.AppendFormat("&night_phone_a={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(0, 3)));
            //        builder.AppendFormat("&night_phone_b={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(3, 3)));
            //        builder.AppendFormat("&night_phone_c={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(6, 4)));
            //    }
            //}
            if (address.StateProvince != null)
                builder.AppendFormat("&state={0}", HttpUtility.UrlEncode(address.StateProvince.Abbreviation));
            else
                builder.AppendFormat("&state={0}", "");

            if (address.Country != null)
                builder.AppendFormat("&country={0}", HttpUtility.UrlEncode(address.Country.TwoLetterIsoCode));
            else
                builder.AppendFormat("&country={0}", "");

            builder.AppendFormat("&zip={0}", HttpUtility.UrlEncode(address.ZipPostalCode));
            builder.AppendFormat("&email={0}", HttpUtility.UrlEncode(address.Email));

            postProcessPaymentRequest.RedirectUrl = builder.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public override bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.PaymentStatus == PaymentStatus.Pending && (DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds > 5)
            {
                return true;
            }
            return true;
        }

        public override Type GetControllerType()
        {
            return typeof(PayPalStandardController);
        }

        public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            var result = decimal.Zero;
            try
            {
                var settings = _services.Settings.LoadSetting<PayPalStandardPaymentSettings>(_services.StoreContext.CurrentStore.Id);

                result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
            }
            catch (Exception)
            {
            }
            return result;
        }

        /// <summary>
        /// Gets PDT details
        /// </summary>
        /// <param name="tx">TX</param>
        /// <param name="values">Values</param>
        /// <param name="response">Response</param>
        /// <returns>Result</returns>
        public bool GetPDTDetails(string tx, PayPalStandardPaymentSettings settings, out Dictionary<string, string> values, out string response)
        {
            var request = (HttpWebRequest)WebRequest.Create(settings.GetPayPalUrl());
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var formContent = string.Format("cmd=_notify-synch&at={0}&tx={1}", settings.PdtToken, tx);
            request.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
            {
                sw.Write(formContent);
            }

            response = null;
            using (var sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                response = HttpUtility.UrlDecode(sr.ReadToEnd());
            }

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var firstLine = true;
            var success = false;

            foreach (string l in response.Split('\n'))
            {
                string line = l.Trim();
                if (firstLine)
                {
                    success = line.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);
                    firstLine = false;
                }
                else
                {
                    int equalPox = line.IndexOf('=');
                    if (equalPox >= 0)
                        values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
                }
            }

            return success;
        }

        /// <summary>
        /// Splits the difference of two value into a portion value (for each item) and a rest value
        /// </summary>
        /// <param name="difference">The difference value</param>
        /// <param name="numberOfLines">Number of lines\items to split the difference</param>
        /// <param name="portion">Portion value</param>
        /// <param name="rest">Rest value</param>
        private void SplitDifference(decimal difference, int numberOfLines, out decimal portion, out decimal rest)
        {
            portion = rest = decimal.Zero;

            if (numberOfLines == 0)
                numberOfLines = 1;

            int intDifference = (int)(difference * 100);
            int intPortion = (int)Math.Truncate((double)intDifference / (double)numberOfLines);
            int intRest = intDifference % numberOfLines;

            portion = Math.Round(((decimal)intPortion) / 100, 2);
            rest = Math.Round(((decimal)intRest) / 100, 2);

            Debug.Assert(difference == ((numberOfLines * portion) + rest));
        }

        /// <summary>
        /// Get all PayPal line items
        /// </summary>
        /// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
        /// <param name="checkoutAttributeValues">List with checkout attribute values</param>
        /// <param name="cartTotal">Receives the calculated cart total amount</param>
        /// <returns>All items for PayPal Standard API</returns>
        public List<PayPalLineItem> GetLineItems(PostProcessPaymentRequest postProcessPaymentRequest, out decimal cartTotal)
        {
            cartTotal = decimal.Zero;

            var order = postProcessPaymentRequest.Order;
            var lst = new List<PayPalLineItem>();

            // Order items... checkout attributes are included in order total
            foreach (var orderItem in order.OrderItems)
            {
                var item = new PayPalLineItem
                {
                    Type = PayPalItemType.CartItem,
                    Name = orderItem.Product.GetLocalized(x => x.Name),
                    Quantity = orderItem.Quantity,
                    Amount = orderItem.UnitPriceExclTax
                };
                lst.Add(item);

                cartTotal += orderItem.PriceExclTax;
            }

            // Shipping
            if (order.OrderShippingExclTax > decimal.Zero)
            {
                var item = new PayPalLineItem
                {
                    Type = PayPalItemType.Shipping,
                    Name = T("Plugins.Payments.PayPalStandard.ShippingFee").Text,
                    Quantity = 1,
                    Amount = order.OrderShippingExclTax
                };
                lst.Add(item);

                cartTotal += order.OrderShippingExclTax;
            }

            // Payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                var item = new PayPalLineItem
                {
                    Type = PayPalItemType.PaymentFee,
                    Name = T("Plugins.Payments.PayPal.PaymentMethodFee").Text,
                    Quantity = 1,
                    Amount = order.PaymentMethodAdditionalFeeExclTax
                };
                lst.Add(item);

                cartTotal += order.PaymentMethodAdditionalFeeExclTax;
            }

            // Tax
            if (order.OrderTax > decimal.Zero)
            {
                var item = new PayPalLineItem
                {
                    Type = PayPalItemType.Tax,
                    Name = T("Plugins.Payments.PayPalStandard.SalesTax").Text,
                    Quantity = 1,
                    Amount = order.OrderTax
                };
                lst.Add(item);

                cartTotal += order.OrderTax;
            }

            return lst;
        }

        /// <summary>
        /// Manually adjusts the net prices for cart items to avoid rounding differences with the PayPal API.
        /// </summary>
        /// <param name="paypalItems">PayPal line items</param>
        /// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
        /// <remarks>
        /// In detail: We add what we have thrown away in the checkout when we rounded prices to two decimal places.
        /// It's a workaround. Better solution would be to store the thrown away decimal places for each OrderItem in the database.
        /// More details: http://magento.xonu.de/magento-extensions/empfehlungen/magento-paypal-rounding-error-fix/
        /// </remarks>
        public void AdjustLineItemAmounts(List<PayPalLineItem> paypalItems, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                var cartItems = paypalItems.Where(x => x.Type == PayPalItemType.CartItem);

                if (cartItems.Count() <= 0)
                    return;

                decimal totalSmartStore = Math.Round(postProcessPaymentRequest.Order.OrderSubtotalExclTax, 2);
                decimal totalPayPal = decimal.Zero;
                decimal delta, portion, rest;

                // calculate what PayPal calculates
                cartItems.Each(x => totalPayPal += (x.AmountRounded * x.Quantity));
                totalPayPal = Math.Round(totalPayPal, 2, MidpointRounding.AwayFromZero);

                // calculate difference
                delta = Math.Round(totalSmartStore - totalPayPal, 2);
                //"SM: {0}, PP: {1}, delta: {2}".FormatInvariant(totalSmartStore, totalPayPal, delta).Dump();

                if (delta == decimal.Zero)
                    return;

                // prepare lines... only lines with quantity = 1 are adjustable. if there is no one, create one.
                if (!cartItems.Any(x => x.Quantity == 1))
                {
                    var item = cartItems.First(x => x.Quantity > 1);
                    item.Quantity -= 1;
                    var newItem = item.Clone();
                    newItem.Quantity = 1;
                    paypalItems.Insert(paypalItems.IndexOf(item) + 1, newItem);
                }

                var cartItemsOneQuantity = paypalItems.Where(x => x.Type == PayPalItemType.CartItem && x.Quantity == 1);
                Debug.Assert(cartItemsOneQuantity.Count() > 0);

                SplitDifference(delta, cartItemsOneQuantity.Count(), out portion, out rest);

                if (portion != decimal.Zero)
                {
                    cartItems
                        .Where(x => x.Quantity == 1)
                        .Each(x => x.Amount = x.Amount + portion);
                }

                if (rest != decimal.Zero)
                {
                    var restItem = cartItems.First(x => x.Quantity == 1);
                    restItem.Amount = restItem.Amount + rest;
                }

                //"SM: {0}, PP: {1}, delta: {2} (portion: {3}, rest: {4})".FormatInvariant(totalSmartStore, totalPayPal, delta, portion, rest).Dump();
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PayPalStandard";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PayPalStandard";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
        }
    }


    public class PayPalLineItem : ICloneable<PayPalLineItem>
    {
        public PayPalItemType Type { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }

        public decimal AmountRounded => Math.Round(Amount, 2);

        public PayPalLineItem Clone()
        {
            var item = new PayPalLineItem
            {
                Type = this.Type,
                Name = this.Name,
                Quantity = this.Quantity,
                Amount = this.Amount
            };
            return item;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }

    public enum PayPalItemType
    {
        CartItem = 0,
        Shipping,
        PaymentFee,
        Tax
    }
}
