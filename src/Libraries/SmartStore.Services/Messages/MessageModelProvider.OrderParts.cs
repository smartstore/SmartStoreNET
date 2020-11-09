using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;

namespace SmartStore.Services.Messages
{
    public partial class MessageModelProvider
    {
        protected virtual object CreateModelPart(Order part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var allow = new HashSet<string>
            {
                nameof(part.Id),
                nameof(part.OrderNumber),
                nameof(part.OrderGuid),
                nameof(part.StoreId),
                nameof(part.OrderStatus),
                nameof(part.PaymentStatus),
                nameof(part.ShippingStatus),
                nameof(part.CustomerTaxDisplayType),
                nameof(part.TaxRatesDictionary),
                nameof(part.VatNumber),
                nameof(part.AffiliateId),
                nameof(part.CustomerIp),
                nameof(part.CardType),
                nameof(part.CardName),
                nameof(part.MaskedCreditCardNumber),
                nameof(part.DirectDebitAccountHolder),
                nameof(part.DirectDebitBankCode), // TODO: (mc) Liquid > Bank data (?)
				nameof(part.PurchaseOrderNumber),
                nameof(part.ShippingMethod),
                nameof(part.PaymentMethodSystemName),
                nameof(part.ShippingRateComputationMethodSystemName)
				// TODO: (mc) Liquid > More whitelisting?
			};

            var m = new HybridExpando(part, allow, MemberOptMethod.Allow);
            var d = m as dynamic;

            d.ID = part.Id;
            d.Billing = CreateModelPart(part.BillingAddress, messageContext);
            if (part.ShippingAddress != null)
            {
                d.Shipping = part.ShippingAddress.IsPostalDataEqual(part.BillingAddress) == true ? null : CreateModelPart(part.ShippingAddress, messageContext);
            }
            d.CustomerEmail = part.BillingAddress.Email.NullEmpty();
            d.CustomerComment = part.CustomerOrderComment.NullEmpty();
            d.Disclaimer = GetTopic("Disclaimer", messageContext);
            d.ConditionsOfUse = GetTopic("ConditionsOfUse", messageContext);
            d.Status = part.OrderStatus.GetLocalizedEnum(_services.Localization, messageContext.Language.Id);
            d.CreatedOn = ToUserDate(part.CreatedOnUtc, messageContext);
            d.PaidOn = ToUserDate(part.PaidDateUtc, messageContext);

            // Payment method
            var paymentMethodName = part.PaymentMethodSystemName;
            var paymentMethod = _services.Resolve<IProviderManager>().GetProvider<IPaymentMethod>(part.PaymentMethodSystemName);
            if (paymentMethod != null)
            {
                paymentMethodName = GetLocalizedValue(messageContext, paymentMethod.Metadata, nameof(paymentMethod.Metadata.FriendlyName), x => x.FriendlyName);
            }
            d.PaymentMethod = paymentMethodName.NullEmpty();

            d.Url = part.Customer != null && !part.Customer.IsGuest()
                ? BuildActionUrl("Details", "Order", new { id = part.Id, area = "" }, messageContext)
                : null;

            // Overrides
            m.Properties["OrderNumber"] = part.GetOrderNumber().NullEmpty();
            m.Properties["AcceptThirdPartyEmailHandOver"] = GetBoolResource(part.AcceptThirdPartyEmailHandOver, messageContext);

            // Items, Totals & Co.
            d.Items = part.OrderItems.Where(x => x.Product != null).Select(x => CreateModelPart(x, messageContext)).ToList();
            d.Totals = CreateOrderTotalsPart(part, messageContext);

            // Checkout Attributes
            if (part.CheckoutAttributeDescription.HasValue())
            {
                d.CheckoutAttributes = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(part.CheckoutAttributeDescription)).NullEmpty();
            }

            PublishModelPartCreatedEvent<Order>(part, m);

            return m;
        }

