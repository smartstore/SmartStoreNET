using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Order;

namespace SmartStore.Web.Controllers
{
    public partial class OrderHelper
    {
        private readonly ICommonServices _services;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly PluginMediator _pluginMediator;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ProductUrlHelper _productUrlHelper;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrencyService _currencyService;
        private readonly ICountryService _countryService;
        private readonly IQuantityUnitService _quantityUnitService;
        private readonly IMediaService _mediaService;
        private readonly IProductService _productService;
        private readonly IEncryptionService _encryptionService;

        public OrderHelper(
            ICommonServices services,
            IDateTimeHelper dateTimeHelper,
            PluginMediator pluginMediator,
            IPriceFormatter priceFormatter,
            ProductUrlHelper productUrlHelper,
            IProductAttributeParser productAttributeParser,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            ICurrencyService currencyService,
            ICountryService countryService,
            IQuantityUnitService quantityUnitService,
            IMediaService mediaService,
            IProductService productService,
            IEncryptionService encryptionService)
        {
            _services = services;
            _dateTimeHelper = dateTimeHelper;
            _pluginMediator = pluginMediator;
            _priceFormatter = priceFormatter;
            _productUrlHelper = productUrlHelper;
            _productAttributeParser = productAttributeParser;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _countryService = countryService;
            _quantityUnitService = quantityUnitService;
            _mediaService = mediaService;
            _productService = productService;
            _encryptionService = encryptionService;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        public static string OrderDetailsPrintViewPath => "~/Views/Order/Details.Print.cshtml";

        private PictureModel PrepareOrderItemPictureModel(
            Product product,
            int pictureSize,
            string productName,
            string attributesXml,
            CatalogSettings catalogSettings)
        {
            Guard.NotNull(product, nameof(product));

            MediaFileInfo file = null;
            var combination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributesXml);

            if (combination != null)
            {
                var mediaIds = combination.GetAssignedMediaIds();
                if (mediaIds != null && mediaIds.Length > 0)
                {
                    file = _mediaService.GetFileById(mediaIds[0], MediaLoadFlags.AsNoTracking);
                }
            }

            // No attribute combination image, then load product picture.
            if (file == null)
            {
                var mediaFile = _productService.GetProductPicturesByProductId(product.Id, 1)
                    .Select(x => x.MediaFile)
                    .FirstOrDefault();

                if (mediaFile != null)
                {
                    file = _mediaService.ConvertMediaFile(mediaFile);
                }
            }

            if (file == null && product.Visibility == ProductVisibility.Hidden && product.ParentGroupedProductId > 0)
            {
                // Let's check whether this product has some parent "grouped" product.
                var mediaFile = _productService.GetProductPicturesByProductId(product.ParentGroupedProductId, 1)
                    .Select(x => x.MediaFile)
                    .FirstOrDefault();

                if (mediaFile != null)
                {
                    file = _mediaService.ConvertMediaFile(mediaFile);
                }
            }

            return new PictureModel
            {
                PictureId = file?.Id ?? 0,
                Size = pictureSize,
                ImageUrl = _mediaService.GetUrl(file, pictureSize, null, !catalogSettings.HideProductDefaultPictures),
                Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? T("Media.Product.ImageLinkTitleFormat", productName),
                AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? T("Media.Product.ImageAlternateTextFormat", productName),
                File = file
            };
        }

