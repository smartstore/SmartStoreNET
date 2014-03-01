using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using System.Globalization;
using SmartStore.Services.Stores;
using System.Text;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWebHelper _webHelper;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly AddressSettings _addressSettings;

        //codehint: sm-add
        private readonly CompanyInformationSettings _companyInformationSettings;
        private readonly BankConnectionSettings _bankConnectionSettings;
        private readonly ContactDataSettings _contactDataSettings;

        #endregion

        #region Ctor

        public PdfService(ILocalizationService localizationService, IOrderService orderService,
            IPaymentService paymentService,
            IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter,
            ICurrencyService currencyService, IMeasureService measureService,
            IPictureService pictureService, IProductService productService,
			IProductAttributeParser productAttributeParser, IStoreService storeService,
			IStoreContext storeContext, IWebHelper webHelper,
            CatalogSettings catalogSettings, CurrencySettings currencySettings,
            MeasureSettings measureSettings, PdfSettings pdfSettings, TaxSettings taxSettings,
            StoreInformationSettings storeInformationSettings, AddressSettings addressSettings,
            CompanyInformationSettings companyInformationSettings, BankConnectionSettings bankConnectionSettings,
			ContactDataSettings contactDataSettings)
        {
            this._localizationService = localizationService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._measureService = measureService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
			this._storeService = storeService;
			this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._currencySettings = currencySettings;
            this._catalogSettings = catalogSettings;
            this._measureSettings = measureSettings;
            this._pdfSettings = pdfSettings;
            this._taxSettings = taxSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._addressSettings = addressSettings;

            //codehint: sm-add
            this._companyInformationSettings = companyInformationSettings;
            this._bankConnectionSettings = bankConnectionSettings;
            this._contactDataSettings = contactDataSettings;
        }

        #endregion

        #region Utilities

        protected virtual Font GetFont()
        {
            //SmartStore.NET supports unicode characters
            //SmartStore.NET uses Free Serif font by default (~/App_Data/Pdf/OpenSans-Regular.ttf file)
            string fontPath = Path.Combine(_webHelper.MapPath("~/App_Data/Pdf/"), _pdfSettings.FontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="lang">Language</param>
        public virtual void PrintOrdersToPdf(Stream stream, IList<Order> orders, Language lang)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (orders == null)
                throw new ArgumentNullException("orders");

            if (lang == null)
                throw new ArgumentNullException("lang");


            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }


            var doc = new Document(pageSize, 40, 40, 40, 80);
            var writer = PdfWriter.GetInstance(doc, stream);
            writer.PageEvent = new OrderPdfPageEvents(_pictureService, _pdfSettings, _companyInformationSettings, _bankConnectionSettings, _contactDataSettings,
				_localizationService, lang, _storeContext);
            doc.Open();
            
            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            int ordCount = orders.Count;
            int ordNum = 0;

            foreach (var order in orders)
            {
                #region Header

                //logo
                var logoPicture = _pictureService.GetPictureById(_pdfSettings.LogoPictureId);
                var logoExists = logoPicture != null;

                ////header
                var headerTable = new PdfPTable(logoExists ? 2 : 1);
                headerTable.WidthPercentage = 100f;
                if (logoExists)
                    headerTable.SetWidths(new[] { 50, 50 });

                //store info
				var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

                var cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                var pOrderId = new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.Order#", lang.Id), order.GetOrderNumber()), titleFont);
                pOrderId.Alignment = Element.ALIGN_LEFT;
                cell.AddElement(pOrderId);
                //var anchor = new Anchor(_storeInformationSettings.StoreUrl.Trim(new char[] { '/' }), font);
                //anchor.Reference = _storeInformationSettings.StoreUrl;
                //var pAnchor = new Paragraph(anchor);
                //pAnchor.Alignment = Element.ALIGN_LEFT;
                //cell.AddElement(pAnchor);
                var pDate = new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.OrderDate", lang.Id), _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(lang.LanguageCulture))), font);
                pDate.Alignment = Element.ALIGN_LEFT;
                cell.AddElement(pDate);
                headerTable.AddCell(cell);

                //logo
                if (logoExists)
                {
                    var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                    if (logoFilePath.HasValue())
                    {
                        var img = Image.GetInstance(logoFilePath);
                        img.ScaleToFit(250, 40);
                        var cellLogo = new PdfPCell(img);
                        cellLogo.Border = Rectangle.NO_BORDER;
                        cellLogo.HorizontalAlignment = Element.ALIGN_RIGHT;
                        headerTable.AddCell(cellLogo);
                    }
                }
                doc.Add(headerTable);

                PdfContentByte cb = writer.DirectContent;
                cb.MoveTo(pageSize.Left + 40f, pageSize.Top - 80f);
                cb.LineTo(pageSize.Right - 40f, pageSize.Top - 80f);
                cb.Stroke();

                #endregion

                #region Addresses

                var fontSmall = new Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.NORMAL);

                var addressTable = new PdfPTable(2);
                addressTable.WidthPercentage = 100f;
                addressTable.SetWidths(new[] { 50, 50 });

                //billing address
                string company = String.Format("{0}, {1}, {2} {3}", _companyInformationSettings.CompanyName, 
                    _companyInformationSettings.Street, 
                    _companyInformationSettings.ZipCode,
                    _companyInformationSettings.City);

                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(" "));
                cell.AddElement(new Paragraph(" "));

                Chunk chkHeader = new Chunk(company, fontSmall);
                chkHeader.SetUnderline(.5f, -2f);
                cell.AddElement(new Paragraph(chkHeader));

                if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(order.BillingAddress.Company))
                {
                    cell.AddElement(new Paragraph(order.BillingAddress.Company, font));
                }
                cell.AddElement(new Paragraph(String.Format(order.BillingAddress.FirstName + " " + order.BillingAddress.LastName), font));
                if (_addressSettings.StreetAddressEnabled)
                {
                    cell.AddElement(new Paragraph(order.BillingAddress.Address1, font));
                }
                if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.BillingAddress.Address2))
                {
                    cell.AddElement(new Paragraph(order.BillingAddress.Address2, font));
                }
                if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled || _addressSettings.CountryEnabled && order.BillingAddress.Country != null)
                {
                    cell.AddElement(new Paragraph(String.Format("{0} {1} - {2} {3}", 
                        order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.TwoLetterIsoCode, lang.Id) : "",
                        order.BillingAddress.StateProvince != null ? "(" + order.BillingAddress.StateProvince.GetLocalized(x => x.Name, lang.Id) + ")": "", 
                        order.BillingAddress.ZipPostalCode,
                        order.BillingAddress.City ), font));
                }
                
                addressTable.AddCell(cell);

                //legal + shop infos 
                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                var paragraph = new Paragraph(_companyInformationSettings.CompanyName, font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                paragraph = new Paragraph(_companyInformationSettings.Street, font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                paragraph = new Paragraph(_companyInformationSettings.ZipCode + " " + _companyInformationSettings.City, font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);
                cell.AddElement(new Paragraph(" ", fontSmall));
                
                //email
                paragraph = new Paragraph(_contactDataSettings.CompanyEmailAddress, font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //url
				paragraph = new Paragraph(store.Url, font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //phone
                paragraph = new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.Phone", lang.Id), _contactDataSettings.CompanyTelephoneNumber), font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //fax
                paragraph = new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.Fax", lang.Id), _contactDataSettings.CompanyFaxNumber), font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //tax number/ust-id
                paragraph = new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.TaxNumber", lang.Id), _companyInformationSettings.TaxNumber), font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //vat id
                paragraph = new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.VatId", lang.Id), _companyInformationSettings.VatId), font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //commercial register heading
                paragraph = new Paragraph(_localizationService.GetResource("PDFInvoice.CommercialRegisterHeading", lang.Id), font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                //handelregister
                paragraph = new Paragraph(_companyInformationSettings.CommercialRegister, font);
                paragraph.Alignment = Element.ALIGN_RIGHT;
                cell.AddElement(paragraph);

                addressTable.AddCell(cell);

                doc.Add(addressTable);
                doc.Add(new Paragraph(" "));

                #endregion

                #region Products
                //products
                doc.Add(new Paragraph(_localizationService.GetResource("PDFInvoice.Product(s)", lang.Id), titleFont));
                doc.Add(new Paragraph(" "));

				float cellPadding = 4f;
                var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);

                var productsTable = new PdfPTable(_catalogSettings.ShowProductSku ? 5 : 4);
                productsTable.WidthPercentage = 100f;
                productsTable.SetWidths(_catalogSettings.ShowProductSku ? new[] { 40, 15, 15, 15, 15 } : new[] { 40, 20, 20, 20 });

                //product name
                cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductName", lang.Id), font));
				cell.Padding = cellPadding;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cell);

                //SKU
                if (_catalogSettings.ShowProductSku)
                {
                    cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.SKU", lang.Id), font));
					cell.Padding = cellPadding;
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                //price
                cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductPrice", lang.Id), font));
				cell.Padding = cellPadding;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                productsTable.AddCell(cell);

                //qty
                cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductQuantity", lang.Id), font));
				cell.Padding = cellPadding;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //total
                cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductTotal", lang.Id), font));
				cell.Padding = cellPadding;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                productsTable.AddCell(cell);

                for (int i = 0; i < orderItems.Count; i++)
                {
                    var orderItem = orderItems[i];
                    var p = orderItem.Product;

                    //product name
					string name = p.GetLocalized(x => x.Name, lang.Id);
					cell = new PdfPCell();
					cell.Padding = cellPadding;
					cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.AddElement(new Paragraph(name, font));

					if (p.ProductType == ProductType.BundledProduct)
					{
						var itemParagraph = new Paragraph("", font);
						itemParagraph.IndentationLeft = 25;

						var bundleData = orderItem.GetBundleData();
						foreach (var bundleItem in bundleData)
						{
							if (bundleData.IndexOf(bundleItem) != 0)
								itemParagraph.Add(new Paragraph(" "));

							if (bundleItem.PerItemShoppingCart && bundleItem.Quantity > 1)
								itemParagraph.Add(new Paragraph("{0} × {1}".FormatWith(bundleItem.ProductName, bundleItem.Quantity), font));
							else
								itemParagraph.Add(new Paragraph(bundleItem.ProductName, font));

							if (bundleItem.PerItemShoppingCart)
							{
								decimal priceWithDiscount = _currencyService.ConvertCurrency(bundleItem.PriceWithDiscount, order.CurrencyRate);
								itemParagraph.Add(new Paragraph(_priceFormatter.FormatPrice(priceWithDiscount, true, order.CustomerCurrencyCode, lang, false), font));
							}
							
							if (bundleItem.AttributesInfo.HasValue())
								itemParagraph.Add(new Paragraph(HtmlUtils.ConvertHtmlToPlainText(bundleItem.AttributesInfo, true, true), attributesFont));
						}
						cell.AddElement(itemParagraph);
					}
					else
					{
						var attributesParagraph = new Paragraph(HtmlUtils.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
						cell.AddElement(attributesParagraph);
					}

                    productsTable.AddCell(cell);

                    //SKU
                    if (_catalogSettings.ShowProductSku)
                    {
						p.MergeWithCombination(orderItem.AttributesXml, _productAttributeParser);
                        cell = new PdfPCell(new Phrase(p.Sku ?? String.Empty, font));
						cell.Padding = cellPadding;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        productsTable.AddCell(cell);
                    }

                    //price
                    string unitPrice = string.Empty;
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                                unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                                unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                            }
                            break;
                    }
                    cell = new PdfPCell(new Phrase(unitPrice, font));
					cell.Padding = cellPadding;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    productsTable.AddCell(cell);

                    //qty
                    cell = new PdfPCell(new Phrase(orderItem.Quantity.ToString(), font));
					cell.Padding = cellPadding;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //total
                    string subTotal = string.Empty;
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                                subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                                subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                            }
                            break;
                    }
                    cell = new PdfPCell(new Phrase(subTotal, font));
					cell.Padding = cellPadding;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    productsTable.AddCell(cell);
                }
                doc.Add(productsTable);

                #endregion

                #region Checkout attributes

                if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    doc.Add(new Paragraph(" "));
                    string attributes = HtmlUtils.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true);
                    var pCheckoutAttributes = new Paragraph(attributes, font);
                    pCheckoutAttributes.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(pCheckoutAttributes);
                    doc.Add(new Paragraph(" "));
                }

                #endregion

                #region Totals

                //subtotal
                doc.Add(new Paragraph(" "));
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                            string orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                            var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), orderSubtotalExclTaxStr), font);
                            p.Alignment = Element.ALIGN_RIGHT;
                            doc.Add(p);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                            string orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                            var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), orderSubtotalInclTaxStr), font);
                            p.Alignment = Element.ALIGN_RIGHT;
                            doc.Add(p);
                        }
                        break;
                }
                //discount (applied to order subtotal)
                if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                                string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                                var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), orderSubTotalDiscountInCustomerCurrencyStr), font);
                                p.Alignment = Element.ALIGN_RIGHT;
                                doc.Add(p);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                                string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                                var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), orderSubTotalDiscountInCustomerCurrencyStr), font);
                                p.Alignment = Element.ALIGN_RIGHT;
                                doc.Add(p);
                            }
                            break;
                    }
                }

                //shipping
                if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                                string orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                                var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), orderShippingExclTaxStr), font);
                                p.Alignment = Element.ALIGN_RIGHT;
                                doc.Add(p);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                                string orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                                var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), orderShippingInclTaxStr), font);
                                p.Alignment = Element.ALIGN_RIGHT;
                                doc.Add(p);
                            }
                            break;
                    }
                }

                //payment fee
                if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                                string paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                                var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), paymentMethodAdditionalFeeExclTaxStr), font);
                                p.Alignment = Element.ALIGN_RIGHT;
                                doc.Add(p);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                                string paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                                var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), paymentMethodAdditionalFeeInclTaxStr), font);
                                p.Alignment = Element.ALIGN_RIGHT;
                                doc.Add(p);
                            }
                            break;
                    }
                }

                //tax
                string taxStr = string.Empty;
                var taxRates = new SortedDictionary<decimal, decimal>();
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                }
                else
                {
                    if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        taxRates = order.TaxRatesDictionary;

                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                        taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);
                    }
                }
                if (displayTax)
                {
                    var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Tax", lang.Id), taxStr), font);
                    p.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(p);
                }
                if (displayTaxRates)
                {
                    foreach (var item in taxRates)
                    {
                        string taxRate = String.Format(_localizationService.GetResource("PDFInvoice.TaxRate", lang.Id), _priceFormatter.FormatTaxRate(item.Key));
                        string taxValue = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, lang);

                        var p = new Paragraph(String.Format("{0} {1}", taxRate, taxValue), font);
                        p.Alignment = Element.ALIGN_RIGHT;
                        doc.Add(p);
                    }
                }

                //discount (applied to order total)
                if (order.OrderDiscount > decimal.Zero)
                {
                    var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                    string orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);

                    var p = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), orderDiscountInCustomerCurrencyStr), font);
                    p.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(p);
                }

                //gift cards
                foreach (var gcuh in order.GiftCardUsageHistory)
                {
                    string gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", lang.Id), gcuh.GiftCard.GiftCardCouponCode);
                    string gcAmountStr = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, lang);

                    var p = new Paragraph(String.Format("{0} {1}", gcTitle, gcAmountStr), font);
                    p.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(p);
                }

                //reward points
                if (order.RedeemedRewardPointsEntry != null)
                {
                    string rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", lang.Id), -order.RedeemedRewardPointsEntry.Points);
                    string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, lang);

                    var p = new Paragraph(String.Format("{0} {1}", rpTitle, rpAmount), font);
                    p.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(p);
                }

                //order total
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                string orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);


                var pTotal = new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id), orderTotalStr), titleFont);
                pTotal.Alignment = Element.ALIGN_RIGHT;
                doc.Add(pTotal);

                #endregion

                #region Order notes

                if (_pdfSettings.RenderOrderNotes)
                {
                    var orderNotes = order.OrderNotes
                        .Where(on => on.DisplayToCustomer)
                        .OrderByDescending(on => on.CreatedOnUtc)
                        .ToList();
                    if (orderNotes.Count > 0)
                    {
                        doc.Add(new Paragraph(_localizationService.GetResource("PDFInvoice.OrderNotes", lang.Id), titleFont));

                        doc.Add(new Paragraph(" "));

                        var notesTable = new PdfPTable(2);
                        notesTable.WidthPercentage = 100f;
                        notesTable.SetWidths(new[] { 30, 70 });

                        //created on
                        cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.CreatedOn", lang.Id), font));
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        notesTable.AddCell(cell);

                        //note
                        cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.Note", lang.Id), font));
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        notesTable.AddCell(cell);

                        foreach (var orderNote in orderNotes)
                        {
                            cell = new PdfPCell();
                            cell.AddElement(new Paragraph(_dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc).ToString(), font));
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            notesTable.AddCell(cell);

                            cell = new PdfPCell();
                            cell.AddElement(new Paragraph(HtmlUtils.ConvertHtmlToPlainText(orderNote.FormatOrderNoteText(), true, true), font));
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            notesTable.AddCell(cell);
                        }
                        doc.Add(notesTable);
                    }
                }

                #endregion

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="lang">Language</param>
        public virtual void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, Language lang)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (shipments == null)
                throw new ArgumentNullException("shipments");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }

            var doc = new Document(pageSize);
            var writer = PdfWriter.GetInstance(doc, stream);
            
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            int shipmentCount = shipments.Count;
            int shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = shipment.Order;
                if (order.ShippingAddress != null)
                {
                    doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Shipment", lang.Id), shipment.Id), titleFont));
                    doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Order", lang.Id), order.GetOrderNumber()), titleFont));

                    if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(order.ShippingAddress.Company))
                        doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Company", lang.Id), order.ShippingAddress.Company), font));

                    doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Name", lang.Id), order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName), font));
                    if (_addressSettings.PhoneEnabled)
                        doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Phone", lang.Id), order.ShippingAddress.PhoneNumber), font));
                    if (_addressSettings.StreetAddressEnabled)
                        doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Address", lang.Id), order.ShippingAddress.Address1), font));

                    if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.ShippingAddress.Address2))
                        doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Address2", lang.Id), order.ShippingAddress.Address2), font));

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
                        doc.Add(new Paragraph(String.Format("{0}, {1} {2}", order.ShippingAddress.City, order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name, lang.Id) : "", order.ShippingAddress.ZipPostalCode), font));

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                        doc.Add(new Paragraph(String.Format("{0}", order.ShippingAddress.Country != null ? order.ShippingAddress.Country.GetLocalized(x => x.Name, lang.Id) : ""), font));

                    doc.Add(new Paragraph(" "));

                    doc.Add(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.ShippingMethod", lang.Id), order.ShippingMethod), font));
                    doc.Add(new Paragraph(" "));

                    var productsTable = new PdfPTable(3);
                    productsTable.WidthPercentage = 100f;
                    productsTable.SetWidths(new[] { 60, 20, 20 });

                    //product name
                    var cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.ProductName", lang.Id), font));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //SKU
                    cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.SKU", lang.Id), font));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.QTY", lang.Id), font));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    foreach (var si in shipment.ShipmentItems)
                    {
                        //product name
                        var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
                        if (orderItem == null)
                            continue;

                        var p = orderItem.Product;
						string name = p.GetLocalized(x => x.Name, lang.Id);
                        cell = new PdfPCell();
                        cell.AddElement(new Paragraph(name, font));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        var attributesParagraph = new Paragraph(HtmlUtils.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        cell.AddElement(attributesParagraph);
                        productsTable.AddCell(cell);

                        //SKU
                        p.MergeWithCombination(orderItem.AttributesXml, _productAttributeParser);
                        cell = new PdfPCell(new Phrase(p.Sku ?? String.Empty, font));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        productsTable.AddCell(cell);

                        //qty
                        cell = new PdfPCell(new Phrase(si.Quantity.ToString(), font));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        productsTable.AddCell(cell);
                    }
                    doc.Add(productsTable);
                }

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                {
                    doc.NewPage();
                }
            }


            doc.Close();
        }

        /// <summary>
        /// Print product collection to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        /// <param name="lang">Language</param>
        public virtual void PrintProductsToPdf(Stream stream, IList<Product> products, Language lang)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (products == null)
                throw new ArgumentNullException("products");

            if (lang == null)
                throw new ArgumentNullException("lang");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();

            int productNumber = 1;
            int prodCount = products.Count;
			string currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
			string measureWeightName = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
			string labelPrice = _localizationService.GetResource("PDFProductCatalog.Price", lang.Id);
			string labelSku = _localizationService.GetResource("PDFProductCatalog.SKU", lang.Id);
			string labelWeight = _localizationService.GetResource("PDFProductCatalog.Weight", lang.Id);
			string labelStock = _localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id);

            foreach (var product in products)
            {
                string productName = product.GetLocalized(x => x.Name, lang.Id);
                string productFullDescription = product.GetLocalized(x => x.FullDescription, lang.Id);

                doc.Add(new Paragraph(String.Format("{0}. {1}", productNumber, productName), titleFont));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(HtmlUtils.StripTags(HtmlUtils.ConvertHtmlToPlainText(productFullDescription)), font));
                doc.Add(new Paragraph(" "));

				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					doc.Add(new Paragraph(String.Format("{0}: {1} {2}", labelPrice, product.Price.ToString("0.00"), currencyCode), font));
					doc.Add(new Paragraph(String.Format("{0}: {1}", labelSku, product.Sku), font));

					if (product.IsShipEnabled && product.Weight > Decimal.Zero)
						doc.Add(new Paragraph(String.Format("{0}: {1} {2}", labelWeight, product.Weight.ToString("0.00"), measureWeightName), font));

					if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
						doc.Add(new Paragraph(String.Format("{0}: {1}", labelStock, product.StockQuantity), font));

					doc.Add(new Paragraph(" "));
				}

                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                if (pictures.Count > 0)
                {
                    var table = new PdfPTable(2);
                    table.WidthPercentage = 100f;

                    for (int i = 0; i < pictures.Count; i++)
                    {
                        var pic = pictures[i];
                        if (pic != null)
                        {
                            var picBinary = _pictureService.LoadPictureBinary(pic);
                            if (picBinary != null && picBinary.Length > 0)
                            {
                                var pictureLocalPath = _pictureService.GetThumbLocalPath(pic, 200, false);
								if (pictureLocalPath.HasValue()) {
									var cell = new PdfPCell(Image.GetInstance(pictureLocalPath));
									cell.HorizontalAlignment = Element.ALIGN_LEFT;
									cell.Border = Rectangle.NO_BORDER;
									cell.PaddingBottom = 5f;
									table.AddCell(cell);
								}
                            }
                        }
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "));
                        cell.Border = Rectangle.NO_BORDER;
                        table.AddCell(cell);
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph(" "));
                }

				if (product.ProductType == ProductType.GroupedProduct)
				{
					//grouped product. render its associated products
					int pNum = 1;
					var searchContext = new ProductSearchContext()
					{
						ParentGroupedProductId = product.Id,
						ShowHidden = true
					};

					foreach (var associatedProduct in _productService.SearchProducts(searchContext))
					{
						doc.Add(new Paragraph(String.Format("{0}-{1}. {2}", productNumber, pNum, associatedProduct.GetLocalized(x => x.Name, lang.Id)), font));
						doc.Add(new Paragraph(" "));

						//uncomment to render associated product description
						//string apDescription = associatedProduct.GetLocalized(x => x.ShortDescription, lang.Id);
						//if (!String.IsNullOrEmpty(apDescription))
						//{
						//    doc.Add(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(apDescription)), font));
						//    doc.Add(new Paragraph(" "));
						//}

						//uncomment to render associated product picture
						//var apPicture = _pictureService.GetPicturesByProductId(associatedProduct.Id).FirstOrDefault();
						//if (apPicture != null)
						//{
						//    var picBinary = _pictureService.LoadPictureBinary(apPicture);
						//    if (picBinary != null && picBinary.Length > 0)
						//    {
						//        var pictureLocalPath = _pictureService.GetThumbLocalPath(apPicture, 200, false);
						//        doc.Add(Image.GetInstance(pictureLocalPath));
						//    }
						//}

						doc.Add(new Paragraph(String.Format("{0}: {1} {2}", labelPrice, associatedProduct.Price.ToString("0.00"), currencyCode), font));
						doc.Add(new Paragraph(String.Format("{0}: {1}", labelSku, associatedProduct.Sku), font));

						if (associatedProduct.IsShipEnabled && associatedProduct.Weight > Decimal.Zero)
							doc.Add(new Paragraph(String.Format("{0}: {1} {2}", labelWeight, associatedProduct.Weight.ToString("0.00"), measureWeightName), font));

						if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
							doc.Add(new Paragraph(String.Format("{0}: {1}", labelStock, associatedProduct.StockQuantity), font));

						doc.Add(new Paragraph(" "));

						pNum++;
					}
				}
				else if (product.ProductType == ProductType.BundledProduct)
				{
					int pNum = 1;

					foreach (var item in _productService.GetBundleItems(product.Id).Select(x => x.Item))
					{
						doc.Add(new Paragraph("{0}-{1}. {2}".FormatWith(productNumber, pNum, item.GetLocalizedName(lang.Id)), font));

						doc.Add(new Paragraph(String.Format("{0}: {1} {2}", labelPrice, item.Product.Price.ToString("0.00"), currencyCode), font));
						doc.Add(new Paragraph(String.Format("{0}: {1}", labelSku, item.Product.Sku), font));

						if (item.Product.IsShipEnabled && item.Product.Weight > Decimal.Zero)
							doc.Add(new Paragraph(String.Format("{0}: {1} {2}", labelWeight, item.Product.Weight.ToString("0.00"), measureWeightName), font));

						if (item.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
							doc.Add(new Paragraph(String.Format("{0}: {1}", labelStock, item.Product.StockQuantity), font));

						doc.Add(new Paragraph(" "));

						pNum++;
					}
				}

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion


        #region PdfEvents

        public class OrderPdfPageEvents : PdfPageEventHelper
        {
            private readonly ILocalizationService _localizationService;
            private readonly IPictureService _pictureService;
            private readonly PdfSettings _pdfSettings;
            private readonly CompanyInformationSettings _companyInformationSettings;
            private readonly BankConnectionSettings _bankConnectionSettings;
            private readonly ContactDataSettings _contactDataSettings;
			private readonly IStoreContext _storeContext;
            private readonly Language _lang;

            public OrderPdfPageEvents(IPictureService pictureService, PdfSettings pdfSettings, 
                CompanyInformationSettings companyInformationSettings, BankConnectionSettings bankConnectionSettings,
                ContactDataSettings contactDataSettings, 
                ILocalizationService localizationService, Language lang,
				IStoreContext storeContext)
            {
                this._localizationService = localizationService;
                this._pictureService = pictureService;
                this._pdfSettings = pdfSettings;
                this._companyInformationSettings = companyInformationSettings;
                this._bankConnectionSettings = bankConnectionSettings;
                this._contactDataSettings = contactDataSettings;
				this._storeContext = storeContext;
                this._lang = lang;
            }

            private float MilimetersToPoints(int mm)
            {
                return iTextSharp.text.Utilities.MillimetersToPoints(Convert.ToSingle(mm));
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);
                Rectangle pageSize = document.PageSize;
                PdfContentByte m_Cb = writer.DirectContent;
                m_Cb.MoveTo(pageSize.GetLeft(MilimetersToPoints(15)), pageSize.GetBottom(MilimetersToPoints(15)) + 40);
                m_Cb.LineTo(pageSize.GetRight(MilimetersToPoints(15)), pageSize.GetBottom(MilimetersToPoints(15)) + 40);
                m_Cb.Stroke();

                //variables
                var font = new Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL);

                // Header - Logo

                //logo
                //var logoPicture = _pictureService.GetPictureById(_pdfSettings.LogoPictureId);
                //var logoExists = logoPicture != null;

                ////header
                //var headerTable = new PdfPTable(logoExists ? 2 : 1);
                //headerTable.WidthPercentage = 100f;
                //if (logoExists)
                //    headerTable.SetWidths(new[] { 50, 50 });

                ////logo
                //if (logoExists)
                //{
                //    var logoFilePath = _pictureService.GetPictureLocalPath(logoPicture, 0, false);
                //    //var img = Image.GetInstance(logoFilePath);
                //    //var cellLogo = new PdfPCell(img);
                //    //cellLogo.Border = Rectangle.NO_BORDER;
                //    //headerTable.AddCell(cellLogo);
                //    //document.Add(headerTable);

                //    Image img = Image.GetInstance(logoFilePath);
                //    img.SetAbsolutePosition(pageSize.GetRight(MilimetersToPoints(20)) - img.ScaledWidth + 10, pageSize.GetTop(MilimetersToPoints(10) + img.ScaledHeight));
                //    document.Add(img);
                //}

                // Footer
                var tableCompanyInfo = new PdfPTable(3);
                tableCompanyInfo.WidthPercentage = 80;
                tableCompanyInfo.SetTotalWidth(new float[] {200, 200, 200});

                var cellInfo = new PdfPCell();
                var cellContact = new PdfPCell();
                var cellBank = new PdfPCell();

                //border
                cellInfo.Border = 0;
                cellContact.Border = 0;
                cellBank.Border = 0;
                cellInfo.HorizontalAlignment = Element.ALIGN_LEFT;
                cellContact.HorizontalAlignment = Element.ALIGN_CENTER;
                cellBank.HorizontalAlignment = Element.ALIGN_RIGHT;

                cellInfo.AddElement(new Phrase(_companyInformationSettings.CompanyName, font));
                cellInfo.AddElement(new Phrase(_companyInformationSettings.Salutation + 
                    (String.IsNullOrEmpty(_companyInformationSettings.Title) ? (_companyInformationSettings.Title + " ") : "") +
                    _companyInformationSettings.Firstname + " " + _companyInformationSettings.Lastname
                    , font));
                cellInfo.AddElement(new Phrase(_companyInformationSettings.Street + " " + _companyInformationSettings.Street2, font));
                cellInfo.AddElement(new Phrase(_companyInformationSettings.ZipCode + " " + _companyInformationSettings.City, font));
                cellInfo.AddElement(new Phrase(_companyInformationSettings.CountryName + (!String.IsNullOrEmpty(_companyInformationSettings.StateName) ? (", " + _companyInformationSettings.StateName) : ""), font));

                cellContact.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Url", _lang.Id), _storeContext.CurrentStore.Url), font));
                cellContact.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Mail", _lang.Id), _contactDataSettings.ContactEmailAddress), font));
                cellContact.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Fon", _lang.Id), _contactDataSettings.CompanyTelephoneNumber), font));
                cellContact.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Fax", _lang.Id), _contactDataSettings.CompanyFaxNumber), font));

                cellBank.AddElement(new Phrase(_bankConnectionSettings.Bankname, font));
                cellBank.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Bankcode", _lang.Id), _bankConnectionSettings.Bankcode), font));
                cellBank.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.AccountNumber", _lang.Id), _bankConnectionSettings.AccountNumber), font));
                cellBank.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.AccountHolder", _lang.Id), _bankConnectionSettings.AccountHolder), font));
                cellBank.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Iban", _lang.Id), _bankConnectionSettings.Iban), font));
                cellBank.AddElement(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Footer.Bic", _lang.Id), _bankConnectionSettings.Bic), font));


                tableCompanyInfo.AddCell(cellInfo);
                tableCompanyInfo.AddCell(cellContact);
                tableCompanyInfo.AddCell(cellBank);

                tableCompanyInfo.WriteSelectedRows(0, 6, pageSize.GetLeft(MilimetersToPoints(15)), pageSize.GetBottom(MilimetersToPoints(15)) + 40, m_Cb);
            }
        }



        #endregion

    }
}