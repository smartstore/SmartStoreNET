using System;
using System.Globalization;
using System.IO;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports XML formatted order data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreOrderXml")]
	[FriendlyName("SmartStore XML order export")]
	[IsHidden(true)]
	public class OrderXmlExportProvider : ExportProviderBase
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreOrderXml"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Order; }
		}

		public override string FileExtension
		{
			get { return "XML"; }
		}

		public override void Execute(IExportExecuteContext context)
		{
			var path = context.FilePath;
			var invariantCulture = CultureInfo.InvariantCulture;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var helper = new ExportXmlHelper(stream))
			{
				helper.Writer.WriteStartDocument();
				helper.Writer.WriteStartElement("Orders");
				helper.Writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic order in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						helper.Writer.WriteStartElement("Order");

						try
						{
							dynamic store = order.Store;
							int? shippingAddressId = order.ShippingAddressId;
							DateTime? paidDateUtc = order.PaidDateUtc;
							int? rewardPointsRemaining = order.RewardPointsRemaining;

							helper.Writer.Write("Id", ((int)order.Id).ToString());
							helper.Writer.Write("OrderNumber", (string)order.OrderNumber);
							helper.Writer.Write("OrderGuid", ((Guid)order.OrderGuid).ToString());
							helper.Writer.Write("StoreId", ((int)order.StoreId).ToString());
							helper.Writer.Write("CustomerId", ((int)order.CustomerId).ToString());
							helper.Writer.Write("BillingAddressId", ((int)order.BillingAddressId).ToString());
							helper.Writer.Write("ShippingAddressId", shippingAddressId.HasValue ? shippingAddressId.Value.ToString() : "");
							helper.Writer.Write("OrderStatusId", ((int)order.OrderStatusId).ToString());
							helper.Writer.Write("ShippingStatusId", ((int)order.ShippingStatusId).ToString());
							helper.Writer.Write("PaymentStatusId", ((int)order.PaymentStatusId).ToString());
							helper.Writer.Write("PaymentMethodSystemName", (string)order.PaymentMethodSystemName);
							helper.Writer.Write("CustomerCurrencyCode", (string)order.CustomerCurrencyCode);
							helper.Writer.Write("CurrencyRate", ((decimal)order.CurrencyRate).ToString(invariantCulture));
							helper.Writer.Write("CustomerTaxDisplayTypeId", ((int)order.CustomerTaxDisplayTypeId).ToString());
							helper.Writer.Write("VatNumber", (string)order.VatNumber);
							helper.Writer.Write("OrderSubtotalInclTax", ((decimal)order.OrderSubtotalInclTax).ToString(invariantCulture));
							helper.Writer.Write("OrderSubtotalExclTax", ((decimal)order.OrderSubtotalExclTax).ToString(invariantCulture));
							helper.Writer.Write("OrderSubTotalDiscountInclTax", ((decimal)order.OrderSubTotalDiscountInclTax).ToString(invariantCulture));
							helper.Writer.Write("OrderSubTotalDiscountExclTax", ((decimal)order.OrderSubTotalDiscountExclTax).ToString(invariantCulture));
							helper.Writer.Write("OrderShippingInclTax", ((decimal)order.OrderShippingInclTax).ToString(invariantCulture));
							helper.Writer.Write("OrderShippingExclTax", ((decimal)order.OrderShippingExclTax).ToString(invariantCulture));
							helper.Writer.Write("OrderShippingTaxRate", ((decimal)order.OrderShippingTaxRate).ToString(invariantCulture));
							helper.Writer.Write("PaymentMethodAdditionalFeeInclTax", ((decimal)order.PaymentMethodAdditionalFeeInclTax).ToString(invariantCulture));
							helper.Writer.Write("PaymentMethodAdditionalFeeExclTax", ((decimal)order.PaymentMethodAdditionalFeeExclTax).ToString(invariantCulture));
							helper.Writer.Write("PaymentMethodAdditionalFeeTaxRate", ((decimal)order.PaymentMethodAdditionalFeeTaxRate).ToString(invariantCulture));
							helper.Writer.Write("TaxRates", (string)order.TaxRates);
							helper.Writer.Write("OrderTax", ((decimal)order.OrderTax).ToString(invariantCulture));
							helper.Writer.Write("OrderDiscount", ((decimal)order.OrderDiscount).ToString(invariantCulture));
							helper.Writer.Write("OrderTotal", ((decimal)order.OrderTotal).ToString(invariantCulture));
							helper.Writer.Write("RefundedAmount", ((decimal)order.RefundedAmount).ToString(invariantCulture));
							helper.Writer.Write("RewardPointsWereAdded", ((bool)order.RewardPointsWereAdded).ToString());
							helper.Writer.Write("CheckoutAttributeDescription", (string)order.CheckoutAttributeDescription);
							helper.Writer.Write("CheckoutAttributesXml", (string)order.CheckoutAttributesXml);
							helper.Writer.Write("CustomerLanguageId", ((int)order.CustomerLanguageId).ToString());
							helper.Writer.Write("AffiliateId", ((int)order.AffiliateId).ToString());
							helper.Writer.Write("CustomerIp", (string)order.CustomerIp);
							helper.Writer.Write("AllowStoringCreditCardNumber", ((bool)order.AllowStoringCreditCardNumber).ToString());
							helper.Writer.Write("CardType", (string)order.CardType);
							helper.Writer.Write("CardName", (string)order.CardName);
							helper.Writer.Write("CardNumber", (string)order.CardNumber);
							helper.Writer.Write("MaskedCreditCardNumber", (string)order.MaskedCreditCardNumber);
							helper.Writer.Write("CardCvv2", (string)order.CardCvv2);
							helper.Writer.Write("CardExpirationMonth", (string)order.CardExpirationMonth);
							helper.Writer.Write("CardExpirationYear", (string)order.CardExpirationYear);
							helper.Writer.Write("AllowStoringDirectDebit", ((bool)order.AllowStoringDirectDebit).ToString());
							helper.Writer.Write("DirectDebitAccountHolder", (string)order.DirectDebitAccountHolder);
							helper.Writer.Write("DirectDebitAccountNumber", (string)order.DirectDebitAccountNumber);
							helper.Writer.Write("DirectDebitBankCode", (string)order.DirectDebitBankCode);
							helper.Writer.Write("DirectDebitBankName", (string)order.DirectDebitBankName);
							helper.Writer.Write("DirectDebitBIC", (string)order.DirectDebitBIC);
							helper.Writer.Write("DirectDebitCountry", (string)order.DirectDebitCountry);
							helper.Writer.Write("DirectDebitIban", (string)order.DirectDebitIban);
							helper.Writer.Write("CustomerOrderComment", (string)order.CustomerOrderComment);
							helper.Writer.Write("AuthorizationTransactionId", (string)order.AuthorizationTransactionId);
							helper.Writer.Write("AuthorizationTransactionCode", (string)order.AuthorizationTransactionCode);
							helper.Writer.Write("AuthorizationTransactionResult", (string)order.AuthorizationTransactionResult);
							helper.Writer.Write("CaptureTransactionId", (string)order.CaptureTransactionId);
							helper.Writer.Write("CaptureTransactionResult", (string)order.CaptureTransactionResult);
							helper.Writer.Write("SubscriptionTransactionId", (string)order.SubscriptionTransactionId);
							helper.Writer.Write("PurchaseOrderNumber", (string)order.PurchaseOrderNumber);
							helper.Writer.Write("PaidDateUtc", paidDateUtc.HasValue ? paidDateUtc.Value.ToString(invariantCulture) : "");
							helper.Writer.Write("ShippingMethod", (string)order.ShippingMethod);
							helper.Writer.Write("ShippingRateComputationMethodSystemName", (string)order.ShippingRateComputationMethodSystemName);
							helper.Writer.Write("Deleted", ((bool)order.Deleted).ToString());
							helper.Writer.Write("CreatedOnUtc", ((DateTime)order.CreatedOnUtc).ToString(invariantCulture));
							helper.Writer.Write("UpdatedOnUtc", ((DateTime)order.UpdatedOnUtc).ToString(invariantCulture));
							helper.Writer.Write("RewardPointsRemaining", rewardPointsRemaining.HasValue ? rewardPointsRemaining.Value.ToString() : "");
							helper.Writer.Write("HasNewPaymentNotification", ((bool)order.HasNewPaymentNotification).ToString());
							helper.Writer.Write("OrderStatus", (string)order.OrderStatus);
							helper.Writer.Write("PaymentStatus", (string)order.PaymentStatus);
							helper.Writer.Write("ShippingStatus", (string)order.ShippingStatus);

							helper.WriteCustomer(order.Customer, "Customer");

							helper.WriteAddress(order.BillingAddress, "BillingAddress");
							helper.WriteAddress(order.ShippingAddress, "ShippingAddress");

							if (store != null)
							{
								helper.Writer.WriteStartElement("Store");
								helper.Writer.Write("Id", ((int)store.Id).ToString());
								helper.Writer.Write("Name", (string)store.Name);
								helper.Writer.Write("Url", (string)store.Url);
								helper.Writer.Write("SslEnabled", ((bool)store.SslEnabled).ToString());
								helper.Writer.Write("SecureUrl", (string)store.SecureUrl);
								helper.Writer.Write("Hosts", (string)store.Hosts);
								helper.Writer.Write("LogoPictureId", ((int)store.LogoPictureId).ToString());
								helper.Writer.Write("DisplayOrder", ((int)store.DisplayOrder).ToString());
								helper.Writer.Write("HtmlBodyId", (string)store.HtmlBodyId);
								helper.Writer.Write("ContentDeliveryNetwork", (string)store.ContentDeliveryNetwork);
								helper.Writer.Write("PrimaryStoreCurrencyId", ((int)store.PrimaryStoreCurrencyId).ToString());
								helper.Writer.Write("PrimaryExchangeRateCurrencyId", ((int)store.PrimaryExchangeRateCurrencyId).ToString());

								helper.WriteCurrency(store.PrimaryStoreCurrency, "PrimaryStoreCurrency");
								helper.WriteCurrency(store.PrimaryExchangeRateCurrency, "PrimaryExchangeRateCurrency");

								helper.Writer.WriteEndElement();	// Store
							}

							helper.Writer.WriteStartElement("OrderItems");
							foreach (dynamic orderItem in order.OrderItems)
							{
								int? licenseDownloadId = orderItem.LicenseDownloadId;
								decimal? itemWeight = orderItem.ItemWeight;

								helper.Writer.WriteStartElement("OrderItem");
								helper.Writer.Write("Id", ((int)orderItem.Id).ToString());
								helper.Writer.Write("OrderItemGuid", ((Guid)orderItem.OrderItemGuid).ToString());
								helper.Writer.Write("OrderId", ((int)orderItem.OrderId).ToString());
								helper.Writer.Write("ProductId", ((int)orderItem.ProductId).ToString());
								helper.Writer.Write("Quantity", ((int)orderItem.Quantity).ToString());
								helper.Writer.Write("UnitPriceInclTax", ((decimal)orderItem.UnitPriceInclTax).ToString(invariantCulture));
								helper.Writer.Write("UnitPriceExclTax", ((decimal)orderItem.UnitPriceExclTax).ToString(invariantCulture));
								helper.Writer.Write("PriceInclTax", ((decimal)orderItem.PriceInclTax).ToString(invariantCulture));
								helper.Writer.Write("PriceExclTax", ((decimal)orderItem.PriceExclTax).ToString(invariantCulture));
								helper.Writer.Write("TaxRate", ((decimal)orderItem.TaxRate).ToString(invariantCulture));
								helper.Writer.Write("DiscountAmountInclTax", ((decimal)orderItem.DiscountAmountInclTax).ToString(invariantCulture));
								helper.Writer.Write("DiscountAmountExclTax", ((decimal)orderItem.DiscountAmountExclTax).ToString(invariantCulture));
								helper.Writer.Write("AttributeDescription", (string)orderItem.AttributeDescription);
								helper.Writer.Write("AttributesXml", (string)orderItem.AttributesXml);
								helper.Writer.Write("DownloadCount", ((int)orderItem.DownloadCount).ToString());
								helper.Writer.Write("IsDownloadActivated", ((bool)orderItem.IsDownloadActivated).ToString());
								helper.Writer.Write("LicenseDownloadId", licenseDownloadId.HasValue ? licenseDownloadId.Value.ToString() : "");
								helper.Writer.Write("ItemWeight", itemWeight.HasValue ? itemWeight.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("BundleData", (string)orderItem.BundleData);
								helper.Writer.Write("ProductCost", ((decimal)orderItem.ProductCost).ToString(invariantCulture));

								helper.WriteProduct(orderItem.Product, "Product");

								helper.Writer.WriteEndElement();	// OrderItem
							}
							helper.Writer.WriteEndElement();	// OrderItems

							helper.Writer.WriteStartElement("Shipments");
							foreach (dynamic shipment in order.Shipments)
							{
								decimal? totalWeight = shipment.TotalWeight;
								DateTime? shippedDateUtc = shipment.ShippedDateUtc;
								DateTime? deliveryDateUtc = shipment.DeliveryDateUtc;

								helper.Writer.WriteStartElement("Shipment");
								helper.Writer.Write("Id", ((int)shipment.Id).ToString());
								helper.Writer.Write("OrderId", ((int)shipment.OrderId).ToString());
								helper.Writer.Write("TrackingNumber", (string)shipment.TrackingNumber);
								helper.Writer.Write("TotalWeight", totalWeight.HasValue ? totalWeight.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("ShippedDateUtc", shippedDateUtc.HasValue ? shippedDateUtc.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("DeliveryDateUtc", deliveryDateUtc.HasValue ? deliveryDateUtc.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("CreatedOnUtc", ((DateTime)shipment.CreatedOnUtc).ToString(invariantCulture));

								helper.Writer.WriteStartElement("ShipmentItems");
								foreach (dynamic shipmentItem in shipment.ShipmentItems)
								{
									helper.Writer.WriteStartElement("ShipmentItem");
									helper.Writer.Write("Id", ((int)shipmentItem.Id).ToString());
									helper.Writer.Write("ShipmentId", ((int)shipmentItem.ShipmentId).ToString());
									helper.Writer.Write("OrderItemId", ((int)shipmentItem.OrderItemId).ToString());
									helper.Writer.Write("Quantity", ((int)shipmentItem.Quantity).ToString());
									helper.Writer.WriteEndElement();	// ShipmentItem
								}
								helper.Writer.WriteEndElement();	// ShipmentItems

								helper.Writer.WriteEndElement();	// Shipment
							}
							helper.Writer.WriteEndElement();	// Shipments
						
							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)order.Id);
						}

						helper.Writer.WriteEndElement();	// Order
					}
				}

				helper.Writer.WriteEndElement();	// Orders
				helper.Writer.WriteEndDocument();
			}
		}
	}
}