        protected virtual object CreateOrderTotalsPart(Order order, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(order, nameof(order));

            var language = messageContext.Language;
            var currencyService = _services.Resolve<ICurrencyService>();
            var paymentService = _services.Resolve<IPaymentService>();
            var priceFormatter = _services.Resolve<IPriceFormatter>();
            var taxSettings = _services.Settings.LoadSetting<TaxSettings>(messageContext.Store.Id);

            var taxRates = new SortedDictionary<decimal, decimal>();
            Money cusTaxTotal = null;
            Money cusDiscount = null;
            Money cusRounding = null;
            Money cusTotal = null;

            var subTotals = GetSubTotals(order, messageContext);

            // Shipping
            bool dislayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

            // Payment method fee
            bool displayPaymentMethodFee = true;
            if (order.PaymentMethodAdditionalFeeExclTax == decimal.Zero)
            {
                displayPaymentMethodFee = false;
            }

            // Tax
            bool displayTax = true;
            bool displayTaxRates = true;
            if (taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = new SortedDictionary<decimal, decimal>();
                    foreach (var tr in order.TaxRatesDictionary)
                    {
                        taxRates.Add(tr.Key, currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));
                    }

                    displayTaxRates = taxSettings.DisplayTaxRates && taxRates.Count > 0;
                    displayTax = !displayTaxRates;

                    cusTaxTotal = FormatPrice(order.OrderTax, order, messageContext);
                }
            }

            // Discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                cusDiscount = FormatPrice(-order.OrderDiscount, order, messageContext);
                dislayDiscount = true;
            }

            // Total
            var roundingAmount = decimal.Zero;
            var orderTotal = order.GetOrderTotalInCustomerCurrency(currencyService, paymentService, out roundingAmount);
            cusTotal = FormatPrice(orderTotal, order.CustomerCurrencyCode, messageContext);

            // Rounding
            if (roundingAmount != decimal.Zero)
            {
                cusRounding = FormatPrice(roundingAmount, order.CustomerCurrencyCode, messageContext);
            }

            // Model
            dynamic m = new ExpandoObject();

            m.SubTotal = subTotals.SubTotal;
            m.SubTotalDiscount = subTotals.DisplaySubTotalDiscount ? subTotals.SubTotalDiscount : null;
            m.Shipping = dislayShipping ? subTotals.ShippingTotal : null;
            m.Payment = displayPaymentMethodFee ? subTotals.PaymentFee : null;
            m.Tax = displayTax ? cusTaxTotal : null;
            m.Discount = dislayDiscount ? cusDiscount : null;
            m.RoundingDiff = cusRounding;
            m.Total = cusTotal;
            m.IsGross = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

            // TaxRates
            m.TaxRates = !displayTaxRates ? (object[])null : taxRates.Select(x =>
            {
                return new
                {
                    Rate = T("Order.TaxRateLine", language.Id, priceFormatter.FormatTaxRate(x.Key)).Text,
                    Value = FormatPrice(x.Value, order, messageContext)
                };
            }).ToArray();


            // Gift Cards
            m.GiftCardUsage = order.GiftCardUsageHistory.Count == 0 ? (object[])null : order.GiftCardUsageHistory.Select(x =>
            {
                return new
                {
                    GiftCard = T("Order.GiftCardInfo", language.Id, x.GiftCard.GiftCardCouponCode).Text,
                    UsedAmount = FormatPrice(-x.UsedValue, order, messageContext),
                    RemainingAmount = FormatPrice(x.GiftCard.GetGiftCardRemainingAmount(), order, messageContext)
                };
            }).ToArray();

            // Reward Points
            m.RedeemedRewardPoints = order.RedeemedRewardPointsEntry == null ? null : new
            {
                Title = T("Order.RewardPoints", language.Id, -order.RedeemedRewardPointsEntry.Points).Text,
                Amount = FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, order, messageContext)
            };

            return m;
        }

        private (Money SubTotal, Money SubTotalDiscount, Money ShippingTotal, Money PaymentFee, bool DisplaySubTotalDiscount) GetSubTotals(Order order, MessageContext messageContext)
        {
            var isNet = order.CustomerTaxDisplayType == TaxDisplayType.ExcludingTax;

            var subTotal = isNet ? order.OrderSubtotalExclTax : order.OrderSubtotalInclTax;
            var subTotalDiscount = isNet ? order.OrderSubTotalDiscountExclTax : order.OrderSubTotalDiscountInclTax;
            var shipping = isNet ? order.OrderShippingExclTax : order.OrderShippingInclTax;
            var payment = isNet ? order.PaymentMethodAdditionalFeeExclTax : order.PaymentMethodAdditionalFeeInclTax;

            // Subtotal
            var cusSubTotal = FormatPrice(subTotal, order, messageContext);

            // Shipping
            var cusShipTotal = FormatPrice(shipping, order, messageContext);

            // Payment method additional fee
            var cusPaymentMethodFee = FormatPrice(payment, order, messageContext);

            // Discount (applied to order subtotal)
            Money cusSubTotalDiscount = null;
            bool dislaySubTotalDiscount = false;
            if (subTotalDiscount > decimal.Zero)
            {
                cusSubTotalDiscount = FormatPrice(-subTotalDiscount, order, messageContext);
                dislaySubTotalDiscount = true;
            }

            return (cusSubTotal, cusSubTotalDiscount, cusShipTotal, cusPaymentMethodFee, dislaySubTotalDiscount);
        }

        protected virtual object CreateModelPart(OrderItem part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var productAttributeParser = _services.Resolve<IProductAttributeParser>();
            var downloadService = _services.Resolve<IDownloadService>();
            var deliveryTimeService = _services.Resolve<IDeliveryTimeService>();
            var order = part.Order;
            var isNet = order.CustomerTaxDisplayType == TaxDisplayType.ExcludingTax;
            var product = part.Product;
            product.MergeWithCombination(part.AttributesXml, productAttributeParser);

            // Bundle items.
            object bundleItems = null;
            if (product.ProductType == ProductType.BundledProduct && part.BundleData.HasValue())
            {
                var bundleData = part.GetBundleData();
                if (bundleData.Any())
                {
                    var productService = _services.Resolve<IProductService>();
                    var products = productService.GetProductsByIds(bundleData.Select(x => x.ProductId).ToArray());
                    var productsDic = products.ToDictionarySafe(x => x.Id, x => x);

                    bundleItems = bundleData
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x =>
                        {
                            productsDic.TryGetValue(x.ProductId, out Product bundleItemProduct);
                            return CreateModelPart(x, part, bundleItemProduct, messageContext);
                        })
                        .ToList();
                }
            }

            var m = new Dictionary<string, object>
            {
                { "DownloadUrl", !downloadService.IsDownloadAllowed(part) ? null : BuildActionUrl("GetDownload", "Download", new { id = part.OrderItemGuid, area = "" }, messageContext) },
                { "AttributeDescription", part.AttributeDescription.NullEmpty() },
                { "Weight", part.ItemWeight },
                { "TaxRate", part.TaxRate },
                { "Qty", part.Quantity },
                { "UnitPrice", FormatPrice(isNet ? part.UnitPriceExclTax : part.UnitPriceInclTax, part.Order, messageContext) },
                { "LineTotal", FormatPrice(isNet ? part.PriceExclTax : part.PriceInclTax, part.Order, messageContext) },
                { "Product", CreateModelPart(product, messageContext, part.AttributesXml) },
                { "BundleItems", bundleItems },
                { "IsGross", !isNet },
                { "DisplayDeliveryTime", part.DisplayDeliveryTime },
            };

            if (part.DeliveryTimeId.HasValue)
            {
                if (deliveryTimeService.GetDeliveryTimeById(part.DeliveryTimeId ?? 0) is DeliveryTime dt)
                {
                    m["DeliveryTime"] = new Dictionary<string, object>
                    {
                        { "Color", dt.ColorHexValue },
                        { "Name", dt.GetLocalized(x => x.Name, messageContext.Language).Value },
                    };
                }
            }

            PublishModelPartCreatedEvent<OrderItem>(part, m);

            return m;
        }

        protected virtual object CreateModelPart(ProductBundleItemOrderData part, OrderItem orderItem, Product product, MessageContext messageContext)
        {
            Guard.NotNull(part, nameof(part));
            Guard.NotNull(orderItem, nameof(orderItem));
            Guard.NotNull(product, nameof(product));
            Guard.NotNull(messageContext, nameof(messageContext));

            var priceWithDiscount = FormatPrice(part.PriceWithDiscount, orderItem.Order, messageContext);

            var m = new Dictionary<string, object>
            {
                { "AttributeDescription", part.AttributesInfo.NullEmpty() },
                { "Quantity", part.Quantity > 1 && part.PerItemShoppingCart ? part.Quantity.ToString() : null },
                { "PerItemShoppingCart", part.PerItemShoppingCart },
                { "PriceWithDiscount", priceWithDiscount },
                { "Product", CreateModelPart(product, messageContext, part.AttributesXml) }
            };

            PublishModelPartCreatedEvent(part, m);

            return m;
        }

        protected virtual object CreateModelPart(OrderNote part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var m = new Dictionary<string, object>
            {
                { "Id", part.Id },
                { "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) },
                { "Text", part.FormatOrderNoteText().NullEmpty() }
            };

            PublishModelPartCreatedEvent<OrderNote>(part, m);

            return m;
        }

        protected virtual object CreateModelPart(ShoppingCartItem part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var m = new Dictionary<string, object>
            {
                { "Id", part.Id },
                { "Quantity", part.Quantity },
                { "Product", CreateModelPart(part.Product, messageContext, part.AttributesXml) },
            };

            PublishModelPartCreatedEvent<ShoppingCartItem>(part, m);

            return m;
        }

        protected virtual object CreateModelPart(Shipment part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var itemParts = new List<object>();
            var orderService = _services.Resolve<IOrderService>();
            var orderItems = orderService.GetOrderItemsByOrderIds(new int[] { part.OrderId })[part.OrderId];
            var orderItemsDic = orderItems.ToDictionarySafe(x => x.Id);

            foreach (var shipmentItem in part.ShipmentItems)
            {
                if (orderItemsDic.TryGetValue(shipmentItem.OrderItemId, out var orderItem) && orderItem.Product != null)
                {
                    var itemPart = CreateModelPart(orderItem, messageContext) as Dictionary<string, object>;
                    itemPart["Qty"] = shipmentItem.Quantity;

                    itemParts.Add(itemPart);
                }
            }

            var trackingUrl = part.TrackingUrl;

            if (trackingUrl.IsEmpty() && part.TrackingNumber.HasValue() && part.Order.ShippingRateComputationMethodSystemName.HasValue())
            {
                // Try to get URL from tracker.
                var srcm = _services.Resolve<IShippingService>().LoadShippingRateComputationMethodBySystemName(part.Order.ShippingRateComputationMethodSystemName);
                if (srcm != null && srcm.Value.IsActive)
                {
                    var tracker = srcm.Value.ShipmentTracker;
                    if (tracker != null)
                    {
                        var shippingSettings = _services.Settings.LoadSetting<ShippingSettings>(part.Order.StoreId);
                        if (srcm.IsShippingRateComputationMethodActive(shippingSettings))
                        {
                            trackingUrl = tracker.GetUrl(part.TrackingNumber);
                        }
                    }
                }
            }

            var m = new Dictionary<string, object>
            {
                { "Id", part.Id },
                { "TrackingNumber", part.TrackingNumber.NullEmpty() },
                { "TrackingUrl", trackingUrl.NullEmpty() },
                { "TotalWeight", part.TotalWeight },
                { "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) },
                { "DeliveredOn", ToUserDate(part.DeliveryDateUtc, messageContext) },
                { "ShippedOn", ToUserDate(part.ShippedDateUtc, messageContext) },
                { "Url", BuildActionUrl("ShipmentDetails", "Order", new { id = part.Id, area = "" }, messageContext)},
                { "Items", itemParts },
            };

            PublishModelPartCreatedEvent(part, m);

            return m;
        }

        protected virtual object CreateModelPart(RecurringPayment part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var m = new Dictionary<string, object>
            {
                { "Id", part.Id },
                { "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) },
                { "StartedOn", ToUserDate(part.StartDateUtc, messageContext) },
                { "NextOn", ToUserDate(part.NextPaymentDate, messageContext) },
                { "CycleLength", part.CycleLength },
                { "CyclePeriod", part.CyclePeriod.GetLocalizedEnum(_services.Localization, messageContext.Language.Id) },
                { "CyclesRemaining", part.CyclesRemaining },
                { "TotalCycles", part.TotalCycles },
                { "Url", BuildActionUrl("Edit", "RecurringPayment", new { id = part.Id, area = "admin" }, messageContext) }
            };

            PublishModelPartCreatedEvent<RecurringPayment>(part, m);

            return m;
        }

        protected virtual object CreateModelPart(ReturnRequest part, MessageContext messageContext)
        {
            Guard.NotNull(messageContext, nameof(messageContext));
            Guard.NotNull(part, nameof(part));

            var m = new Dictionary<string, object>
            {
                { "Id", part.Id },
                { "Reason", part.ReasonForReturn.NullEmpty() },
                { "Status", part.ReturnRequestStatus.GetLocalizedEnum(_services.Localization, messageContext.Language.Id) },
                { "RequestedAction", part.RequestedAction.NullEmpty() },
                { "CustomerComments", HtmlUtils.StripTags(part.CustomerComments).NullEmpty() },
                { "StaffNotes", HtmlUtils.StripTags(part.StaffNotes).NullEmpty() },
                { "Quantity", part.Quantity },
                { "RefundToWallet", part.RefundToWallet },
                { "Url", BuildActionUrl("Edit", "ReturnRequest", new { id = part.Id, area = "admin" }, messageContext) }
            };

            PublishModelPartCreatedEvent<ReturnRequest>(part, m);

            return m;
        }
    }
}