        private OrderDetailsModel.OrderItemModel PrepareOrderItemModel(
            Order order,
            OrderItem orderItem,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            MediaSettings mediaSettings)
        {
            var language = _services.WorkContext.WorkingLanguage;

            orderItem.Product.MergeWithCombination(orderItem.AttributesXml);

            var model = new OrderDetailsModel.OrderItemModel
            {
                Id = orderItem.Id,
                Sku = orderItem.Product.Sku,
                ProductId = orderItem.Product.Id,
                ProductName = orderItem.Product.GetLocalized(x => x.Name),
                ProductSeName = orderItem.Product.GetSeName(),
                ProductType = orderItem.Product.ProductType,
                Quantity = orderItem.Quantity,
                AttributeInfo = orderItem.AttributeDescription
            };

            var quantityUnit = _quantityUnitService.GetQuantityUnitById(orderItem.Product.QuantityUnitId);
            model.QuantityUnit = quantityUnit == null ? "" : quantityUnit.GetLocalized(x => x.Name);

            if (orderItem.Product.ProductType == ProductType.BundledProduct && orderItem.BundleData.HasValue())
            {
                var bundleData = orderItem.GetBundleData();
                var bundleItems = shoppingCartSettings.ShowProductBundleImagesOnShoppingCart
                    ? _productService.GetBundleItems(orderItem.ProductId).ToDictionarySafe(x => x.Item.ProductId)
                    : new Dictionary<int, ProductBundleItemData>();

                model.BundlePerItemPricing = orderItem.Product.BundlePerItemPricing;
                model.BundlePerItemShoppingCart = bundleData.Any(x => x.PerItemShoppingCart);

                foreach (var bid in bundleData)
                {
                    var bundleItemModel = new OrderDetailsModel.BundleItemModel
                    {
                        Sku = bid.Sku,
                        ProductName = bid.ProductName,
                        ProductSeName = bid.ProductSeName,
                        VisibleIndividually = bid.VisibleIndividually,
                        Quantity = bid.Quantity,
                        DisplayOrder = bid.DisplayOrder,
                        AttributeInfo = bid.AttributesInfo
                    };

                    bundleItemModel.ProductUrl = _productUrlHelper.GetProductUrl(bid.ProductId, bundleItemModel.ProductSeName, bid.AttributesXml);

                    if (model.BundlePerItemShoppingCart)
                    {
                        var priceWithDiscount = _currencyService.ConvertCurrency(bid.PriceWithDiscount, order.CurrencyRate);
                        bundleItemModel.PriceWithDiscount = _priceFormatter.FormatPrice(priceWithDiscount, true, order.CustomerCurrencyCode, language, false, false);
                    }

                    // Bundle item picture.
                    if (shoppingCartSettings.ShowProductBundleImagesOnShoppingCart && bundleItems.TryGetValue(bid.ProductId, out var bundleItem))
                    {
                        bundleItemModel.HideThumbnail = bundleItem.Item.HideThumbnail;

                        bundleItemModel.Picture = PrepareOrderItemPictureModel(
                            bundleItem.Item.Product,
                            mediaSettings.CartThumbBundleItemPictureSize,
                            bid.ProductName,
                            bid.AttributesXml,
                            catalogSettings);
                    }

                    model.BundleItems.Add(bundleItemModel);
                }
            }

            // Unit price, subtotal.
            switch (order.CustomerTaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    {
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        model.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false, false);

                        var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                        model.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false, false);
                    }
                    break;

                case TaxDisplayType.IncludingTax:
                    {
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        model.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true, false);

