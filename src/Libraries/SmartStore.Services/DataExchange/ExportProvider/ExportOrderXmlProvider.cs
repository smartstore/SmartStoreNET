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
	/// <summary>
	/// Exports XML formatted order data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreOrderXml")]
	[FriendlyName("SmartStore XML order export")]
	[IsHidden(true)]
	public class ExportOrderXmlProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreOrderXml"; }
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
				var xmlHelper = new ExportXmlHelper(writer, invariantCulture);

				writer.WriteStartDocument();
				writer.WriteStartElement("Orders");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic order in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						writer.WriteStartElement("Order");

						try
						{
							dynamic store = order.Store;
							int? shippingAddressId = order.ShippingAddressId;
							DateTime? paidDateUtc = order.PaidDateUtc;
							int? rewardPointsRemaining = order.RewardPointsRemaining;

							writer.Write("Id", ((int)order.Id).ToString());
							writer.Write("OrderNumber", (string)order.OrderNumber);
							writer.Write("OrderGuid", ((Guid)order.OrderGuid).ToString());
							writer.Write("StoreId", ((int)order.StoreId).ToString());
							writer.Write("CustomerId", ((int)order.CustomerId).ToString());
							writer.Write("BillingAddressId", ((int)order.BillingAddressId).ToString());
							writer.Write("ShippingAddressId", shippingAddressId.HasValue ? shippingAddressId.Value.ToString() : "");
							writer.Write("OrderStatusId", ((int)order.OrderStatusId).ToString());
							writer.Write("ShippingStatusId", ((int)order.ShippingStatusId).ToString());
							writer.Write("PaymentStatusId", ((int)order.PaymentStatusId).ToString());
							writer.Write("PaymentMethodSystemName", (string)order.PaymentMethodSystemName);
							writer.Write("CustomerCurrencyCode", (string)order.CustomerCurrencyCode);
							writer.Write("CurrencyRate", ((decimal)order.CurrencyRate).ToString(invariantCulture));
							writer.Write("CustomerTaxDisplayTypeId", ((int)order.CustomerTaxDisplayTypeId).ToString());
							writer.Write("VatNumber", (string)order.VatNumber);
							writer.Write("OrderSubtotalInclTax", ((decimal)order.OrderSubtotalInclTax).ToString(invariantCulture));
							writer.Write("OrderSubtotalExclTax", ((decimal)order.OrderSubtotalExclTax).ToString(invariantCulture));
							writer.Write("OrderSubTotalDiscountInclTax", ((decimal)order.OrderSubTotalDiscountInclTax).ToString(invariantCulture));
							writer.Write("OrderSubTotalDiscountExclTax", ((decimal)order.OrderSubTotalDiscountExclTax).ToString(invariantCulture));
							writer.Write("OrderShippingInclTax", ((decimal)order.OrderShippingInclTax).ToString(invariantCulture));
							writer.Write("OrderShippingExclTax", ((decimal)order.OrderShippingExclTax).ToString(invariantCulture));
							writer.Write("OrderShippingTaxRate", ((decimal)order.OrderShippingTaxRate).ToString(invariantCulture));
							writer.Write("PaymentMethodAdditionalFeeInclTax", ((decimal)order.PaymentMethodAdditionalFeeInclTax).ToString(invariantCulture));
							writer.Write("PaymentMethodAdditionalFeeExclTax", ((decimal)order.PaymentMethodAdditionalFeeExclTax).ToString(invariantCulture));
							writer.Write("PaymentMethodAdditionalFeeTaxRate", ((decimal)order.PaymentMethodAdditionalFeeTaxRate).ToString(invariantCulture));
							writer.Write("TaxRates", (string)order.TaxRates);
							writer.Write("OrderTax", ((decimal)order.OrderTax).ToString(invariantCulture));
							writer.Write("OrderDiscount", ((decimal)order.OrderDiscount).ToString(invariantCulture));
							writer.Write("OrderTotal", ((decimal)order.OrderTotal).ToString(invariantCulture));
							writer.Write("RefundedAmount", ((decimal)order.RefundedAmount).ToString(invariantCulture));
							writer.Write("RewardPointsWereAdded", ((bool)order.RewardPointsWereAdded).ToString());
							writer.Write("CheckoutAttributeDescription", (string)order.CheckoutAttributeDescription);
							writer.Write("CheckoutAttributesXml", (string)order.CheckoutAttributesXml);
							writer.Write("CustomerLanguageId", ((int)order.CustomerLanguageId).ToString());
							writer.Write("AffiliateId", ((int)order.AffiliateId).ToString());
							writer.Write("CustomerIp", (string)order.CustomerIp);
							writer.Write("AllowStoringCreditCardNumber", ((bool)order.AllowStoringCreditCardNumber).ToString());
							writer.Write("CardType", (string)order.CardType);
							writer.Write("CardName", (string)order.CardName);
							writer.Write("CardNumber", (string)order.CardNumber);
							writer.Write("MaskedCreditCardNumber", (string)order.MaskedCreditCardNumber);
							writer.Write("CardCvv2", (string)order.CardCvv2);
							writer.Write("CardExpirationMonth", (string)order.CardExpirationMonth);
							writer.Write("CardExpirationYear", (string)order.CardExpirationYear);
							writer.Write("AllowStoringDirectDebit", ((bool)order.AllowStoringDirectDebit).ToString());
							writer.Write("DirectDebitAccountHolder", (string)order.DirectDebitAccountHolder);
							writer.Write("DirectDebitAccountNumber", (string)order.DirectDebitAccountNumber);
							writer.Write("DirectDebitBankCode", (string)order.DirectDebitBankCode);
							writer.Write("DirectDebitBankName", (string)order.DirectDebitBankName);
							writer.Write("DirectDebitBIC", (string)order.DirectDebitBIC);
							writer.Write("DirectDebitCountry", (string)order.DirectDebitCountry);
							writer.Write("DirectDebitIban", (string)order.DirectDebitIban);
							writer.Write("CustomerOrderComment", (string)order.CustomerOrderComment);
							writer.Write("AuthorizationTransactionId", (string)order.AuthorizationTransactionId);
							writer.Write("AuthorizationTransactionCode", (string)order.AuthorizationTransactionCode);
							writer.Write("AuthorizationTransactionResult", (string)order.AuthorizationTransactionResult);
							writer.Write("CaptureTransactionId", (string)order.CaptureTransactionId);
							writer.Write("CaptureTransactionResult", (string)order.CaptureTransactionResult);
							writer.Write("SubscriptionTransactionId", (string)order.SubscriptionTransactionId);
							writer.Write("PurchaseOrderNumber", (string)order.PurchaseOrderNumber);
							writer.Write("PaidDateUtc", paidDateUtc.HasValue ? paidDateUtc.Value.ToString(invariantCulture) : "");
							writer.Write("ShippingMethod", (string)order.ShippingMethod);
							writer.Write("ShippingRateComputationMethodSystemName", (string)order.ShippingRateComputationMethodSystemName);
							writer.Write("Deleted", ((bool)order.Deleted).ToString());
							writer.Write("CreatedOnUtc", ((DateTime)order.CreatedOnUtc).ToString(invariantCulture));
							writer.Write("UpdatedOnUtc", ((DateTime)order.UpdatedOnUtc).ToString(invariantCulture));
							writer.Write("RewardPointsRemaining", rewardPointsRemaining.HasValue ? rewardPointsRemaining.Value.ToString() : "");
							writer.Write("HasNewPaymentNotification", ((bool)order.HasNewPaymentNotification).ToString());
							writer.Write("OrderStatus", (string)order.OrderStatus);
							writer.Write("PaymentStatus", (string)order.PaymentStatus);
							writer.Write("ShippingStatus", (string)order.ShippingStatus);

							xmlHelper.WriteCustomer(order.Customer, "Customer");

							xmlHelper.WriteAddress(order.BillingAddress, "BillingAddress");
							xmlHelper.WriteAddress(order.ShippingAddress, "ShippingAddress");

							if (store != null)
							{
								writer.WriteStartElement("Store");
								writer.Write("Id", ((int)store.Id).ToString());
								writer.Write("Name", (string)store.Name);
								writer.Write("Url", (string)store.Url);
								writer.Write("SslEnabled", ((bool)store.SslEnabled).ToString());
								writer.Write("SecureUrl", (string)store.SecureUrl);
								writer.Write("Hosts", (string)store.Hosts);
								writer.Write("LogoPictureId", ((int)store.LogoPictureId).ToString());
								writer.Write("DisplayOrder", ((int)store.DisplayOrder).ToString());
								writer.Write("HtmlBodyId", (string)store.HtmlBodyId);
								writer.Write("ContentDeliveryNetwork", (string)store.ContentDeliveryNetwork);
								writer.Write("PrimaryStoreCurrencyId", ((int)store.PrimaryStoreCurrencyId).ToString());
								writer.Write("PrimaryExchangeRateCurrencyId", ((int)store.PrimaryExchangeRateCurrencyId).ToString());

								xmlHelper.WriteCurrency(store.PrimaryStoreCurrency, "PrimaryStoreCurrency");
								xmlHelper.WriteCurrency(store.PrimaryExchangeRateCurrency, "PrimaryExchangeRateCurrency");

								writer.WriteEndElement();	// Store
							}

							writer.WriteStartElement("OrderItems");
							foreach (dynamic orderItem in order.OrderItems)
							{
								int? licenseDownloadId = orderItem.LicenseDownloadId;
								decimal? itemWeight = orderItem.ItemWeight;

								writer.WriteStartElement("OrderItem");
								writer.Write("Id", ((int)orderItem.Id).ToString());
								writer.Write("OrderItemGuid", ((Guid)orderItem.OrderItemGuid).ToString());
								writer.Write("OrderId", ((int)orderItem.OrderId).ToString());
								writer.Write("ProductId", ((int)orderItem.ProductId).ToString());
								writer.Write("Quantity", ((int)orderItem.Quantity).ToString());
								writer.Write("UnitPriceInclTax", ((decimal)orderItem.UnitPriceInclTax).ToString(invariantCulture));
								writer.Write("UnitPriceExclTax", ((decimal)orderItem.UnitPriceExclTax).ToString(invariantCulture));
								writer.Write("PriceInclTax", ((decimal)orderItem.PriceInclTax).ToString(invariantCulture));
								writer.Write("PriceExclTax", ((decimal)orderItem.PriceExclTax).ToString(invariantCulture));
								writer.Write("TaxRate", ((decimal)orderItem.TaxRate).ToString(invariantCulture));
								writer.Write("DiscountAmountInclTax", ((decimal)orderItem.DiscountAmountInclTax).ToString(invariantCulture));
								writer.Write("DiscountAmountExclTax", ((decimal)orderItem.DiscountAmountExclTax).ToString(invariantCulture));
								writer.Write("AttributeDescription", (string)orderItem.AttributeDescription);
								writer.Write("AttributesXml", (string)orderItem.AttributesXml);
								writer.Write("DownloadCount", ((int)orderItem.DownloadCount).ToString());
								writer.Write("IsDownloadActivated", ((bool)orderItem.IsDownloadActivated).ToString());
								writer.Write("LicenseDownloadId", licenseDownloadId.HasValue ? licenseDownloadId.Value.ToString() : "");
								writer.Write("ItemWeight", itemWeight.HasValue ? itemWeight.Value.ToString(invariantCulture) : "");
								writer.Write("BundleData", (string)orderItem.BundleData);
								writer.Write("ProductCost", ((decimal)orderItem.ProductCost).ToString(invariantCulture));

								xmlHelper.WriteProduct(orderItem.Product, "Product");

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
								writer.Write("Id", ((int)shipment.Id).ToString());
								writer.Write("OrderId", ((int)shipment.OrderId).ToString());
								writer.Write("TrackingNumber", (string)shipment.TrackingNumber);
								writer.Write("TotalWeight", totalWeight.HasValue ? totalWeight.Value.ToString(invariantCulture) : "");
								writer.Write("ShippedDateUtc", shippedDateUtc.HasValue ? shippedDateUtc.Value.ToString(invariantCulture) : "");
								writer.Write("DeliveryDateUtc", deliveryDateUtc.HasValue ? deliveryDateUtc.Value.ToString(invariantCulture) : "");
								writer.Write("CreatedOnUtc", ((DateTime)shipment.CreatedOnUtc).ToString(invariantCulture));

								writer.WriteStartElement("ShipmentItems");
								foreach (dynamic shipmentItem in shipment.ShipmentItems)
								{
									writer.WriteStartElement("ShipmentItem");
									writer.Write("Id", ((int)shipmentItem.Id).ToString());
									writer.Write("ShipmentId", ((int)shipmentItem.ShipmentId).ToString());
									writer.Write("OrderItemId", ((int)shipmentItem.OrderItemId).ToString());
									writer.Write("Quantity", ((int)shipmentItem.Quantity).ToString());
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
							context.RecordException(exc, (int)order.Id);
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
