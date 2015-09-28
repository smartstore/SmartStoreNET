using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	[SystemName("Exports.SmartStoreNetOrderXml")]
	[FriendlyName("SmartStore.NET order export")]
	[IsHidden(true)]
	public class OrderExportXmlProvider : IExportProvider
	{
		private void WriteAddress(XmlWriter writer, string node, dynamic address, CultureInfo culture)
		{
			if (address == null)
				return;

			int? countryId = address.CountryId;
			int? stateProvinceId = address.StateProvinceId;

			writer.WriteStartElement(node);

			writer.WriteElementString("Id", ((int)address.Id).ToString());
			writer.WriteElementString("FirstName", (string)address.FirstName);
			writer.WriteElementString("LastName", (string)address.LastName);
			writer.WriteElementString("Email", (string)address.Email);
			writer.WriteElementString("Company", (string)address.Company);
			writer.WriteElementString("CountryId", countryId.HasValue ? countryId.Value.ToString() : "");
			writer.WriteElementString("StateProvinceId", stateProvinceId.HasValue ? stateProvinceId.Value.ToString() : "");
			writer.WriteElementString("City", (string)address.City);
			writer.WriteElementString("Address1", (string)address.Address1);
			writer.WriteElementString("Address2", (string)address.Address2);
			writer.WriteElementString("ZipPostalCode", (string)address.ZipPostalCode);
			writer.WriteElementString("PhoneNumber", (string)address.PhoneNumber);
			writer.WriteElementString("FaxNumber", (string)address.FaxNumber);
			writer.WriteElementString("CreatedOnUtc", ((DateTime)address.CreatedOnUtc).ToString(culture));

			if (address.Country != null)
			{
				dynamic country = address.Country;

				writer.WriteStartElement("Country");
				writer.WriteElementString("Id", ((int)country.Id).ToString());
				writer.WriteElementString("Name", (string)country.Name);
				writer.WriteElementString("AllowsBilling", ((bool)country.AllowsBilling).ToString());
				writer.WriteElementString("AllowsShipping", ((bool)country.AllowsShipping).ToString());
				writer.WriteElementString("TwoLetterIsoCode", (string)country.TwoLetterIsoCode);
				writer.WriteElementString("ThreeLetterIsoCode", (string)country.ThreeLetterIsoCode);
				writer.WriteElementString("NumericIsoCode", ((int)country.NumericIsoCode).ToString());
				writer.WriteElementString("SubjectToVat", ((bool)country.SubjectToVat).ToString());
				writer.WriteElementString("Published", ((bool)country.Published).ToString());
				writer.WriteElementString("DisplayOrder", ((int)country.DisplayOrder).ToString());
				writer.WriteElementString("LimitedToStores", ((bool)country.LimitedToStores).ToString());
				writer.WriteEndElement();	// Country
			}

			if (address.StateProvince != null)
			{
				dynamic stateProvince = address.StateProvince;

				writer.WriteStartElement("StateProvince");
				writer.WriteElementString("Id", ((int)stateProvince.Id).ToString());
				writer.WriteElementString("CountryId", ((int)stateProvince.CountryId).ToString());
				writer.WriteElementString("Name", (string)stateProvince.Name);
				writer.WriteElementString("Abbreviation", (string)stateProvince.Abbreviation);
				writer.WriteElementString("Published", ((bool)stateProvince.Published).ToString());
				writer.WriteElementString("DisplayOrder", ((int)stateProvince.DisplayOrder).ToString());
				writer.WriteEndElement();	// StateProvince
			}

			writer.WriteEndElement();	// node
		}

		private void WriteCurrency(XmlWriter writer, string node, dynamic currency, CultureInfo culture)
		{
			if (currency == null)
				return;

			writer.WriteStartElement(node);

			writer.WriteElementString("Id", ((int)currency.Id).ToString());
			writer.WriteElementString("Name", (string)currency.Name);
			writer.WriteElementString("CurrencyCode", (string)currency.CurrencyCode);
			writer.WriteElementString("Rate", ((decimal)currency.Rate).ToString(culture));
			writer.WriteElementString("DisplayLocale", (string)currency.DisplayLocale);
			writer.WriteElementString("CustomFormatting", (string)currency.CustomFormatting);
			writer.WriteElementString("LimitedToStores", ((bool)currency.LimitedToStores).ToString());
			writer.WriteElementString("Published", ((bool)currency.Published).ToString());
			writer.WriteElementString("DisplayOrder", ((int)currency.DisplayOrder).ToString());
			writer.WriteElementString("CreatedOnUtc", ((DateTime)currency.CreatedOnUtc).ToString(culture));
			writer.WriteElementString("UpdatedOnUtc", ((DateTime)currency.UpdatedOnUtc).ToString(culture));
			writer.WriteElementString("DomainEndings", (string)currency.DomainEndings);

			writer.WriteEndElement();	// node
		}

		private void WriteProduct(XmlWriter writer, string node, dynamic product, CultureInfo culture)
		{
			if (product == null)
				return;

			int? downloadExpirationDays = product.DownloadExpirationDays;
			int? sampleDownloadId = product.SampleDownloadId;
			decimal? specialPrice = product.SpecialPrice;
			DateTime? specialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc;
			DateTime? specialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc;
			DateTime? availableStartDateTimeUtc = product.AvailableStartDateTimeUtc;
			DateTime? availableEndDateTimeUtc = product.AvailableEndDateTimeUtc;
			decimal? basePriceAmount = product.BasePriceAmount;
			int? basePriceBaseAmount = product.BasePriceBaseAmount;
			decimal? lowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice;

			writer.WriteStartElement(node);

			writer.WriteElementString("Id", ((int)product.Id).ToString());
			writer.WriteElementString("Name", ((string)product.Name).RemoveInvalidXmlChars());
			writer.WriteElementString("SeName", (string)product.SeName);
			writer.WriteElementString("ShortDescription", ((string)product.ShortDescription).RemoveInvalidXmlChars());
			writer.WriteElementString("FullDescription", ((string)product.FullDescription).RemoveInvalidXmlChars());
			writer.WriteElementString("AdminComment", ((string)product.AdminComment).RemoveInvalidXmlChars());
			writer.WriteElementString("ProductTemplateId", ((int)product.ProductTemplateId).ToString());
			writer.WriteElementString("ProductTemplateViewPath", (string)product._ProductTemplateViewPath);
			writer.WriteElementString("ShowOnHomePage", ((bool)product.ShowOnHomePage).ToString());
			writer.WriteElementString("HomePageDisplayOrder", ((int)product.HomePageDisplayOrder).ToString());
			writer.WriteElementString("MetaKeywords", ((string)product.MetaKeywords).RemoveInvalidXmlChars());
			writer.WriteElementString("MetaDescription", ((string)product.MetaDescription).RemoveInvalidXmlChars());
			writer.WriteElementString("MetaTitle", ((string)product.MetaTitle).RemoveInvalidXmlChars());
			writer.WriteElementString("AllowCustomerReviews", ((bool)product.AllowCustomerReviews).ToString());
			writer.WriteElementString("ApprovedRatingSum", ((int)product.ApprovedRatingSum).ToString());
			writer.WriteElementString("NotApprovedRatingSum", ((int)product.NotApprovedRatingSum).ToString());
			writer.WriteElementString("ApprovedTotalReviews", ((int)product.ApprovedTotalReviews).ToString());
			writer.WriteElementString("NotApprovedTotalReviews", ((int)product.NotApprovedTotalReviews).ToString());
			writer.WriteElementString("Published", ((bool)product.Published).ToString());
			writer.WriteElementString("CreatedOnUtc", ((DateTime)product.CreatedOnUtc).ToString(culture));
			writer.WriteElementString("UpdatedOnUtc", ((DateTime)product.UpdatedOnUtc).ToString(culture));
			writer.WriteElementString("SubjectToAcl", ((bool)product.SubjectToAcl).ToString());
			writer.WriteElementString("LimitedToStores", ((bool)product.LimitedToStores).ToString());
			writer.WriteElementString("ProductTypeId", ((int)product.ProductTypeId).ToString());
			writer.WriteElementString("ParentGroupedProductId", ((int)product.ParentGroupedProductId).ToString());
			writer.WriteElementString("Sku", (string)product.Sku);
			writer.WriteElementString("ManufacturerPartNumber", (string)product.ManufacturerPartNumber);
			writer.WriteElementString("Gtin", (string)product.Gtin);
			writer.WriteElementString("IsGiftCard", ((bool)product.IsGiftCard).ToString());
			writer.WriteElementString("GiftCardTypeId", ((int)product.GiftCardTypeId).ToString());
			writer.WriteElementString("RequireOtherProducts", ((bool)product.RequireOtherProducts).ToString());
			writer.WriteElementString("RequiredProductIds", (string)product.RequiredProductIds);
			writer.WriteElementString("AutomaticallyAddRequiredProducts", ((bool)product.AutomaticallyAddRequiredProducts).ToString());
			writer.WriteElementString("IsDownload", ((bool)product.IsDownload).ToString());
			writer.WriteElementString("DownloadId", ((int)product.DownloadId).ToString());
			writer.WriteElementString("UnlimitedDownloads", ((bool)product.UnlimitedDownloads).ToString());
			writer.WriteElementString("MaxNumberOfDownloads", ((int)product.MaxNumberOfDownloads).ToString());
			writer.WriteElementString("DownloadExpirationDays", downloadExpirationDays.HasValue ? downloadExpirationDays.Value.ToString() : "");
			writer.WriteElementString("DownloadActivationTypeId", ((int)product.DownloadActivationTypeId).ToString());
			writer.WriteElementString("HasSampleDownload", ((bool)product.HasSampleDownload).ToString());
			writer.WriteElementString("SampleDownloadId", sampleDownloadId.HasValue ? sampleDownloadId.Value.ToString() : "");
			writer.WriteElementString("HasUserAgreement", ((bool)product.HasUserAgreement).ToString());
			writer.WriteElementString("UserAgreementText", ((string)product.UserAgreementText).RemoveInvalidXmlChars());
			writer.WriteElementString("IsRecurring", ((bool)product.IsRecurring).ToString());
			writer.WriteElementString("RecurringCycleLength", ((int)product.RecurringCycleLength).ToString());
			writer.WriteElementString("RecurringCyclePeriodId", ((int)product.RecurringCyclePeriodId).ToString());
			writer.WriteElementString("RecurringTotalCycles", ((int)product.RecurringTotalCycles).ToString());
			writer.WriteElementString("IsShipEnabled", ((bool)product.IsShipEnabled).ToString());
			writer.WriteElementString("IsFreeShipping", ((bool)product.IsFreeShipping).ToString());
			writer.WriteElementString("AdditionalShippingCharge", ((decimal)product.AdditionalShippingCharge).ToString(culture));
			writer.WriteElementString("IsTaxExempt", ((bool)product.IsTaxExempt).ToString());
			writer.WriteElementString("TaxCategoryId", ((int)product.TaxCategoryId).ToString());
			writer.WriteElementString("ManageInventoryMethodId", ((int)product.ManageInventoryMethodId).ToString());
			writer.WriteElementString("StockQuantity", ((int)product.StockQuantity).ToString());
			writer.WriteElementString("DisplayStockAvailability", ((bool)product.DisplayStockAvailability).ToString());
			writer.WriteElementString("DisplayStockQuantity", ((bool)product.DisplayStockQuantity).ToString());
			writer.WriteElementString("MinStockQuantity", ((int)product.MinStockQuantity).ToString());
			writer.WriteElementString("LowStockActivityId", ((int)product.LowStockActivityId).ToString());
			writer.WriteElementString("NotifyAdminForQuantityBelow", ((int)product.NotifyAdminForQuantityBelow).ToString());
			writer.WriteElementString("BackorderModeId", ((int)product.BackorderModeId).ToString());
			writer.WriteElementString("AllowBackInStockSubscriptions", ((bool)product.AllowBackInStockSubscriptions).ToString());
			writer.WriteElementString("OrderMinimumQuantity", ((int)product.OrderMinimumQuantity).ToString());
			writer.WriteElementString("OrderMaximumQuantity", ((int)product.OrderMaximumQuantity).ToString());
			writer.WriteElementString("AllowedQuantities", (string)product.AllowedQuantities);
			writer.WriteElementString("DisableBuyButton", ((bool)product.DisableBuyButton).ToString());
			writer.WriteElementString("DisableWishlistButton", ((bool)product.DisableWishlistButton).ToString());
			writer.WriteElementString("AvailableForPreOrder", ((bool)product.AvailableForPreOrder).ToString());
			writer.WriteElementString("CallForPrice", ((bool)product.CallForPrice).ToString());
			writer.WriteElementString("Price", ((decimal)product.Price).ToString(culture));
			writer.WriteElementString("OldPrice", ((decimal)product.OldPrice).ToString(culture));
			writer.WriteElementString("ProductCost", ((decimal)product.ProductCost).ToString(culture));
			writer.WriteElementString("SpecialPrice", specialPrice.HasValue ? specialPrice.Value.ToString(culture) : "");
			writer.WriteElementString("SpecialPriceStartDateTimeUtc", specialPriceStartDateTimeUtc.HasValue ? specialPriceStartDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("SpecialPriceEndDateTimeUtc", specialPriceEndDateTimeUtc.HasValue ? specialPriceEndDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("CustomerEntersPrice", ((bool)product.CustomerEntersPrice).ToString());
			writer.WriteElementString("MinimumCustomerEnteredPrice", ((decimal)product.MinimumCustomerEnteredPrice).ToString(culture));
			writer.WriteElementString("MaximumCustomerEnteredPrice", ((decimal)product.MaximumCustomerEnteredPrice).ToString(culture));
			writer.WriteElementString("HasTierPrices", ((bool)product.HasTierPrices).ToString());
			writer.WriteElementString("HasDiscountsApplied", ((bool)product.HasDiscountsApplied).ToString());
			writer.WriteElementString("Weight", ((decimal)product.Weight).ToString(culture));
			writer.WriteElementString("Length", ((decimal)product.Length).ToString(culture));
			writer.WriteElementString("Width", ((decimal)product.Width).ToString(culture));
			writer.WriteElementString("Height", ((decimal)product.Height).ToString(culture));
			writer.WriteElementString("AvailableStartDateTimeUtc", availableStartDateTimeUtc.HasValue ? availableStartDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("AvailableEndDateTimeUtc", availableEndDateTimeUtc.HasValue ? availableEndDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("BasePriceEnabled", ((bool)product.BasePriceEnabled).ToString());
			writer.WriteElementString("BasePriceMeasureUnit", (string)product.BasePriceMeasureUnit);
			writer.WriteElementString("BasePriceAmount", basePriceAmount.HasValue ? basePriceAmount.Value.ToString(culture) : "");
			writer.WriteElementString("BasePriceBaseAmount", basePriceBaseAmount.HasValue ? basePriceBaseAmount.Value.ToString() : "");
			writer.WriteElementString("BasePriceHasValue", ((bool)product.BasePriceHasValue).ToString());
			writer.WriteElementString("BasePriceInfo", (string)product._BasePriceInfo);
			writer.WriteElementString("VisibleIndividually", ((bool)product.VisibleIndividually).ToString());
			writer.WriteElementString("DisplayOrder", ((int)product.DisplayOrder).ToString());
			writer.WriteElementString("BundleTitleText", ((string)product.BundleTitleText).RemoveInvalidXmlChars());
			writer.WriteElementString("BundlePerItemPricing", ((bool)product.BundlePerItemPricing).ToString());
			writer.WriteElementString("BundlePerItemShipping", ((bool)product.BundlePerItemShipping).ToString());
			writer.WriteElementString("BundlePerItemShoppingCart", ((bool)product.BundlePerItemShoppingCart).ToString());
			writer.WriteElementString("LowestAttributeCombinationPrice", lowestAttributeCombinationPrice.HasValue ? lowestAttributeCombinationPrice.Value.ToString(culture) : "");
			writer.WriteElementString("IsEsd", ((bool)product.IsEsd).ToString());

			if (product.DeliveryTime != null)
			{
				writer.WriteStartElement("DeliveryTime");
				writer.WriteElementString("Id", ((int)product.DeliveryTime.Id).ToString());
				writer.WriteElementString("Name", ((string)product.DeliveryTime.Name).RemoveInvalidXmlChars());
				writer.WriteElementString("DisplayLocale", ((string)product.DeliveryTime.DisplayLocale).RemoveInvalidXmlChars());
				writer.WriteElementString("ColorHexValue", (string)product.DeliveryTime.ColorHexValue);
				writer.WriteElementString("DisplayOrder", ((int)product.DeliveryTime.DisplayOrder).ToString());
				writer.WriteEndElement();	// DeliveryTime
			}

			if (product.QuantityUnit != null)
			{
				writer.WriteStartElement("QuantityUnit");
				writer.WriteElementString("Id", ((int)product.QuantityUnit.Id).ToString());
				writer.WriteElementString("Name", ((string)product.QuantityUnit.Name).RemoveInvalidXmlChars());
				writer.WriteElementString("Description", ((string)product.QuantityUnit.Description).RemoveInvalidXmlChars());
				writer.WriteElementString("DisplayLocale", ((string)product.QuantityUnit.DisplayLocale).RemoveInvalidXmlChars());
				writer.WriteElementString("DisplayOrder", ((int)product.QuantityUnit.DisplayOrder).ToString());
				writer.WriteElementString("IsDefault", ((bool)product.QuantityUnit.IsDefault).ToString());
				writer.WriteEndElement();	// QuantityUnit
			}

			writer.WriteEndElement();	// node
		}

		public static string SystemName
		{
			get { return "Exports.SmartStoreNetOrderXml"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Order; }
		}

		public string FileExtension
		{
			get { return "XML"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
				Indent = true,
				IndentChars = "\t"
			};

			var path = context.FilePath;
			var invariantCulture = CultureInfo.InvariantCulture;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("Orders");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Data.ReadNextSegment())
				{
					var segment = context.Data.CurrentSegment;

					foreach (dynamic order in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						int orderId = order.Id;
						dynamic customer = order.Customer;
						dynamic store = order.Store;

						writer.WriteStartElement("Order");

						try
						{
							int? shippingAddressId = order.ShippingAddressId;
							DateTime? paidDateUtc = order.PaidDateUtc;
							int? rewardPointsRemaining = order.RewardPointsRemaining;

							writer.WriteElementString("Id", orderId.ToString());
							writer.WriteElementString("OrderNumber", (string)order.OrderNumber);
							writer.WriteElementString("OrderGuid", ((Guid)order.OrderGuid).ToString());
							writer.WriteElementString("StoreId", ((int)order.StoreId).ToString());
							writer.WriteElementString("CustomerId", ((int)order.CustomerId).ToString());
							writer.WriteElementString("BillingAddressId", ((int)order.BillingAddressId).ToString());
							writer.WriteElementString("ShippingAddressId", shippingAddressId.HasValue ? shippingAddressId.Value.ToString() : "");
							writer.WriteElementString("OrderStatusId", ((int)order.OrderStatusId).ToString());
							writer.WriteElementString("ShippingStatusId", ((int)order.ShippingStatusId).ToString());
							writer.WriteElementString("PaymentStatusId", ((int)order.PaymentStatusId).ToString());
							writer.WriteElementString("PaymentMethodSystemName", (string)order.PaymentMethodSystemName);
							writer.WriteElementString("CustomerCurrencyCode", (string)order.CustomerCurrencyCode);
							writer.WriteElementString("CurrencyRate", ((decimal)order.CurrencyRate).ToString(invariantCulture));
							writer.WriteElementString("CustomerTaxDisplayTypeId", ((int)order.CustomerTaxDisplayTypeId).ToString());
							writer.WriteElementString("VatNumber", (string)order.VatNumber);
							writer.WriteElementString("OrderSubtotalInclTax", ((decimal)order.OrderSubtotalInclTax).ToString(invariantCulture));
							writer.WriteElementString("OrderSubtotalExclTax", ((decimal)order.OrderSubtotalExclTax).ToString(invariantCulture));
							writer.WriteElementString("OrderSubTotalDiscountInclTax", ((decimal)order.OrderSubTotalDiscountInclTax).ToString(invariantCulture));
							writer.WriteElementString("OrderSubTotalDiscountExclTax", ((decimal)order.OrderSubTotalDiscountExclTax).ToString(invariantCulture));
							writer.WriteElementString("OrderShippingInclTax", ((decimal)order.OrderShippingInclTax).ToString(invariantCulture));
							writer.WriteElementString("OrderShippingExclTax", ((decimal)order.OrderShippingExclTax).ToString(invariantCulture));
							writer.WriteElementString("OrderShippingTaxRate", ((decimal)order.OrderShippingTaxRate).ToString(invariantCulture));
							writer.WriteElementString("PaymentMethodAdditionalFeeInclTax", ((decimal)order.PaymentMethodAdditionalFeeInclTax).ToString(invariantCulture));
							writer.WriteElementString("PaymentMethodAdditionalFeeExclTax", ((decimal)order.PaymentMethodAdditionalFeeExclTax).ToString(invariantCulture));
							writer.WriteElementString("PaymentMethodAdditionalFeeTaxRate", ((decimal)order.PaymentMethodAdditionalFeeTaxRate).ToString(invariantCulture));
							writer.WriteElementString("TaxRates", (string)order.TaxRates);
							writer.WriteElementString("OrderTax", ((decimal)order.OrderTax).ToString(invariantCulture));
							writer.WriteElementString("OrderDiscount", ((decimal)order.OrderDiscount).ToString(invariantCulture));
							writer.WriteElementString("OrderTotal", ((decimal)order.OrderTotal).ToString(invariantCulture));
							writer.WriteElementString("RefundedAmount", ((decimal)order.RefundedAmount).ToString(invariantCulture));
							writer.WriteElementString("RewardPointsWereAdded", ((bool)order.RewardPointsWereAdded).ToString());
							writer.WriteElementString("CheckoutAttributeDescription", ((string)order.CheckoutAttributeDescription).RemoveInvalidXmlChars());
							writer.WriteElementString("CheckoutAttributesXml", ((string)order.CheckoutAttributesXml).RemoveInvalidXmlChars());
							writer.WriteElementString("CustomerLanguageId", ((int)order.CustomerLanguageId).ToString());
							writer.WriteElementString("AffiliateId", ((int)order.AffiliateId).ToString());
							writer.WriteElementString("CustomerIp", (string)order.CustomerIp);
							writer.WriteElementString("AllowStoringCreditCardNumber", ((bool)order.AllowStoringCreditCardNumber).ToString());
							writer.WriteElementString("CardType", (string)order.CardType);
							writer.WriteElementString("CardName", (string)order.CardName);
							writer.WriteElementString("CardNumber", (string)order.CardNumber);
							writer.WriteElementString("MaskedCreditCardNumber", (string)order.MaskedCreditCardNumber);
							writer.WriteElementString("CardCvv2", (string)order.CardCvv2);
							writer.WriteElementString("CardExpirationMonth", (string)order.CardExpirationMonth);
							writer.WriteElementString("CardExpirationYear", (string)order.CardExpirationYear);
							writer.WriteElementString("AllowStoringDirectDebit", ((bool)order.AllowStoringDirectDebit).ToString());
							writer.WriteElementString("DirectDebitAccountHolder", (string)order.DirectDebitAccountHolder);
							writer.WriteElementString("DirectDebitAccountNumber", (string)order.DirectDebitAccountNumber);
							writer.WriteElementString("DirectDebitBankCode", (string)order.DirectDebitBankCode);
							writer.WriteElementString("DirectDebitBankName", (string)order.DirectDebitBankName);
							writer.WriteElementString("DirectDebitBIC", (string)order.DirectDebitBIC);
							writer.WriteElementString("DirectDebitCountry", (string)order.DirectDebitCountry);
							writer.WriteElementString("DirectDebitIban", (string)order.DirectDebitIban);
							writer.WriteElementString("CustomerOrderComment", ((string)order.CustomerOrderComment).RemoveInvalidXmlChars());
							writer.WriteElementString("AuthorizationTransactionId", (string)order.AuthorizationTransactionId);
							writer.WriteElementString("AuthorizationTransactionCode", (string)order.AuthorizationTransactionCode);
							writer.WriteElementString("AuthorizationTransactionResult", (string)order.AuthorizationTransactionResult);
							writer.WriteElementString("CaptureTransactionId", (string)order.CaptureTransactionId);
							writer.WriteElementString("CaptureTransactionResult", (string)order.CaptureTransactionResult);
							writer.WriteElementString("SubscriptionTransactionId", (string)order.SubscriptionTransactionId);
							writer.WriteElementString("PurchaseOrderNumber", (string)order.PurchaseOrderNumber);
							writer.WriteElementString("PaidDateUtc", paidDateUtc.HasValue ? paidDateUtc.Value.ToString(invariantCulture) : "");
							writer.WriteElementString("ShippingMethod", (string)order.ShippingMethod);
							writer.WriteElementString("ShippingRateComputationMethodSystemName", (string)order.ShippingRateComputationMethodSystemName);
							writer.WriteElementString("Deleted", ((bool)order.Deleted).ToString());
							writer.WriteElementString("CreatedOnUtc", ((DateTime)order.CreatedOnUtc).ToString(invariantCulture));
							writer.WriteElementString("UpdatedOnUtc", ((DateTime)order.UpdatedOnUtc).ToString(invariantCulture));
							writer.WriteElementString("RewardPointsRemaining", rewardPointsRemaining.HasValue ? rewardPointsRemaining.Value.ToString() : "");
							writer.WriteElementString("HasNewPaymentNotification", ((bool)order.HasNewPaymentNotification).ToString());
							writer.WriteElementString("OrderStatus", (string)order.OrderStatus);
							writer.WriteElementString("PaymentStatus", (string)order.PaymentStatus);
							writer.WriteElementString("ShippingStatus", (string)order.ShippingStatus);

							if (customer != null)
							{
								DateTime? lastLoginDateUtc = customer.LastLoginDateUtc;

								writer.WriteStartElement("Customer");
								writer.WriteElementString("Id", ((int)customer.Id).ToString());
								writer.WriteElementString("CustomerGuid", ((Guid)customer.CustomerGuid).ToString());
								writer.WriteElementString("Username", (string)customer.Username);
								writer.WriteElementString("Email", (string)customer.Email);
								writer.WriteElementString("PasswordFormatId", ((int)customer.PasswordFormatId).ToString());
								writer.WriteElementString("AdminComment", ((string)customer.AdminComment).RemoveInvalidXmlChars());
								writer.WriteElementString("IsTaxExempt", ((bool)customer.IsTaxExempt).ToString());
								writer.WriteElementString("AffiliateId", ((int)customer.AffiliateId).ToString());
								writer.WriteElementString("Active", ((bool)customer.Active).ToString());
								writer.WriteElementString("Deleted", ((bool)customer.Deleted).ToString());
								writer.WriteElementString("IsSystemAccount", ((bool)customer.IsSystemAccount).ToString());
								writer.WriteElementString("SystemName", (string)customer.SystemName);
								writer.WriteElementString("LastIpAddress", (string)customer.LastIpAddress);
								writer.WriteElementString("CreatedOnUtc", ((DateTime)customer.CreatedOnUtc).ToString(invariantCulture));
								writer.WriteElementString("LastLoginDateUtc", lastLoginDateUtc.HasValue ? lastLoginDateUtc.Value.ToString(invariantCulture) : "");
								writer.WriteElementString("LastActivityDateUtc", ((DateTime)customer.LastActivityDateUtc).ToString(invariantCulture));

								WriteAddress(writer, "BillingAddress", customer.BillingAddress, invariantCulture);
								WriteAddress(writer, "ShippingAddress", customer.ShippingAddress, invariantCulture);
								writer.WriteEndElement();	// Customer
							}

							WriteAddress(writer, "BillingAddress", order.BillingAddress, invariantCulture);
							WriteAddress(writer, "ShippingAddress", order.ShippingAddress, invariantCulture);

							if (store != null)
							{
								writer.WriteStartElement("Store");
								writer.WriteElementString("Id", ((int)store.Id).ToString());
								writer.WriteElementString("Name", (string)store.Name);
								writer.WriteElementString("Url", (string)store.Url);
								writer.WriteElementString("SslEnabled", ((bool)store.SslEnabled).ToString());
								writer.WriteElementString("SecureUrl", (string)store.SecureUrl);
								writer.WriteElementString("Hosts", (string)store.Hosts);
								writer.WriteElementString("LogoPictureId", ((int)store.LogoPictureId).ToString());
								writer.WriteElementString("DisplayOrder", ((int)store.DisplayOrder).ToString());
								writer.WriteElementString("HtmlBodyId", (string)store.HtmlBodyId);
								writer.WriteElementString("ContentDeliveryNetwork", (string)store.ContentDeliveryNetwork);
								writer.WriteElementString("PrimaryStoreCurrencyId", ((int)store.PrimaryStoreCurrencyId).ToString());
								writer.WriteElementString("PrimaryExchangeRateCurrencyId", ((int)store.PrimaryExchangeRateCurrencyId).ToString());

								WriteCurrency(writer, "PrimaryStoreCurrency", store.PrimaryStoreCurrency, invariantCulture);
								WriteCurrency(writer, "PrimaryExchangeRateCurrency", store.PrimaryExchangeRateCurrency, invariantCulture);
								writer.WriteEndElement();	// Store
							}

							writer.WriteStartElement("OrderItems");
							foreach (dynamic orderItem in order.OrderItems)
							{
								int? licenseDownloadId = orderItem.LicenseDownloadId;
								decimal? itemWeight = orderItem.ItemWeight;

								writer.WriteStartElement("OrderItem");								
								writer.WriteElementString("Id", ((int)orderItem.Id).ToString());
								writer.WriteElementString("OrderItemGuid", ((Guid)orderItem.OrderItemGuid).ToString());
								writer.WriteElementString("OrderId", ((int)orderItem.OrderId).ToString());
								writer.WriteElementString("ProductId", ((int)orderItem.ProductId).ToString());
								writer.WriteElementString("Quantity", ((int)orderItem.Quantity).ToString());
								writer.WriteElementString("UnitPriceInclTax", ((decimal)orderItem.UnitPriceInclTax).ToString(invariantCulture));
								writer.WriteElementString("UnitPriceExclTax", ((decimal)orderItem.UnitPriceExclTax).ToString(invariantCulture));
								writer.WriteElementString("PriceInclTax", ((decimal)orderItem.PriceInclTax).ToString(invariantCulture));
								writer.WriteElementString("PriceExclTax", ((decimal)orderItem.PriceExclTax).ToString(invariantCulture));
								writer.WriteElementString("TaxRate", ((decimal)orderItem.TaxRate).ToString(invariantCulture));
								writer.WriteElementString("DiscountAmountInclTax", ((decimal)orderItem.DiscountAmountInclTax).ToString(invariantCulture));
								writer.WriteElementString("DiscountAmountExclTax", ((decimal)orderItem.DiscountAmountExclTax).ToString(invariantCulture));
								writer.WriteElementString("AttributeDescription", ((string)orderItem.AttributeDescription).RemoveInvalidXmlChars());
								writer.WriteElementString("AttributesXml", ((string)orderItem.AttributesXml).RemoveInvalidXmlChars());
								writer.WriteElementString("DownloadCount", ((int)orderItem.DownloadCount).ToString());
								writer.WriteElementString("IsDownloadActivated", ((bool)orderItem.IsDownloadActivated).ToString());
								writer.WriteElementString("LicenseDownloadId", licenseDownloadId.HasValue ? licenseDownloadId.Value.ToString() : "");
								writer.WriteElementString("ItemWeight", itemWeight.HasValue ? itemWeight.Value.ToString(invariantCulture) : "");
								writer.WriteElementString("BundleData", ((string)orderItem.BundleData).RemoveInvalidXmlChars());
								writer.WriteElementString("ProductCost", ((decimal)orderItem.ProductCost).ToString(invariantCulture));

								WriteProduct(writer, "Product", orderItem.Product, invariantCulture);
								writer.WriteEndElement();	// OrderItem
							}
							writer.WriteEndElement();	// OrderItems

							writer.WriteStartElement("Shipments");
							foreach (dynamic shipment in order.Shipments)
							{
								decimal? totalWeight = shipment.TotalWeight;
								DateTime? shippedDateUtc = shipment.ShippedDateUtc;
								DateTime? deliveryDateUtc = shipment.DeliveryDateUtc;

								writer.WriteStartElement("Shipment");
								writer.WriteElementString("Id", ((int)shipment.Id).ToString());
								writer.WriteElementString("OrderId", ((int)shipment.OrderId).ToString());
								writer.WriteElementString("TrackingNumber", (string)shipment.TrackingNumber);
								writer.WriteElementString("TotalWeight", totalWeight.HasValue ? totalWeight.Value.ToString(invariantCulture) : "");
								writer.WriteElementString("ShippedDateUtc", shippedDateUtc.HasValue ? shippedDateUtc.Value.ToString(invariantCulture) : "");
								writer.WriteElementString("DeliveryDateUtc", deliveryDateUtc.HasValue ? deliveryDateUtc.Value.ToString(invariantCulture) : "");
								writer.WriteElementString("CreatedOnUtc", ((DateTime)shipment.CreatedOnUtc).ToString(invariantCulture));

								writer.WriteStartElement("ShipmentItems");
								foreach (dynamic shipmentItem in shipment.ShipmentItems)
								{
									writer.WriteStartElement("ShipmentItem");
									writer.WriteElementString("Id", ((int)shipmentItem.Id).ToString());
									writer.WriteElementString("ShipmentId", ((int)shipmentItem.ShipmentId).ToString());
									writer.WriteElementString("OrderItemId", ((int)shipmentItem.OrderItemId).ToString());
									writer.WriteElementString("Quantity", ((int)shipmentItem.Quantity).ToString());
									writer.WriteEndElement();	// ShipmentItem
								}
								writer.WriteEndElement();	// ShipmentItems

								writer.WriteEndElement();	// Shipment
							}
							writer.WriteEndElement();	// Shipments

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.Log.Error("Error while processing order with id {0}: {1}".FormatInvariant(orderId, exc.ToAllMessages()), exc);
							++context.RecordsFailed;
						}

						writer.WriteEndElement();	// Order
					}
				}

				writer.WriteEndElement();	// Orders
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
