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

        #region Utilities

        protected virtual void WriteCategories(XmlWriter writer, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            if (categories != null && categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    writer.WriteStartElement("Category");
                    writer.Write("Id", category.Id.ToString());
                    writer.Write("Name", category.Name);
					writer.Write("FullName", category.FullName);
                    writer.Write("Description", category.Description);
					writer.Write("BottomDescription", category.BottomDescription);
                    writer.Write("CategoryTemplateId", category.CategoryTemplateId.ToString());
                    writer.Write("MetaKeywords", category.MetaKeywords);
                    writer.Write("MetaDescription", category.MetaDescription);
                    writer.Write("MetaTitle", category.MetaTitle);
                    writer.Write("SeName", category.GetSeName(0, true, false));
                    writer.Write("ParentCategoryId", category.ParentCategoryId.ToString());
                    writer.Write("PageSize", category.PageSize.ToString());
                    writer.Write("AllowCustomersToSelectPageSize", category.AllowCustomersToSelectPageSize.ToString());
                    writer.Write("PageSizeOptions", category.PageSizeOptions);
                    writer.Write("PriceRanges", category.PriceRanges);
                    writer.Write("ShowOnHomePage", category.ShowOnHomePage.ToString());
					writer.Write("HasDiscountsApplied", category.HasDiscountsApplied.ToString());
                    writer.Write("Published", category.Published.ToString());
                    writer.Write("Deleted", category.Deleted.ToString());
                    writer.Write("DisplayOrder", category.DisplayOrder.ToString());
                    writer.Write("CreatedOnUtc", category.CreatedOnUtc.ToString());
                    writer.Write("UpdatedOnUtc", category.UpdatedOnUtc.ToString());
					writer.Write("SubjectToAcl", category.SubjectToAcl.ToString());
					writer.Write("LimitedToStores", category.LimitedToStores.ToString());
					writer.Write("Alias", category.Alias);
					writer.Write("DefaultViewMode", category.DefaultViewMode);

                    writer.WriteStartElement("Products");
                    var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, 0, int.MaxValue, true);
                    foreach (var productCategory in productCategories)
                    {
                        var product = productCategory.Product;
                        if (product != null && !product.Deleted)
                        {
                            writer.WriteStartElement("ProductCategory");
                            writer.Write("ProductCategoryId", productCategory.Id.ToString());
                            writer.Write("ProductId", productCategory.ProductId.ToString());
                            writer.Write("IsFeaturedProduct", productCategory.IsFeaturedProduct.ToString());
                            writer.Write("DisplayOrder", productCategory.DisplayOrder.ToString());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();

                    writer.WriteStartElement("SubCategories");
                    WriteCategories(writer, category.Id);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportManufacturersToXml(IList<Manufacturer> manufacturers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Manufacturers");
            xmlWriter.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

            foreach (var manufacturer in manufacturers)
            {
                xmlWriter.WriteStartElement("Manufacturer");

                xmlWriter.WriteElementString("Id", null, manufacturer.Id.ToString());
                xmlWriter.WriteElementString("Name", null, manufacturer.Name);
				xmlWriter.WriteElementString("SeName", null, manufacturer.GetSeName(0, true, false));
                xmlWriter.WriteElementString("Description", null, manufacturer.Description);
                xmlWriter.WriteElementString("ManufacturerTemplateId", null, manufacturer.ManufacturerTemplateId.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, manufacturer.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, manufacturer.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, manufacturer.MetaTitle);
                xmlWriter.WriteElementString("PictureId", null, manufacturer.PictureId.ToString());
                xmlWriter.WriteElementString("PageSize", null, manufacturer.PageSize.ToString());
                xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, manufacturer.AllowCustomersToSelectPageSize.ToString());
                xmlWriter.WriteElementString("PageSizeOptions", null, manufacturer.PageSizeOptions);
                xmlWriter.WriteElementString("PriceRanges", null, manufacturer.PriceRanges);
                xmlWriter.WriteElementString("Published", null, manufacturer.Published.ToString());
                xmlWriter.WriteElementString("Deleted", null, manufacturer.Deleted.ToString());
                xmlWriter.WriteElementString("DisplayOrder", null, manufacturer.DisplayOrder.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, manufacturer.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, manufacturer.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("Products");
                var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturer.Id, 0, int.MaxValue, true);
                if (productManufacturers != null)
                {
                    foreach (var productManufacturer in productManufacturers)
                    {
                        var product = productManufacturer.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductManufacturer");
                            xmlWriter.WriteElementString("Id", null, productManufacturer.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productManufacturer.ProductId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCategoriesToXml()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Categories");
            xmlWriter.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);
            WriteCategories(xmlWriter, 0);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

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

        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCustomersToXml(IList<Customer> customers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Customers");
            xmlWriter.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

            foreach (var customer in customers)
            {
                xmlWriter.WriteStartElement("Customer");

                xmlWriter.WriteElementString("Id", null, customer.Id.ToString());
                xmlWriter.WriteElementString("CustomerNumber", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber));
                xmlWriter.WriteElementString("CustomerGuid", null, customer.CustomerGuid.ToString());
                xmlWriter.WriteElementString("Email", null, customer.Email);
                xmlWriter.WriteElementString("Username", null, customer.Username);
                xmlWriter.WriteElementString("Password", null, customer.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, customer.PasswordFormatId.ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, customer.PasswordSalt);
				xmlWriter.WriteElementString("AdminComment", null, customer.AdminComment);
                xmlWriter.WriteElementString("IsTaxExempt", null, customer.IsTaxExempt.ToString());
				xmlWriter.WriteElementString("AffiliateId", null, customer.AffiliateId.ToString());
                xmlWriter.WriteElementString("Active", null, customer.Active.ToString());
				xmlWriter.WriteElementString("IsSystemAccount", null, customer.IsSystemAccount.ToString());
				xmlWriter.WriteElementString("SystemName", null, customer.SystemName);
				xmlWriter.WriteElementString("LastIpAddress", null, customer.LastIpAddress);
				xmlWriter.WriteElementString("CreatedOnUtc", null, customer.CreatedOnUtc.ToString());
				xmlWriter.WriteElementString("LastLoginDateUtc", null, customer.LastLoginDateUtc.HasValue ? customer.LastLoginDateUtc.Value.ToString() : "");
				xmlWriter.WriteElementString("LastActivityDateUtc", null, customer.LastActivityDateUtc.ToString());

                xmlWriter.WriteElementString("IsGuest", null, customer.IsGuest().ToString());
                xmlWriter.WriteElementString("IsRegistered", null, customer.IsRegistered().ToString());
                xmlWriter.WriteElementString("IsAdministrator", null, customer.IsAdmin().ToString());
                xmlWriter.WriteElementString("IsForumModerator", null, customer.IsForumModerator().ToString());

                xmlWriter.WriteElementString("FirstName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName));
                xmlWriter.WriteElementString("LastName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName));
                xmlWriter.WriteElementString("Gender", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender));
                xmlWriter.WriteElementString("Company", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Company));

                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StreetAddress", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress));
                xmlWriter.WriteElementString("StreetAddress2", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2));
                xmlWriter.WriteElementString("ZipPostalCode", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode));
                xmlWriter.WriteElementString("City", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.City));
                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StateProvinceId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId).ToString());
                xmlWriter.WriteElementString("Phone", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone));
                xmlWriter.WriteElementString("Fax", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax));
				xmlWriter.WriteElementString("VatNumber", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber));
				xmlWriter.WriteElementString("VatNumberStatusId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId).ToString());
				xmlWriter.WriteElementString("TimeZoneId", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId));

                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email);
                bool subscribedToNewsletters = newsletter != null && newsletter.Active;
                xmlWriter.WriteElementString("Newsletter", null, subscribedToNewsletters.ToString());

                xmlWriter.WriteElementString("AvatarPictureId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId).ToString());
                xmlWriter.WriteElementString("ForumPostCount", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount).ToString());
                xmlWriter.WriteElementString("Signature", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature));

				xmlWriter.WriteStartElement("Addresses");

				foreach (var address in customer.Addresses)
				{
					bool isCurrentBillingAddress = (customer.BillingAddress != null && customer.BillingAddress.Id == address.Id);
					bool isCurrentShippingAddress = (customer.ShippingAddress != null && customer.ShippingAddress.Id == address.Id);

					xmlWriter.WriteStartElement("Address");
					xmlWriter.WriteElementString("IsCurrentBillingAddress", null, isCurrentBillingAddress.ToString());
					xmlWriter.WriteElementString("IsCurrentShippingAddress", null, isCurrentShippingAddress.ToString());

					xmlWriter.WriteElementString("Id", null, address.Id.ToString());
					xmlWriter.WriteElementString("FirstName", null, address.FirstName);
					xmlWriter.WriteElementString("LastName", null, address.LastName);
					xmlWriter.WriteElementString("Email", null, address.Email);
					xmlWriter.WriteElementString("Company", null, address.Company);
					xmlWriter.WriteElementString("City", null, address.City);
					xmlWriter.WriteElementString("Address1", null, address.Address1);
					xmlWriter.WriteElementString("Address2", null, address.Address2);
					xmlWriter.WriteElementString("ZipPostalCode", null, address.ZipPostalCode);
					xmlWriter.WriteElementString("PhoneNumber", null, address.PhoneNumber);
					xmlWriter.WriteElementString("FaxNumber", null, address.FaxNumber);
					xmlWriter.WriteElementString("CreatedOnUtc", null, address.CreatedOnUtc.ToString());

					if (address.Country != null)
					{
						xmlWriter.WriteStartElement("Country");
						xmlWriter.WriteElementString("Id", null, address.Country.Id.ToString());
						xmlWriter.WriteElementString("Name", null, address.Country.Name);
						xmlWriter.WriteElementString("AllowsBilling", null, address.Country.AllowsBilling.ToString());
						xmlWriter.WriteElementString("AllowsShipping", null, address.Country.AllowsShipping.ToString());
						xmlWriter.WriteElementString("TwoLetterIsoCode", null, address.Country.TwoLetterIsoCode);
						xmlWriter.WriteElementString("ThreeLetterIsoCode", null, address.Country.ThreeLetterIsoCode);
						xmlWriter.WriteElementString("NumericIsoCode", null, address.Country.NumericIsoCode.ToString());
						xmlWriter.WriteElementString("SubjectToVat", null, address.Country.SubjectToVat.ToString());
						xmlWriter.WriteElementString("Published", null, address.Country.Published.ToString());
						xmlWriter.WriteElementString("DisplayOrder", null, address.Country.DisplayOrder.ToString());
						xmlWriter.WriteElementString("LimitedToStores", null, address.Country.LimitedToStores.ToString());
						xmlWriter.WriteEndElement();	// Country
					}

					if (address.StateProvince != null)
					{
						xmlWriter.WriteStartElement("StateProvince");
						xmlWriter.WriteElementString("Id", null, address.StateProvince.Id.ToString());
						xmlWriter.WriteElementString("CountryId", null, address.StateProvince.CountryId.ToString());
						xmlWriter.WriteElementString("Name", null, address.StateProvince.Name);
						xmlWriter.WriteElementString("Abbreviation", null, address.StateProvince.Abbreviation);
						xmlWriter.WriteElementString("Published", null, address.StateProvince.Published.ToString());
						xmlWriter.WriteElementString("DisplayOrder", null, address.StateProvince.DisplayOrder.ToString());
						xmlWriter.WriteEndElement();	// StateProvince
					}

					xmlWriter.WriteEndElement();	// Address
				}

				xmlWriter.WriteEndElement();	// Addresses

				xmlWriter.WriteEndElement();	// Customer
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        #endregion
    }
}
