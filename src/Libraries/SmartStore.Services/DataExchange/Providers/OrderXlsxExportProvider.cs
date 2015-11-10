using System;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports Excel formatted order data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreOrderXlsx")]
	[FriendlyName("SmartStore Excel order export")]
	[IsHidden(true)]
	public class OrderXlsxExportProvider : ExportProviderBase
	{
		private string[] Properties
		{
			get
			{
				return new string[]
				{
					"OrderId",
					"OrderGuid",
					"CustomerId",
					"OrderSubtotalInclTax",
					"OrderSubtotalExclTax",
					"OrderSubTotalDiscountInclTax",
					"OrderSubTotalDiscountExclTax",
					"OrderShippingInclTax",
					"OrderShippingExclTax",
					"PaymentMethodAdditionalFeeInclTax",
					"PaymentMethodAdditionalFeeExclTax",
					"TaxRates",
					"OrderTax",
					"OrderTotal",
					"RefundedAmount",
					"OrderDiscount",
					"CurrencyRate",
					"CustomerCurrencyCode",
					"AffiliateId",
					"OrderStatusId",
					"PaymentMethodSystemName",
					"PurchaseOrderNumber",
					"PaymentStatusId",
					"ShippingStatusId",
					"ShippingMethod",
					"ShippingRateComputationMethodSystemName",
					"VatNumber",
					"CreatedOnUtc",
					"UpdatedOnUtc",
					"RewardPointsUsed",
					"RewardPointsRemaining",
					"HasNewPaymentNotification",
					//billing address
					"BillingFirstName",
					"BillingLastName",
					"BillingEmail",
					"BillingCompany",
					"BillingCountry",
					"BillingStateProvince",
					"BillingCity",
					"BillingAddress1",
					"BillingAddress2",
					"BillingZipPostalCode",
					"BillingPhoneNumber",
					"BillingFaxNumber",
					//shipping address
					"ShippingFirstName",
					"ShippingLastName",
					"ShippingEmail",
					"ShippingCompany",
					"ShippingCountry",
					"ShippingStateProvince",
					"ShippingCity",
					"ShippingAddress1",
					"ShippingAddress2",
					"ShippingZipPostalCode",
					"ShippingPhoneNumber",
					"ShippingFaxNumber"
				};
			}
		}

		private void WriteCell(ExcelWorksheet worksheet, int row, ref int column, object value)
		{
			worksheet.Cells[row, column].Value = value;
			++column;
		}

		public static string SystemName
		{
			get { return "Exports.SmartStoreOrderXlsx"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Order; }
		}

		public override string FileExtension
		{
			get { return "XLSX"; }
		}

		protected override void Export(IExportExecuteContext context)
		{
			using (var xlPackage = new ExcelPackage(context.DataStream))
			{
				// uncomment this line if you want the XML written out to the outputDir
				//xlPackage.DebugMode = true; 

				// get handle to the existing worksheet
				var worksheet = xlPackage.Workbook.Worksheets.Add("Orders");

				// create headers and format them
				string[] properties = Properties;

				for (int i = 0; i < properties.Length; ++i)
				{
					worksheet.Cells[1, i + 1].Value = properties[i];
					worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
					worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
					worksheet.Cells[1, i + 1].Style.Font.Bold = true;
				}

				int row = 2;

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic order in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						Order entity = order.Entity;

						try
						{
							int column = 1;
							dynamic customer = order.Customer;
							dynamic billingAddress = order.BillingAddress;
							dynamic shippingAddress = order.ShippingAddress;

							int rewardPointsUsed = 0;
							int rewardPointsRemaining = customer._RewardPointsBalance;

							if (order.RedeemedRewardPointsEntry != null && (int)order.RedeemedRewardPointsEntry.Points != 0)
								rewardPointsUsed = (-1 * (int)order.RedeemedRewardPointsEntry.Points);

							WriteCell(worksheet, row, ref column, (string)order.OrderNumber);
							WriteCell(worksheet, row, ref column, entity.OrderGuid);
							WriteCell(worksheet, row, ref column, entity.CustomerId);
							WriteCell(worksheet, row, ref column, entity.OrderSubtotalInclTax);
							WriteCell(worksheet, row, ref column, entity.OrderSubtotalExclTax);
							WriteCell(worksheet, row, ref column, entity.OrderSubTotalDiscountInclTax);
							WriteCell(worksheet, row, ref column, entity.OrderSubTotalDiscountExclTax);
							WriteCell(worksheet, row, ref column, entity.OrderShippingInclTax);
							WriteCell(worksheet, row, ref column, entity.OrderShippingExclTax);
							WriteCell(worksheet, row, ref column, entity.PaymentMethodAdditionalFeeInclTax);
							WriteCell(worksheet, row, ref column, entity.PaymentMethodAdditionalFeeExclTax);
							WriteCell(worksheet, row, ref column, entity.TaxRates);
							WriteCell(worksheet, row, ref column, entity.OrderTax);
							WriteCell(worksheet, row, ref column, entity.OrderTotal);
							WriteCell(worksheet, row, ref column, entity.RefundedAmount);
							WriteCell(worksheet, row, ref column, entity.OrderDiscount);
							WriteCell(worksheet, row, ref column, entity.CurrencyRate);
							WriteCell(worksheet, row, ref column, entity.CustomerCurrencyCode);
							WriteCell(worksheet, row, ref column, entity.AffiliateId);
							WriteCell(worksheet, row, ref column, entity.OrderStatusId);
							WriteCell(worksheet, row, ref column, entity.PaymentMethodSystemName);
							WriteCell(worksheet, row, ref column, entity.PurchaseOrderNumber);
							WriteCell(worksheet, row, ref column, entity.PaymentStatusId);
							WriteCell(worksheet, row, ref column, entity.ShippingStatusId);
							WriteCell(worksheet, row, ref column, entity.ShippingMethod);
							WriteCell(worksheet, row, ref column, entity.ShippingRateComputationMethodSystemName);
							WriteCell(worksheet, row, ref column, entity.VatNumber);
							WriteCell(worksheet, row, ref column, entity.CreatedOnUtc.ToOADate());
							WriteCell(worksheet, row, ref column, entity.UpdatedOnUtc.ToOADate());
							WriteCell(worksheet, row, ref column, rewardPointsUsed > 0 ? rewardPointsUsed.ToString() : "");
							WriteCell(worksheet, row, ref column, rewardPointsRemaining > 0 ? rewardPointsRemaining.ToString() : "");
							WriteCell(worksheet, row, ref column, entity.HasNewPaymentNotification);

							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.FirstName : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.LastName : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.Email : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.Company : "");
							WriteCell(worksheet, row, ref column, billingAddress != null && billingAddress.Country != null ? (string)billingAddress.Country.Name : "");
							WriteCell(worksheet, row, ref column, billingAddress != null && billingAddress.StateProvince != null ? (string)billingAddress.StateProvince.Name : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.City : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.Address1 : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.Address2 : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.ZipPostalCode : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.PhoneNumber : "");
							WriteCell(worksheet, row, ref column, billingAddress != null ? (string)billingAddress.FaxNumber : "");

							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.FirstName : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.LastName : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.Email : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.Company : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null && shippingAddress.Country != null ? (string)shippingAddress.Country.Name : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null && shippingAddress.StateProvince != null ? (string)shippingAddress.StateProvince.Name : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.City : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.Address1 : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.Address2 : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.ZipPostalCode : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.PhoneNumber : "");
							WriteCell(worksheet, row, ref column, shippingAddress != null ? (string)shippingAddress.FaxNumber : "");
						
							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, entity.Id);
						}

						++row;
					}
				}

				// we had better add some document properties to the spreadsheet 
				// set some core property values
				//var storeName = _storeInformationSettings.StoreName;
				//var storeUrl = _storeInformationSettings.StoreUrl;
				//xlPackage.Workbook.Properties.Title = string.Format("{0} orders", storeName);
				//xlPackage.Workbook.Properties.Author = storeName;
				//xlPackage.Workbook.Properties.Subject = string.Format("{0} orders", storeName);
				//xlPackage.Workbook.Properties.Keywords = string.Format("{0} orders", storeName);
				//xlPackage.Workbook.Properties.Category = "Orders";
				//xlPackage.Workbook.Properties.Comments = string.Format("{0} orders", storeName);

				// set some extended property values
				//xlPackage.Workbook.Properties.Company = storeName;
				//xlPackage.Workbook.Properties.HyperlinkBase = new Uri(storeUrl);

				// save the new spreadsheet
				xlPackage.Save();
			}
		}
	}
}
