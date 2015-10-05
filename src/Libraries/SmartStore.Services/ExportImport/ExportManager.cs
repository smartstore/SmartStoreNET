using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;

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
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILanguageService _languageService;
		private readonly ICommonServices _services;
        private readonly IStoreMappingService _storeMappingService;
        
        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILanguageService languageService,
			ICommonServices services,
            IStoreMappingService storeMappingService)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._languageService = languageService;
			this._services = services;
            this._storeMappingService = storeMappingService;

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
        /// Export products to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual void ExportProductsToXlsx(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Products");

				// get handle to the cells range of the worksheet
				var cells = worksheet.Cells;

                //Create Headers and format them 
                var properties = new string[]
                {
                    "ProductTypeId",
                    "ParentGroupedProductId",
					"VisibleIndividually",
                    "Name",
                    "ShortDescription",
                    "FullDescription",
                    "ProductTemplateId",
                    "ShowOnHomePage",
					"HomePageDisplayOrder",
                    "MetaKeywords",
                    "MetaDescription",
                    "MetaTitle",
                    "SeName",
                    "AllowCustomerReviews",
                    "Published",
                    "SKU",
                    "ManufacturerPartNumber",
                    "Gtin",
                    "IsGiftCard",
                    "GiftCardTypeId",
                    "RequireOtherProducts",
					"RequiredProductIds",
                    "AutomaticallyAddRequiredProducts",
                    "IsDownload",
                    "DownloadId",
                    "UnlimitedDownloads",
                    "MaxNumberOfDownloads",
                    "DownloadActivationTypeId",
                    "HasSampleDownload",
                    "SampleDownloadId",
                    "HasUserAgreement",
                    "UserAgreementText",
                    "IsRecurring",
                    "RecurringCycleLength",
                    "RecurringCyclePeriodId",
                    "RecurringTotalCycles",
                    "IsShipEnabled",
                    "IsFreeShipping",
                    "AdditionalShippingCharge",
					"IsEsd",
                    "IsTaxExempt",
                    "TaxCategoryId",
                    "ManageInventoryMethodId",
                    "StockQuantity",
                    "DisplayStockAvailability",
                    "DisplayStockQuantity",
                    "MinStockQuantity",
                    "LowStockActivityId",
                    "NotifyAdminForQuantityBelow",
                    "BackorderModeId",
                    "AllowBackInStockSubscriptions",
                    "OrderMinimumQuantity",
                    "OrderMaximumQuantity",
                    "AllowedQuantities",
                    "DisableBuyButton",
                    "DisableWishlistButton",
					"AvailableForPreOrder",
                    "CallForPrice",
                    "Price",
                    "OldPrice",
                    "ProductCost",
                    "SpecialPrice",
                    "SpecialPriceStartDateTimeUtc",
                    "SpecialPriceEndDateTimeUtc",
                    "CustomerEntersPrice",
                    "MinimumCustomerEnteredPrice",
                    "MaximumCustomerEnteredPrice",
                    "Weight",
                    "Length",
                    "Width",
                    "Height",
                    "CreatedOnUtc",
                    "CategoryIds",
                    "ManufacturerIds",
                    "Picture1",
                    "Picture2",
                    "Picture3",
					"DeliveryTimeId",
                    "QuantityUnitId",
					"BasePriceEnabled",
					"BasePriceMeasureUnit",
					"BasePriceAmount",
					"BasePriceBaseAmount",
					"BundleTitleText",
					"BundlePerItemShipping",
					"BundlePerItemPricing",
					"BundlePerItemShoppingCart",
					"BundleItemSkus",
                    "AvailableStartDateTimeUtc",
                    "AvailableEndDateTimeUtc",
                    "StoreIds",
                    "LimitedToStores"
                };

                //BEGIN: add headers for languages 
                var languages = _languageService.GetAllLanguages(true);
                var headlines = new string[properties.Length + languages.Count * 3];
                var languageFields = new string[languages.Count * 3];
                var j = 0;

                foreach (var lang in languages)
                {
                    languageFields.SetValue("Name[" + lang.UniqueSeoCode + "]", j++);
                    languageFields.SetValue("ShortDescription[" + lang.UniqueSeoCode + "]", j++);
                    languageFields.SetValue("FullDescription[" + lang.UniqueSeoCode + "]", j++);
                }

                properties.CopyTo(headlines, 0);
                languageFields.CopyTo(headlines, properties.Length);
                //END: add headers for languages 

                for (int i = 0; i < headlines.Length; i++)
                {
                    cells[1, i + 1].Value = headlines[i];
                    cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var p in products)
                {
                    int col = 1;

					cells[row, col].Value = p.ProductTypeId;
					col++;

					cells[row, col].Value = p.ParentGroupedProductId;
					col++;

					cells[row, col].Value = p.VisibleIndividually;
					col++;

					cells[row, col].Value = p.Name;
					col++;

					cells[row, col].Value = p.ShortDescription;
					col++;

					cells[row, col].Value = p.FullDescription;
					col++;

					cells[row, col].Value = p.ProductTemplateId;
					col++;

					cells[row, col].Value = p.ShowOnHomePage;
					col++;

					cells[row, col].Value = p.HomePageDisplayOrder;
					col++;

					cells[row, col].Value = p.MetaKeywords;
					col++;

					cells[row, col].Value = p.MetaDescription;
					col++;

					cells[row, col].Value = p.MetaTitle;
					col++;

					cells[row, col].Value = p.GetSeName(0);
					col++;

					cells[row, col].Value = p.AllowCustomerReviews;
					col++;

					cells[row, col].Value = p.Published;
					col++;

					cells[row, col].Value = p.Sku;
					col++;

					cells[row, col].Value = p.ManufacturerPartNumber;
					col++;

					cells[row, col].Value = p.Gtin;
					col++;

					cells[row, col].Value = p.IsGiftCard;
					col++;

					cells[row, col].Value = p.GiftCardTypeId;
					col++;

					cells[row, col].Value = p.RequireOtherProducts;
					col++;

					cells[row, col].Value = p.RequiredProductIds;
					col++;

					cells[row, col].Value = p.AutomaticallyAddRequiredProducts;
					col++;

					cells[row, col].Value = p.IsDownload;
					col++;

					cells[row, col].Value = p.DownloadId;
					col++;

					cells[row, col].Value = p.UnlimitedDownloads;
					col++;

					cells[row, col].Value = p.MaxNumberOfDownloads;
					col++;

					cells[row, col].Value = p.DownloadActivationTypeId;
					col++;

					cells[row, col].Value = p.HasSampleDownload;
					col++;

					cells[row, col].Value = p.SampleDownloadId;
					col++;

					cells[row, col].Value = p.HasUserAgreement;
					col++;

					cells[row, col].Value = p.UserAgreementText;
					col++;

					cells[row, col].Value = p.IsRecurring;
					col++;

					cells[row, col].Value = p.RecurringCycleLength;
					col++;

					cells[row, col].Value = p.RecurringCyclePeriodId;
					col++;

					cells[row, col].Value = p.RecurringTotalCycles;
					col++;

					cells[row, col].Value = p.IsShipEnabled;
					col++;

					cells[row, col].Value = p.IsFreeShipping;
					col++;

					cells[row, col].Value = p.AdditionalShippingCharge;
					col++;

					cells[row, col].Value = p.IsEsd;
					col++;

					cells[row, col].Value = p.IsTaxExempt;
					col++;

					cells[row, col].Value = p.TaxCategoryId;
					col++;

					cells[row, col].Value = p.ManageInventoryMethodId;
					col++;

					cells[row, col].Value = p.StockQuantity;
					col++;

					cells[row, col].Value = p.DisplayStockAvailability;
					col++;

					cells[row, col].Value = p.DisplayStockQuantity;
					col++;

					cells[row, col].Value = p.MinStockQuantity;
					col++;

					cells[row, col].Value = p.LowStockActivityId;
					col++;

					cells[row, col].Value = p.NotifyAdminForQuantityBelow;
					col++;

					cells[row, col].Value = p.BackorderModeId;
					col++;

					cells[row, col].Value = p.AllowBackInStockSubscriptions;
					col++;

					cells[row, col].Value = p.OrderMinimumQuantity;
					col++;

					cells[row, col].Value = p.OrderMaximumQuantity;
					col++;

					cells[row, col].Value = p.AllowedQuantities;
					col++;

					cells[row, col].Value = p.DisableBuyButton;
					col++;

					cells[row, col].Value = p.DisableWishlistButton;
					col++;

					cells[row, col].Value = p.AvailableForPreOrder;
					col++;

					cells[row, col].Value = p.CallForPrice;
					col++;

					cells[row, col].Value = p.Price;
					col++;

					cells[row, col].Value = p.OldPrice;
					col++;

					cells[row, col].Value = p.ProductCost;
					col++;

					cells[row, col].Value = p.SpecialPrice;
					col++;

					cells[row, col].Value = p.SpecialPriceStartDateTimeUtc;
					col++;

					cells[row, col].Value = p.SpecialPriceEndDateTimeUtc;
					col++;

					cells[row, col].Value = p.CustomerEntersPrice;
					col++;

					cells[row, col].Value = p.MinimumCustomerEnteredPrice;
					col++;

					cells[row, col].Value = p.MaximumCustomerEnteredPrice;
					col++;

					cells[row, col].Value = p.Weight;
					col++;

					cells[row, col].Value = p.Length;
					col++;

					cells[row, col].Value = p.Width;
					col++;

					cells[row, col].Value = p.Height;
					col++;

					cells[row, col].Value = p.CreatedOnUtc.ToOADate();
					col++;

                    //category identifiers
                    string categoryIds = null;
                    foreach (var pc in _categoryService.GetProductCategoriesByProductId(p.Id))
                    {
                        categoryIds += pc.CategoryId;
                        categoryIds += ";";
                    }
					cells[row, col].Value = categoryIds;
					col++;

                    //manufacturer identifiers
                    string manufacturerIds = null;
                    foreach (var pm in _manufacturerService.GetProductManufacturersByProductId(p.Id))
                    {
                        manufacturerIds += pm.ManufacturerId;
                        manufacturerIds += ";";
                    }
					cells[row, col].Value = manufacturerIds;
					col++;

					//pictures (up to 3 pictures)
					var pics = new string[] { null, null, null };
					var pictures = _pictureService.GetPicturesByProductId(p.Id, 3);
					for (int i = 0; i < pictures.Count; i++)
					{
						pics[i] = _pictureService.GetThumbLocalPath(pictures[i]);
						pictures[i].PictureBinary = null;
					}
					cells[row, col].Value = pics[0];
					col++;
					cells[row, col].Value = pics[1];
					col++;
					cells[row, col].Value = pics[2];
					col++;

					cells[row, col].Value = p.DeliveryTimeId;
					col++;
                    cells[row, col].Value = p.QuantityUnitId;
                    col++;
					cells[row, col].Value = p.BasePriceEnabled;
					col++;
					cells[row, col].Value = p.BasePriceMeasureUnit;
					col++;
					cells[row, col].Value = p.BasePriceAmount;
					col++;
					cells[row, col].Value = p.BasePriceBaseAmount;
					col++;

					cells[row, col].Value = p.BundleTitleText;
					col++;

					cells[row, col].Value = p.BundlePerItemShipping;
					col++;

					cells[row, col].Value = p.BundlePerItemPricing;
					col++;

					cells[row, col].Value = p.BundlePerItemShoppingCart;
					col++;

					string bundleItemSkus = "";

					if (p.ProductType == ProductType.BundledProduct)
					{
						bundleItemSkus = string.Join(",", _productService.GetBundleItems(p.Id, true).Select(x => x.Item.Product.Sku));
					}

					cells[row, col].Value = bundleItemSkus;
					col++;

                    cells[row, col].Value = p.AvailableStartDateTimeUtc;
                    col++;

                    cells[row, col].Value = p.AvailableEndDateTimeUtc;
                    col++;

                    string storeIds = "";

                    if (p.LimitedToStores)
                    {
                        storeIds = string.Join(";", _storeMappingService.GetStoreMappings(p).Select(x => x.StoreId));
                    }
                    cells[row, col].Value = storeIds;
                    col++;

                    cells[row, col].Value = p.LimitedToStores;
                    col++;

                    //BEGIN: export localized values
                    foreach (var lang in languages)
                    {
                        worksheet.Cells[row, col].Value = p.GetLocalized(x => x.Name, lang.Id, false, false);
                        col++;

                        worksheet.Cells[row, col].Value = p.GetLocalized(x => x.ShortDescription, lang.Id, false, false);
                        col++;

                        worksheet.Cells[row, col].Value = p.GetLocalized(x => x.FullDescription, lang.Id, false, false);
                        col++;
                    }
                    //END: export localized values

                    row++;
                }

                // we had better add some document properties to the spreadsheet 

                // set some core property values
				//var storeName = _storeInformationSettings.StoreName;
				//var storeUrl = _storeInformationSettings.StoreUrl;
				//xlPackage.Workbook.Properties.Title = string.Format("{0} products", storeName);
				//xlPackage.Workbook.Properties.Author = storeName;
				//xlPackage.Workbook.Properties.Subject = string.Format("{0} products", storeName);
				//xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", storeName);
				//xlPackage.Workbook.Properties.Category = "Products";
				//xlPackage.Workbook.Properties.Comments = string.Format("{0} products", storeName);

				// set some extended property values
				//xlPackage.Workbook.Properties.Company = storeName;
				//xlPackage.Workbook.Properties.HyperlinkBase = new Uri(storeUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }

			// EPPLus had serious memory leak problems in V3.
			// We enforce the garbage collector to release unused memory,
 			// it's not perfect, but better than nothing.
			GC.Collect();
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
