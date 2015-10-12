using System;
using System.Drawing;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	/// <summary>
	/// Exports Excel formatted order data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreOrderXlsx")]
	[FriendlyName("SmartStore Excel order export")]
	[IsHidden(true)]
	public class ExportOrderXlsxProvider : IExportProvider
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
			get { return "XLSX"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var xlPackage = new ExcelPackage(stream))
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
							WriteCell(worksheet, row, ref column, (Guid)order.OrderGuid);
							WriteCell(worksheet, row, ref column, (int)order.CustomerId);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderSubtotalInclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderSubtotalExclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderSubTotalDiscountInclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderSubTotalDiscountExclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderShippingInclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderShippingExclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.PaymentMethodAdditionalFeeInclTax);
							WriteCell(worksheet, row, ref column, (decimal)order.PaymentMethodAdditionalFeeExclTax);
							WriteCell(worksheet, row, ref column, (string)order.TaxRates);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderTax);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderTotal);
							WriteCell(worksheet, row, ref column, (decimal)order.RefundedAmount);
							WriteCell(worksheet, row, ref column, (decimal)order.OrderDiscount);
							WriteCell(worksheet, row, ref column, (decimal)order.CurrencyRate);
							WriteCell(worksheet, row, ref column, (string)order.CustomerCurrencyCode);
							WriteCell(worksheet, row, ref column, (int)order.AffiliateId);
							WriteCell(worksheet, row, ref column, (int)order.OrderStatusId);
							WriteCell(worksheet, row, ref column, (string)order.PaymentMethodSystemName);
							WriteCell(worksheet, row, ref column, (string)order.PurchaseOrderNumber);
							WriteCell(worksheet, row, ref column, (int)order.PaymentStatusId);
							WriteCell(worksheet, row, ref column, (int)order.ShippingStatusId);
							WriteCell(worksheet, row, ref column, (string)order.ShippingMethod);
							WriteCell(worksheet, row, ref column, (string)order.ShippingRateComputationMethodSystemName);
							WriteCell(worksheet, row, ref column, (string)order.VatNumber);
							WriteCell(worksheet, row, ref column, ((DateTime)order.CreatedOnUtc).ToOADate());
							WriteCell(worksheet, row, ref column, ((DateTime)order.UpdatedOnUtc).ToOADate());
							WriteCell(worksheet, row, ref column, rewardPointsUsed > 0 ? rewardPointsUsed.ToString() : "");
							WriteCell(worksheet, row, ref column, rewardPointsRemaining > 0 ? rewardPointsRemaining.ToString() : "");
							WriteCell(worksheet, row, ref column, (bool)order.HasNewPaymentNotification);

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
							context.RecordException(exc, (int)order.Id);
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

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
