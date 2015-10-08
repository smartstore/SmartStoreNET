using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Messages;
using SmartStore.Services.Seo;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        
        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="customers">Customers</param>
        public virtual void ExportCustomersToXlsx(Stream stream, IList<Customer> customers)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Customers");
                //Create Headers and format them
                var properties = new string[]
                    {
                        "Id",
                        "CustomerNumber",
                        "CustomerGuid",
                        "Email",
                        "Username",
                        "PasswordStr",//why can't we use 'Password' name?
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
                        "Signature",
                    };

                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var customer in customers)
                {
                    int col = 1;

                    worksheet.Cells[row, col].Value = customer.Id;
                    col++;

                    worksheet.Cells[row, col].Value = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber);
                    col++;
                    
                    worksheet.Cells[row, col].Value = customer.CustomerGuid;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Email;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Username;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Password;
                    col++;

                    worksheet.Cells[row, col].Value = customer.PasswordFormatId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.PasswordSalt;
                    col++;

					worksheet.Cells[row, col].Value = customer.AdminComment;
					col++;

                    worksheet.Cells[row, col].Value = customer.IsTaxExempt;
                    col++;

					worksheet.Cells[row, col].Value = customer.AffiliateId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Active;
                    col++;

					worksheet.Cells[row, col].Value = customer.IsSystemAccount;
					col++;

					worksheet.Cells[row, col].Value = customer.SystemName;
					col++;

					worksheet.Cells[row, col].Value = customer.LastIpAddress;
					col++;

					worksheet.Cells[row, col].Value = customer.CreatedOnUtc.ToString();
					col++;

					worksheet.Cells[row, col].Value = (customer.LastLoginDateUtc.HasValue ? customer.LastLoginDateUtc.Value.ToString() : null);
					col++;

					worksheet.Cells[row, col].Value = customer.LastActivityDateUtc.ToString();
					col++;


                    //roles
                    worksheet.Cells[row, col].Value = customer.IsGuest();
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsRegistered();
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsAdmin();
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsForumModerator();
                    col++;

                    //attributes
                    var firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                    var lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                    var gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                    var company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
                    var streetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                    var streetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                    var zipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                    var city = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                    var countryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                    var stateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId);
                    var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                    var fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);
					var vatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
					var vatNumberStatusId = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumberStatusId);
					var timeZoneId = customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId);

					var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email);
					bool subscribedToNewsletters = newsletter != null && newsletter.Active;

                    var avatarPictureId = customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId);
                    var forumPostCount = customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount);
                    var signature = customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature);

                    worksheet.Cells[row, col].Value = firstName;
                    col++;

                    worksheet.Cells[row, col].Value = lastName;
                    col++;

                    worksheet.Cells[row, col].Value = gender;
                    col++;

                    worksheet.Cells[row, col].Value = company;
                    col++;

                    worksheet.Cells[row, col].Value = streetAddress;
                    col++;

                    worksheet.Cells[row, col].Value = streetAddress2;
                    col++;

                    worksheet.Cells[row, col].Value = zipPostalCode;
                    col++;

                    worksheet.Cells[row, col].Value = city;
                    col++;

                    worksheet.Cells[row, col].Value = countryId;
                    col++;

                    worksheet.Cells[row, col].Value = stateProvinceId;
                    col++;

                    worksheet.Cells[row, col].Value = phone;
                    col++;

                    worksheet.Cells[row, col].Value = fax;
                    col++;

					worksheet.Cells[row, col].Value = vatNumber;
					col++;

					worksheet.Cells[row, col].Value = vatNumberStatusId;
					col++;

					worksheet.Cells[row, col].Value = timeZoneId;
					col++;

					worksheet.Cells[row, col].Value = subscribedToNewsletters;
					col++;

                    worksheet.Cells[row, col].Value = avatarPictureId;
                    col++;

                    worksheet.Cells[row, col].Value = forumPostCount;
                    col++;

                    worksheet.Cells[row, col].Value = signature;
                    col++;

                    row++;
                }

                // we had better add some document properties to the spreadsheet 

                // set some core property values
				//var storeName = _storeInformationSettings.StoreName;
				//var storeUrl = _storeInformationSettings.StoreUrl;
				//xlPackage.Workbook.Properties.Title = string.Format("{0} customers", storeName);
				//xlPackage.Workbook.Properties.Author = storeName;
				//xlPackage.Workbook.Properties.Subject = string.Format("{0} customers", storeName);
				//xlPackage.Workbook.Properties.Keywords = string.Format("{0} customers", storeName);
				//xlPackage.Workbook.Properties.Category = "Customers";
				//xlPackage.Workbook.Properties.Comments = string.Format("{0} customers", storeName);

				// set some extended property values
				//xlPackage.Workbook.Properties.Company = storeName;
				//xlPackage.Workbook.Properties.HyperlinkBase = new Uri(storeUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        #endregion
    }
}
