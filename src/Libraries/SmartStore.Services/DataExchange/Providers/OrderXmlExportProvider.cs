using System;
using System.Globalization;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
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

		protected override void Export(IExportExecuteContext context)
		{
			var invariantCulture = CultureInfo.InvariantCulture;

			using (var helper = new ExportXmlHelper(context.DataStream))
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

						Order entity = order.Entity;

						helper.Writer.WriteStartElement("Order");

						try
						{
							dynamic store = order.Store;

							helper.Writer.Write("Id", entity.Id.ToString());
							helper.Writer.Write("OrderNumber", (string)order.OrderNumber);
							helper.Writer.Write("OrderGuid", entity.OrderGuid.ToString());
							helper.Writer.Write("StoreId", entity.StoreId.ToString());
							helper.Writer.Write("CustomerId", entity.CustomerId.ToString());
							helper.Writer.Write("BillingAddressId", entity.BillingAddressId.ToString());
							helper.Writer.Write("ShippingAddressId", entity.ShippingAddressId.HasValue ? entity.ShippingAddressId.Value.ToString() : "");
							helper.Writer.Write("OrderStatusId", entity.OrderStatusId.ToString());
							helper.Writer.Write("ShippingStatusId", entity.ShippingStatusId.ToString());
							helper.Writer.Write("PaymentStatusId", entity.PaymentStatusId.ToString());
							helper.Writer.Write("PaymentMethodSystemName", entity.PaymentMethodSystemName);
							helper.Writer.Write("CustomerCurrencyCode", entity.CustomerCurrencyCode);
							helper.Writer.Write("CurrencyRate", entity.CurrencyRate.ToString(invariantCulture));
							helper.Writer.Write("CustomerTaxDisplayTypeId", entity.CustomerTaxDisplayTypeId.ToString());
							helper.Writer.Write("VatNumber", entity.VatNumber);
							helper.Writer.Write("OrderSubtotalInclTax", entity.OrderSubtotalInclTax.ToString(invariantCulture));
							helper.Writer.Write("OrderSubtotalExclTax", entity.OrderSubtotalExclTax.ToString(invariantCulture));
							helper.Writer.Write("OrderSubTotalDiscountInclTax", entity.OrderSubTotalDiscountInclTax.ToString(invariantCulture));
							helper.Writer.Write("OrderSubTotalDiscountExclTax", entity.OrderSubTotalDiscountExclTax.ToString(invariantCulture));
							helper.Writer.Write("OrderShippingInclTax", entity.OrderShippingInclTax.ToString(invariantCulture));
							helper.Writer.Write("OrderShippingExclTax", entity.OrderShippingExclTax.ToString(invariantCulture));
							helper.Writer.Write("OrderShippingTaxRate", entity.OrderShippingTaxRate.ToString(invariantCulture));
							helper.Writer.Write("PaymentMethodAdditionalFeeInclTax", entity.PaymentMethodAdditionalFeeInclTax.ToString(invariantCulture));
							helper.Writer.Write("PaymentMethodAdditionalFeeExclTax", entity.PaymentMethodAdditionalFeeExclTax.ToString(invariantCulture));
							helper.Writer.Write("PaymentMethodAdditionalFeeTaxRate", entity.PaymentMethodAdditionalFeeTaxRate.ToString(invariantCulture));
							helper.Writer.Write("TaxRates", entity.TaxRates);
							helper.Writer.Write("OrderTax", entity.OrderTax.ToString(invariantCulture));
							helper.Writer.Write("OrderDiscount", entity.OrderDiscount.ToString(invariantCulture));
							helper.Writer.Write("OrderTotal", entity.OrderTotal.ToString(invariantCulture));
							helper.Writer.Write("RefundedAmount", entity.RefundedAmount.ToString(invariantCulture));
							helper.Writer.Write("RewardPointsWereAdded", entity.RewardPointsWereAdded.ToString());
							helper.Writer.Write("CheckoutAttributeDescription", entity.CheckoutAttributeDescription);
							helper.Writer.Write("CheckoutAttributesXml", entity.CheckoutAttributesXml);
							helper.Writer.Write("CustomerLanguageId", entity.CustomerLanguageId.ToString());
							helper.Writer.Write("AffiliateId", entity.AffiliateId.ToString());
							helper.Writer.Write("CustomerIp", entity.CustomerIp);
							helper.Writer.Write("AllowStoringCreditCardNumber", entity.AllowStoringCreditCardNumber.ToString());
							helper.Writer.Write("CardType", entity.CardType);
							helper.Writer.Write("CardName", entity.CardName);
							helper.Writer.Write("CardNumber", entity.CardNumber);
							helper.Writer.Write("MaskedCreditCardNumber", entity.MaskedCreditCardNumber);
							helper.Writer.Write("CardCvv2", entity.CardCvv2);
							helper.Writer.Write("CardExpirationMonth", entity.CardExpirationMonth);
							helper.Writer.Write("CardExpirationYear", entity.CardExpirationYear);
							helper.Writer.Write("AllowStoringDirectDebit", entity.AllowStoringDirectDebit.ToString());
							helper.Writer.Write("DirectDebitAccountHolder", entity.DirectDebitAccountHolder);
							helper.Writer.Write("DirectDebitAccountNumber", entity.DirectDebitAccountNumber);
							helper.Writer.Write("DirectDebitBankCode", entity.DirectDebitBankCode);
							helper.Writer.Write("DirectDebitBankName", entity.DirectDebitBankName);
							helper.Writer.Write("DirectDebitBIC", entity.DirectDebitBIC);
							helper.Writer.Write("DirectDebitCountry", entity.DirectDebitCountry);
							helper.Writer.Write("DirectDebitIban", entity.DirectDebitIban);
							helper.Writer.Write("CustomerOrderComment", entity.CustomerOrderComment);
							helper.Writer.Write("AuthorizationTransactionId", entity.AuthorizationTransactionId);
							helper.Writer.Write("AuthorizationTransactionCode", entity.AuthorizationTransactionCode);
							helper.Writer.Write("AuthorizationTransactionResult", entity.AuthorizationTransactionResult);
							helper.Writer.Write("CaptureTransactionId", entity.CaptureTransactionId);
							helper.Writer.Write("CaptureTransactionResult", entity.CaptureTransactionResult);
							helper.Writer.Write("SubscriptionTransactionId", entity.SubscriptionTransactionId);
							helper.Writer.Write("PurchaseOrderNumber", entity.PurchaseOrderNumber);
							helper.Writer.Write("PaidDateUtc", entity.PaidDateUtc.HasValue ? entity.PaidDateUtc.Value.ToString(invariantCulture) : "");
							helper.Writer.Write("ShippingMethod", entity.ShippingMethod);
							helper.Writer.Write("ShippingRateComputationMethodSystemName", entity.ShippingRateComputationMethodSystemName);
							helper.Writer.Write("Deleted", entity.Deleted.ToString());
							helper.Writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(invariantCulture));
							helper.Writer.Write("UpdatedOnUtc", entity.UpdatedOnUtc.ToString(invariantCulture));
							helper.Writer.Write("RewardPointsRemaining", entity.RewardPointsRemaining.HasValue ? entity.RewardPointsRemaining.Value.ToString() : "");
							helper.Writer.Write("HasNewPaymentNotification", entity.HasNewPaymentNotification.ToString());
							helper.Writer.Write("OrderStatus", (string)order.OrderStatus);
							helper.Writer.Write("PaymentStatus", (string)order.PaymentStatus);
							helper.Writer.Write("ShippingStatus", (string)order.ShippingStatus);

							helper.WriteCustomer(order.Customer, "Customer");

							helper.WriteAddress(order.BillingAddress, "BillingAddress");
							helper.WriteAddress(order.ShippingAddress, "ShippingAddress");

							if (store != null)
							{
								Store entityStore = store.Entity;

								helper.Writer.WriteStartElement("Store");
								helper.Writer.Write("Id", entityStore.Id.ToString());
								helper.Writer.Write("Name", entityStore.Name);
								helper.Writer.Write("Url", entityStore.Url);
								helper.Writer.Write("SslEnabled", entityStore.SslEnabled.ToString());
								helper.Writer.Write("SecureUrl", entityStore.SecureUrl);
								helper.Writer.Write("Hosts", entityStore.Hosts);
								helper.Writer.Write("LogoPictureId", entityStore.LogoPictureId.ToString());
								helper.Writer.Write("DisplayOrder", entityStore.DisplayOrder.ToString());
								helper.Writer.Write("HtmlBodyId", entityStore.HtmlBodyId);
								helper.Writer.Write("ContentDeliveryNetwork", entityStore.ContentDeliveryNetwork);
								helper.Writer.Write("PrimaryStoreCurrencyId", entityStore.PrimaryStoreCurrencyId.ToString());
								helper.Writer.Write("PrimaryExchangeRateCurrencyId", entityStore.PrimaryExchangeRateCurrencyId.ToString());

								helper.WriteCurrency(store.PrimaryStoreCurrency, "PrimaryStoreCurrency");
								helper.WriteCurrency(store.PrimaryExchangeRateCurrency, "PrimaryExchangeRateCurrency");

								helper.Writer.WriteEndElement();	// Store
							}

							helper.Writer.WriteStartElement("OrderItems");
							foreach (dynamic orderItem in order.OrderItems)
							{
								OrderItem entityOrderItem = orderItem.Entity;

								helper.Writer.WriteStartElement("OrderItem");
								helper.Writer.Write("Id", entityOrderItem.Id.ToString());
								helper.Writer.Write("OrderItemGuid", entityOrderItem.OrderItemGuid.ToString());
								helper.Writer.Write("OrderId", entityOrderItem.OrderId.ToString());
								helper.Writer.Write("ProductId", entityOrderItem.ProductId.ToString());
								helper.Writer.Write("Quantity", entityOrderItem.Quantity.ToString());
								helper.Writer.Write("UnitPriceInclTax", entityOrderItem.UnitPriceInclTax.ToString(invariantCulture));
								helper.Writer.Write("UnitPriceExclTax", entityOrderItem.UnitPriceExclTax.ToString(invariantCulture));
								helper.Writer.Write("PriceInclTax", entityOrderItem.PriceInclTax.ToString(invariantCulture));
								helper.Writer.Write("PriceExclTax", entityOrderItem.PriceExclTax.ToString(invariantCulture));
								helper.Writer.Write("TaxRate", entityOrderItem.TaxRate.ToString(invariantCulture));
								helper.Writer.Write("DiscountAmountInclTax", entityOrderItem.DiscountAmountInclTax.ToString(invariantCulture));
								helper.Writer.Write("DiscountAmountExclTax", entityOrderItem.DiscountAmountExclTax.ToString(invariantCulture));
								helper.Writer.Write("AttributeDescription", entityOrderItem.AttributeDescription);
								helper.Writer.Write("AttributesXml", entityOrderItem.AttributesXml);
								helper.Writer.Write("DownloadCount", entityOrderItem.DownloadCount.ToString());
								helper.Writer.Write("IsDownloadActivated", entityOrderItem.IsDownloadActivated.ToString());
								helper.Writer.Write("LicenseDownloadId", entityOrderItem.LicenseDownloadId.HasValue ? entityOrderItem.LicenseDownloadId.Value.ToString() : "");
								helper.Writer.Write("ItemWeight", entityOrderItem.ItemWeight.HasValue ? entityOrderItem.ItemWeight.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("BundleData", entityOrderItem.BundleData);
								helper.Writer.Write("ProductCost", entityOrderItem.ProductCost.ToString(invariantCulture));

								helper.WriteProduct(orderItem.Product, "Product");

								helper.Writer.WriteEndElement();	// OrderItem
							}
							helper.Writer.WriteEndElement();	// OrderItems

							helper.Writer.WriteStartElement("Shipments");
							foreach (dynamic shipment in order.Shipments)
							{
								Shipment entityShipment = shipment.Entity;

								helper.Writer.WriteStartElement("Shipment");
								helper.Writer.Write("Id", entityShipment.Id.ToString());
								helper.Writer.Write("OrderId", entityShipment.OrderId.ToString());
								helper.Writer.Write("TrackingNumber", entityShipment.TrackingNumber);
								helper.Writer.Write("TotalWeight", entityShipment.TotalWeight.HasValue ? entityShipment.TotalWeight.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("ShippedDateUtc", entityShipment.ShippedDateUtc.HasValue ? entityShipment.ShippedDateUtc.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("DeliveryDateUtc", entityShipment.DeliveryDateUtc.HasValue ? entityShipment.DeliveryDateUtc.Value.ToString(invariantCulture) : "");
								helper.Writer.Write("CreatedOnUtc", entityShipment.CreatedOnUtc.ToString(invariantCulture));

								helper.Writer.WriteStartElement("ShipmentItems");
								foreach (dynamic shipmentItem in shipment.ShipmentItems)
								{
									ShipmentItem entityShipmentItem = shipmentItem.Entity;

									helper.Writer.WriteStartElement("ShipmentItem");
									helper.Writer.Write("Id", entityShipmentItem.Id.ToString());
									helper.Writer.Write("ShipmentId", entityShipmentItem.ShipmentId.ToString());
									helper.Writer.Write("OrderItemId", entityShipmentItem.OrderItemId.ToString());
									helper.Writer.Write("Quantity", entityShipmentItem.Quantity.ToString());
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
							context.RecordException(exc, entity.Id);
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