                        var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                        model.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true, false);
                    }
                    break;
            }

            model.ProductUrl = _productUrlHelper.GetProductUrl(model.ProductSeName, orderItem);

            if (shoppingCartSettings.ShowProductImagesOnShoppingCart)
            {
                model.Picture = PrepareOrderItemPictureModel(
                    orderItem.Product,
                    mediaSettings.CartThumbPictureSize,
                    model.ProductName,
                    orderItem.AttributesXml,
                    catalogSettings);
            }

            return model;
        }

        public OrderDetailsModel PrepareOrderDetailsModel(Order order)
        {
            Guard.NotNull(order, nameof(order));

            var store = _services.StoreService.GetStoreById(order.StoreId) ?? _services.StoreContext.CurrentStore;
            var language = _services.WorkContext.WorkingLanguage;

            var orderSettings = _services.Settings.LoadSetting<OrderSettings>(store.Id);
            var catalogSettings = _services.Settings.LoadSetting<CatalogSettings>(store.Id);
            var taxSettings = _services.Settings.LoadSetting<TaxSettings>(store.Id);
            var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(store.Id);
            var addressSettings = _services.Settings.LoadSetting<AddressSettings>(store.Id);
            var companyInfoSettings = _services.Settings.LoadSetting<CompanyInformationSettings>(store.Id);
            var shoppingCartSettings = _services.Settings.LoadSetting<ShoppingCartSettings>(store.Id);
            var mediaSettings = _services.Settings.LoadSetting<MediaSettings>(store.Id);

            var model = new OrderDetailsModel();

            // TODO: refactor modelling for multi-order processing.
            model.MerchantCompanyInfo = companyInfoSettings;
            model.MerchantCompanyCountryName = _countryService.GetCountryById(companyInfoSettings.CountryId)?.GetLocalized(x => x.Name);

            model.Id = order.Id;
            model.StoreId = order.StoreId;
            model.CustomerLanguageId = order.CustomerLanguageId;
            model.CustomerComment = order.CustomerOrderComment;

            model.OrderNumber = order.GetOrderNumber();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_services.Localization, _services.WorkContext);
            model.IsReOrderAllowed = orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order);
            model.DisplayPdfInvoice = pdfSettings.Enabled;
            model.RenderOrderNotes = pdfSettings.RenderOrderNotes;

            // Shipping info.
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_services.Localization, _services.WorkContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.ShippingAddress.PrepareModel(order.ShippingAddress, false, addressSettings);
                model.ShippingMethod = order.ShippingMethod;


                // Shipments (only already shipped).
                var shipments = order.Shipments.Where(x => x.ShippedDateUtc.HasValue).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };

                    if (shipment.ShippedDateUtc.HasValue)
                    {
                        shipmentModel.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    }
                    if (shipment.DeliveryDateUtc.HasValue)
                    {
                        shipmentModel.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    }

                    model.Shipments.Add(shipmentModel);
                }
            }

            model.BillingAddress.PrepareModel(order.BillingAddress, false, addressSettings);
            model.VatNumber = order.VatNumber;

            // Payment method.
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethodSystemName = order.PaymentMethodSystemName;
            model.PaymentMethod = paymentMethod != null ? _pluginMediator.GetLocalizedFriendlyName(paymentMethod.Metadata) : order.PaymentMethodSystemName;
            model.CanRePostProcessPayment = _paymentService.CanRePostProcessPayment(order);

            // Purchase order number (we have to find a better to inject this information because it's related to a certain plugin).
            if (paymentMethod != null && paymentMethod.Metadata.SystemName.Equals("SmartStore.PurchaseOrderNumber", StringComparison.InvariantCultureIgnoreCase))
            {
                model.DisplayPurchaseOrderNumber = true;
                model.PurchaseOrderNumber = order.PurchaseOrderNumber;
            }

            if (order.AllowStoringCreditCardNumber)
            {
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                model.MaskedCreditCardNumber = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                model.CardExpirationMonth = _encryptionService.DecryptText(order.CardExpirationMonth);
                model.CardExpirationYear = _encryptionService.DecryptText(order.CardExpirationYear);
            }

            if (order.AllowStoringDirectDebit)
            {
                model.DirectDebitAccountHolder = _encryptionService.DecryptText(order.DirectDebitAccountHolder);
                model.DirectDebitAccountNumber = _encryptionService.DecryptText(order.DirectDebitAccountNumber);
                model.DirectDebitBankCode = _encryptionService.DecryptText(order.DirectDebitBankCode);
                model.DirectDebitBankName = _encryptionService.DecryptText(order.DirectDebitBankName);
                model.DirectDebitBIC = _encryptionService.DecryptText(order.DirectDebitBIC);
                model.DirectDebitCountry = _encryptionService.DecryptText(order.DirectDebitCountry);
                model.DirectDebitIban = _encryptionService.DecryptText(order.DirectDebitIban);
            }

            // Totals.
            switch (order.CustomerTaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    {
                        // Order subtotal.
                        var orderSubtotalExclTax = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalExclTax, true, order.CustomerCurrencyCode, language, false, false);

                        // Discount (applied to order subtotal).
                        var orderSubTotalDiscountExclTax = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountExclTax > decimal.Zero)
                        {
                            model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTax, true, order.CustomerCurrencyCode, language, false, false);
                        }

                        // Order shipping.
                        var orderShippingExclTax = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingExclTax, true, order.CustomerCurrencyCode, language, false, false);

                        // Payment method additional fee.
                        var paymentMethodAdditionalFeeExclTax = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                        if (paymentMethodAdditionalFeeExclTax != decimal.Zero)
                        {
                            model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTax, true, order.CustomerCurrencyCode,
                                language, false, false);
                        }
                    }
                    break;

                case TaxDisplayType.IncludingTax:
                    {
                        // Order subtotal.
                        var orderSubtotalInclTax = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalInclTax, true, order.CustomerCurrencyCode, language, true, false);

                        // Discount (applied to order subtotal).
                        var orderSubTotalDiscountInclTax = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountInclTax > decimal.Zero)
                        {
                            model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTax, true, order.CustomerCurrencyCode, language, true, false);
                        }

                        // Order shipping.
                        var orderShippingInclTax = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingInclTax, true, order.CustomerCurrencyCode, language, true, false);

                        // Payment method additional fee.
                        var paymentMethodAdditionalFeeInclTax = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                        if (paymentMethodAdditionalFeeInclTax != decimal.Zero)
                        {
                            model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTax, true, order.CustomerCurrencyCode,
                                language, true, false);
                        }
                    }
                    break;
            }

            // Tax.
            var displayTax = true;
            var displayTaxRates = true;

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
                    displayTaxRates = taxSettings.DisplayTaxRates && order.TaxRatesDictionary.Count > 0;
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);

                    model.Tax = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
                    foreach (var tr in order.TaxRatesDictionary)
                    {
                        var rate = _priceFormatter.FormatTaxRate(tr.Key);
                        //var labelKey = "ShoppingCart.Totals.TaxRateLine" + (_services.WorkContext.TaxDisplayType == TaxDisplayType.IncludingTax ? "Incl" : "Excl");
                        var labelKey = _services.WorkContext.TaxDisplayType == TaxDisplayType.IncludingTax ? "ShoppingCart.Totals.TaxRateLineIncl" : "ShoppingCart.Totals.TaxRateLineExcl";

                        model.TaxRates.Add(new OrderDetailsModel.TaxRate
                        {
                            Rate = rate,
                            Label = T(labelKey).Text.FormatCurrent(rate),
                            Value = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, language),
                        });
                    }
                }
            }

            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;


            // Discount (applied to order total).
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
            {
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
            }

            // Gift cards.
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                var remainingAmountBase = gcuh.GiftCard.GetGiftCardRemainingAmount();
                var remainingAmount = _currencyService.ConvertCurrency(remainingAmountBase, order.CurrencyRate);

                var gcModel = new OrderDetailsModel.GiftCard
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language),
                    Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false)
                };

                model.GiftCards.Add(gcModel);
            }

            // Reward points         .  
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)),
                    true, order.CustomerCurrencyCode, false, language);
            }

            // Credit balance.
            if (order.CreditBalance > decimal.Zero)
            {
                var convertedCreditBalance = _currencyService.ConvertCurrency(order.CreditBalance, order.CurrencyRate);
                model.CreditBalance = _priceFormatter.FormatPrice(-convertedCreditBalance, true, order.CustomerCurrencyCode, false, language);
            }

            // Total.
            var roundingAmount = decimal.Zero;
            var orderTotal = order.GetOrderTotalInCustomerCurrency(_currencyService, _paymentService, out roundingAmount);

            model.OrderTotal = _priceFormatter.FormatPrice(orderTotal, true, order.CustomerCurrencyCode, false, language);

            if (roundingAmount != decimal.Zero)
            {
                model.OrderTotalRounding = _priceFormatter.FormatPrice(roundingAmount, true, order.CustomerCurrencyCode, false, language);
            }

            // Checkout attributes.
            model.CheckoutAttributeInfo = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(order.CheckoutAttributeDescription));

            // Order notes.
            foreach (var orderNote in order.OrderNotes
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                var createdOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc);

                model.OrderNotes.Add(new OrderDetailsModel.OrderNote
                {
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = createdOn,
                    FriendlyCreatedOn = createdOn.RelativeFormat(false, "f")
                });
            }

            // Purchased products.
            model.ShowSku = catalogSettings.ShowProductSku;
            model.ShowProductImages = shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowProductBundleImages = shoppingCartSettings.ShowProductBundleImagesOnShoppingCart;
            model.BundleThumbSize = mediaSettings.CartThumbBundleItemPictureSize;

            var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);

            foreach (var orderItem in orderItems)
            {
                var orderItemModel = PrepareOrderItemModel(
                    order,
                    orderItem,
                    catalogSettings,
                    shoppingCartSettings,
                    mediaSettings);

                model.Items.Add(orderItemModel);
            }

            return model;
        }
    }
}