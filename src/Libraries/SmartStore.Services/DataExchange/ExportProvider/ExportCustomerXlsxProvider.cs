using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	/// <summary>
	/// Exports Excel formatted customer data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreCustomerXlsx")]
	[FriendlyName("SmartStore Excel customer export")]
	[IsHidden(true)]
	public class ExportCustomerXlsxProvider : IExportProvider
	{
		private string[] Properties
		{
			get
			{
				return new string[]
				{
					"Id",
					"CustomerNumber",
					"CustomerGuid",
					"Email",
					"Username",
					"PasswordStr",
					"PasswordFormatId",
					"PasswordSalt",
					"AdminComment",
					"IsTaxExempt",
					"AffiliateId",
					"Active",
					"IsSystemAccount",
					"SystemName",
					"LastIpAddress",
					"CreatedOnUtc",
					"LastLoginDateUtc",
					"LastActivityDateUtc",
					"IsGuest",
					"IsRegistered",
					"IsAdministrator",
					"IsForumModerator",
					"FirstName",
					"LastName",
					"Gender",
					"Company",
					"StreetAddress",
					"StreetAddress2",
					"ZipPostalCode",
					"City",
					"CountryId",
					"StateProvinceId",
					"Phone",
					"Fax",
					"VatNumber",
					"VatNumberStatusId",
					"TimeZoneId",
					"Newsletter",
					"AvatarPictureId",
					"ForumPostCount",
					"Signature"
				};
			}
		}

		private void WriteCell(ExcelWorksheet worksheet, int row, ref int column, object value)
		{
			worksheet.Cells[row, column].Value = value;
			++column;
		}

		private string GetGenericAttributeValue(dynamic customer, string attributeName)
		{
			if (customer._GenericAttributes != null)
			{
				foreach (var attribute in customer._GenericAttributes)
				{
					if (attributeName.IsCaseInsensitiveEqual((string)attribute.Key))
					{
						return (string)attribute.Value;
					}
				}
			}
			return null;
		}

		public static string SystemName
		{
			get { return "Exports.SmartStoreCustomerXlsx"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Customer; }
		}

		public string FileExtension
		{
			get { return "XLSX"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var invariantCulture = CultureInfo.InvariantCulture;
			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var xlPackage = new ExcelPackage(stream))
			{
				// uncomment this line if you want the XML written out to the outputDir
				//xlPackage.DebugMode = true; 

				// get handle to the existing worksheet
				var worksheet = xlPackage.Workbook.Worksheets.Add("Customers");

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

					foreach (dynamic customer in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						try
						{
							int column = 1;
							DateTime? lastLoginDateUtc = customer.LastLoginDateUtc;
							Customer entity = customer._Entity;

							WriteCell(worksheet, row, ref column, (int)customer.Id);
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.CustomerNumber));
							WriteCell(worksheet, row, ref column, (Guid)customer.CustomerGuid);
							WriteCell(worksheet, row, ref column, (string)customer.Email);
							WriteCell(worksheet, row, ref column, (string)customer.Username);
							WriteCell(worksheet, row, ref column, (string)customer.Password);
							WriteCell(worksheet, row, ref column, (int)customer.PasswordFormatId);
							WriteCell(worksheet, row, ref column, (string)customer.PasswordSalt);
							WriteCell(worksheet, row, ref column, (string)customer.AdminComment);
							WriteCell(worksheet, row, ref column, (bool)customer.IsTaxExempt);
							WriteCell(worksheet, row, ref column, (int)customer.AffiliateId);
							WriteCell(worksheet, row, ref column, (bool)customer.Active);
							WriteCell(worksheet, row, ref column, (bool)customer.IsSystemAccount);
							WriteCell(worksheet, row, ref column, (string)customer.SystemName);
							WriteCell(worksheet, row, ref column, (string)customer.LastIpAddress);
							WriteCell(worksheet, row, ref column, ((DateTime)customer.CreatedOnUtc).ToString(invariantCulture));
							WriteCell(worksheet, row, ref column, lastLoginDateUtc.HasValue ? lastLoginDateUtc.Value.ToString(invariantCulture) : null);
							WriteCell(worksheet, row, ref column, ((DateTime)customer.LastActivityDateUtc).ToString(invariantCulture));
							WriteCell(worksheet, row, ref column, entity.IsGuest());
							WriteCell(worksheet, row, ref column, entity.IsRegistered());
							WriteCell(worksheet, row, ref column, entity.IsAdmin());
							WriteCell(worksheet, row, ref column, entity.IsForumModerator());

							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.FirstName));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.LastName));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.Gender));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.Company));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.StreetAddress));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.StreetAddress2));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.ZipPostalCode));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.City));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.CountryId));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.StateProvinceId));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.Phone));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.Fax));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.VatNumber));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.VatNumberStatusId));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.TimeZoneId));
							WriteCell(worksheet, row, ref column, (bool)customer._HasNewsletterSubscription);
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.AvatarPictureId));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.ForumPostCount));
							WriteCell(worksheet, row, ref column, GetGenericAttributeValue(customer, SystemCustomerAttributeNames.Signature));

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)customer.Id);
						}

						++row;
					}
				}

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
