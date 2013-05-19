using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using OfficeOpenXml;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        #region properties

        private IDictionary<string, int> s_columnsMap = new Dictionary<string, int>();
        

        //the columns
        private readonly static string[] s_properties = new string[]
        {
            "Name",
            "ShortDescription",
            "FullDescription",
            "ProductTemplateId",
            "ShowOnHomePage",
            "MetaKeywords",
            "MetaDescription",
            "MetaTitle",
            "SeName",
            "AllowCustomerReviews",
            "Published",
            "ProductVariantName",
            "SKU",
            "ManufacturerPartNumber",
            "Gtin",
            "IsGiftCard",
            "GiftCardTypeId",
            "RequireOtherProducts",
            "RequiredProductVariantIds",
            "AutomaticallyAddRequiredProductVariants",
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
			"DeliveryTimeId",	// codehint: sm-add (following)
			"BasePrice_Enabled",
			"BasePrice_MeasureUnit",
			"BasePrice_Amount",
			"BasePrice_BaseAmount"
        };
        #endregion
        
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;

        public ImportManager(IProductService productService, ICategoryService categoryService,
            IManufacturerService manufacturerService, IPictureService pictureService,
            IUrlRecordService urlRecordService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._urlRecordService = urlRecordService;

            this.ColumnsMap = new Dictionary<string, int>();
        }

        internal Dictionary<string, int> ColumnsMap { get; set; }

        protected virtual int GetColumnIndex(string columnName)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            int idx;
            if (ColumnsMap.TryGetValue(columnName, out idx)) 
            {
                return idx;
            }

            for (int i = 0; i < s_properties.Length; i++)
            {
                if (s_properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    ColumnsMap.Add(columnName, i + 1);
                    return i + 1; //excel indexes start from 1
                }
            }

            ColumnsMap.Add(columnName, 0);
            return 0;
        }

        private T GetValue<T>(ExcelWorksheet worksheet, int rowIndex, string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);

            if (columnIndex == 0) 
            {
                return default(T);
            }

            var cell = worksheet.Cells[rowIndex, columnIndex];
            object value = null;

            if (cell.Value != null)
            {
                value = cell.Value;
                return value.Convert<T>();
            }
            else 
            {
                return default(T);
            }
            
        }

		// codehint: sm-add
		private bool HasValue(ExcelWorksheet worksheet, int rowIndex, string columnName) {
			var columnIndex = GetColumnIndex(columnName);

			if (columnIndex == 0)
				return false;

			var cell = worksheet.Cells[rowIndex, columnIndex];
			return (cell != null && cell.Value != null);
		}

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual void ImportProductsFromXlsx(Stream stream)
        {
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new SmartException("No worksheet found");

                // TODO: reset s_columnsMap 

                int iRow = 2;
                while (true)
                {
                    bool allColumnsAreEmpty = true;
                    for (var i = 1; i <= s_properties.Length; i++)
                        if (worksheet.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(worksheet.Cells[iRow, i].Value.ToString()))
                        {
                            allColumnsAreEmpty = false;
                            break;
                        }
                    if (allColumnsAreEmpty)
                        break;

                    string name = GetValue<string>(worksheet, iRow, "Name");
                    string shortDescription = GetValue<string>(worksheet, iRow, "ShortDescription");
                    string fullDescription = GetValue<string>(worksheet, iRow, "FullDescription");
                    int productTemplateId = GetValue<int>(worksheet, iRow, "ProductTemplateId");
                    bool showOnHomePage = GetValue<bool>(worksheet, iRow, "ShowOnHomePage");
                    string metaKeywords = GetValue<string>(worksheet, iRow, "MetaKeywords");
                    string metaDescription = GetValue<string>(worksheet, iRow, "MetaDescription");
                    string metaTitle = GetValue<string>(worksheet, iRow, "MetaTitle");
                    string seName = GetValue<string>(worksheet, iRow, "SeName");
                    bool allowCustomerReviews = GetValue<bool>(worksheet, iRow, "AllowCustomerReviews");
                    bool published = GetValue<bool>(worksheet, iRow, "Published");
                    string productVariantName = GetValue<string>(worksheet, iRow, "ProductVariantName");
                    string sku = GetValue<string>(worksheet, iRow, "SKU");
                    string manufacturerPartNumber = GetValue<string>(worksheet, iRow, "ManufacturerPartNumber");
                    string gtin = GetValue<string>(worksheet, iRow, "Gtin");
                    bool isGiftCard = GetValue<bool>(worksheet, iRow, "IsGiftCard");
                    int giftCardTypeId = GetValue<int>(worksheet, iRow, "GiftCardTypeId");
                    bool requireOtherProducts = GetValue<bool>(worksheet, iRow, "RequireOtherProducts");
                    string requiredProductVariantIds = GetValue<string>(worksheet, iRow, "RequiredProductVariantIds");
                    bool automaticallyAddRequiredProductVariants = GetValue<bool>(worksheet, iRow, "AutomaticallyAddRequiredProductVariants");
                    bool isDownload = GetValue<bool>(worksheet, iRow, "IsDownload");
                    int downloadId = GetValue<int>(worksheet, iRow, "DownloadId");
                    bool unlimitedDownloads = GetValue<bool>(worksheet, iRow, "UnlimitedDownloads");
                    int maxNumberOfDownloads = GetValue<int>(worksheet, iRow, "MaxNumberOfDownloads");
                    int downloadActivationTypeId = GetValue<int>(worksheet, iRow, "DownloadActivationTypeId");
                    bool hasSampleDownload = GetValue<bool>(worksheet, iRow, "HasSampleDownload");
                    int sampleDownloadId = GetValue<int>(worksheet, iRow, "SampleDownloadId");
                    bool hasUserAgreement = GetValue<bool>(worksheet, iRow, "HasUserAgreement");
                    string userAgreementText = GetValue<string>(worksheet, iRow, "UserAgreementText");
                    bool isRecurring = GetValue<bool>(worksheet, iRow, "IsRecurring");
                    int recurringCycleLength = GetValue<int>(worksheet, iRow, "RecurringCycleLength");
                    int recurringCyclePeriodId = GetValue<int>(worksheet, iRow, "RecurringCyclePeriodId");
                    int recurringTotalCycles = GetValue<int>(worksheet, iRow, "RecurringTotalCycles");
                    bool isShipEnabled = GetValue<bool>(worksheet, iRow, "IsShipEnabled");
                    bool isFreeShipping = GetValue<bool>(worksheet, iRow, "IsFreeShipping");
                    decimal additionalShippingCharge = GetValue<decimal>(worksheet, iRow, "AdditionalShippingCharge");
                    bool isTaxExempt = GetValue<bool>(worksheet, iRow, "IsTaxExempt");
                    int taxCategoryId = GetValue<int>(worksheet, iRow, "TaxCategoryId");
                    int manageInventoryMethodId = GetValue<int>(worksheet, iRow, "ManageInventoryMethodId");
                    int stockQuantity = GetValue<int>(worksheet, iRow, "StockQuantity");
                    bool displayStockAvailability = GetValue<bool>(worksheet, iRow, "DisplayStockAvailability");
                    bool displayStockQuantity = GetValue<bool>(worksheet, iRow, "DisplayStockQuantity");
                    int minStockQuantity = GetValue<int>(worksheet, iRow, "MinStockQuantity");
                    int lowStockActivityId = GetValue<int>(worksheet, iRow, "LowStockActivityId");
                    int notifyAdminForQuantityBelow = GetValue<int>(worksheet, iRow, "NotifyAdminForQuantityBelow");
                    int backorderModeId = GetValue<int>(worksheet, iRow, "BackorderModeId");
                    bool allowBackInStockSubscriptions = GetValue<bool>(worksheet, iRow, "AllowBackInStockSubscriptions");
                    int orderMinimumQuantity = GetValue<int>(worksheet, iRow, "OrderMinimumQuantity");
                    int orderMaximumQuantity = GetValue<int>(worksheet, iRow, "OrderMaximumQuantity");
                    string allowedQuantities = GetValue<string>(worksheet, iRow, "AllowedQuantities");
                    bool disableBuyButton = GetValue<bool>(worksheet, iRow, "DisableBuyButton");
                    bool disableWishlistButton = GetValue<bool>(worksheet, iRow, "DisableWishlistButton");
                    bool callForPrice = GetValue<bool>(worksheet, iRow, "CallForPrice");
                    decimal price = GetValue<decimal>(worksheet, iRow, "Price");
                    decimal oldPrice = GetValue<decimal>(worksheet, iRow, "OldPrice");
                    decimal productCost = GetValue<decimal>(worksheet, iRow, "ProductCost");

					// codehint: sm-edit
					decimal? specialPrice = null;
					if (HasValue(worksheet, iRow, "SpecialPrice"))
						specialPrice = GetValue<decimal>(worksheet, iRow, "SpecialPrice");

                    DateTime? specialPriceStartDateTimeUtc = null;
                    var specialPriceStartDateTimeUtcExcel = GetValue<double>(worksheet, iRow, "SpecialPriceStartDateTimeUtc");
                    if (specialPriceStartDateTimeUtcExcel != 0)
                        specialPriceStartDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceStartDateTimeUtcExcel));
                    DateTime? specialPriceEndDateTimeUtc = null;
                    var specialPriceEndDateTimeUtcExcel = GetValue<double>(worksheet, iRow, "SpecialPriceEndDateTimeUtc");
                    if (specialPriceEndDateTimeUtcExcel != 0)
                        specialPriceEndDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceEndDateTimeUtcExcel));

                    //DateTime? specialPriceStartDateTimeUtc = null;
                    //var specialPriceStartDateTimeUtcExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPriceStartDateTimeUtc")].Value;
                    //if (specialPriceStartDateTimeUtcExcel != null)
                    //    specialPriceStartDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceStartDateTimeUtcExcel));
                    //DateTime? specialPriceEndDateTimeUtc = null;
                    //var specialPriceEndDateTimeUtcExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPriceEndDateTimeUtc")].Value;
                    //if (specialPriceEndDateTimeUtcExcel != null)
                    //    specialPriceEndDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceEndDateTimeUtcExcel));


                    bool customerEntersPrice = GetValue<bool>(worksheet, iRow, "CustomerEntersPrice");
                    decimal minimumCustomerEnteredPrice = GetValue<decimal>(worksheet, iRow, "MinimumCustomerEnteredPrice");
                    decimal maximumCustomerEnteredPrice = GetValue<decimal>(worksheet, iRow, "MaximumCustomerEnteredPrice");
                    decimal weight = GetValue<decimal>(worksheet, iRow, "Weight");
                    decimal length = GetValue<decimal>(worksheet, iRow, "Length");
                    decimal width = GetValue<decimal>(worksheet, iRow, "Width");
                    decimal height = GetValue<decimal>(worksheet, iRow, "Height");

                    DateTime createdOnUtc = new DateTime();
                    var createdOnUtcExcel = GetValue<double>(worksheet, iRow, "CreatedOnUtc");
                    if (createdOnUtcExcel != 0)
                        createdOnUtc = DateTime.FromOADate(Convert.ToDouble(createdOnUtcExcel));
                    
                    //DateTime createdOnUtc = DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[iRow, GetColumnIndex(properties, "CreatedOnUtc")].Value));

                    string categoryIds = GetValue<string>(worksheet, iRow, "CategoryIds");
                    string manufacturerIds = GetValue<string>(worksheet, iRow, "ManufacturerIds");
                    string picture1 = GetValue<string>(worksheet, iRow, "Picture1");
                    string picture2 = GetValue<string>(worksheet, iRow, "Picture2");
                    string picture3 = GetValue<string>(worksheet, iRow, "Picture3");

					// codehint: sm-add (note s_properties if you want add columns)
					int? deliveryTimeId = null;
					if (HasValue(worksheet, iRow, "DeliveryTimeId"))
						deliveryTimeId = GetValue<int>(worksheet, iRow, "DeliveryTimeId");

					bool basePriceEnabled = GetValue<bool>(worksheet, iRow, "BasePrice_Enabled");
					string basePriceMeasureUnit = GetValue<string>(worksheet, iRow, "BasePrice_MeasureUnit");

					decimal? basePriceAmount = null;
					if (HasValue(worksheet, iRow, "BasePrice_Amount"))
						basePriceAmount = GetValue<decimal>(worksheet, iRow, "BasePrice_Amount");

					int? basePriceBaseAmount = null;
					if (HasValue(worksheet, iRow, "BasePrice_BaseAmount"))
						basePriceBaseAmount = GetValue<int>(worksheet, iRow, "BasePrice_BaseAmount");


                    //string name = worksheet.Cells[iRow, GetColumnIndex(properties, "Name")].Value as string;
                    //string shortDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "ShortDescription")].Value as string;
                    //string fullDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "FullDescription")].Value as string;
                    //int productTemplateId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "ProductTemplateId")].Value);
                    //bool showOnHomePage = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "ShowOnHomePage")].Value);
                    //string metaKeywords = worksheet.Cells[iRow, GetColumnIndex(properties, "MetaKeywords")].Value as string;
                    //string metaDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "MetaDescription")].Value as string;
                    //string metaTitle = worksheet.Cells[iRow, GetColumnIndex(properties, "MetaTitle")].Value as string;
                    //string seName = worksheet.Cells[iRow, GetColumnIndex(properties, "SeName")].Value as string;
                    //bool allowCustomerReviews = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "AllowCustomerReviews")].Value);
                    //bool published = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "Published")].Value);
                    //string productVariantName = worksheet.Cells[iRow, GetColumnIndex(properties, "ProductVariantName")].Value as string;
                    //string sku = worksheet.Cells[iRow, GetColumnIndex(properties, "SKU")].Value as string;
                    //string manufacturerPartNumber = worksheet.Cells[iRow, GetColumnIndex(properties, "ManufacturerPartNumber")].Value as string;
                    //string gtin = worksheet.Cells[iRow, GetColumnIndex(properties, "Gtin")].Value as string;
                    //bool isGiftCard = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsGiftCard")].Value);
                    //int giftCardTypeId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "GiftCardTypeId")].Value);
                    //bool requireOtherProducts = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "RequireOtherProducts")].Value);
                    //string requiredProductVariantIds = worksheet.Cells[iRow, GetColumnIndex(properties, "RequiredProductVariantIds")].Value as string;
                    //bool automaticallyAddRequiredProductVariants = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "AutomaticallyAddRequiredProductVariants")].Value);
                    //bool isDownload = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsDownload")].Value);
                    //int downloadId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "DownloadId")].Value);
                    //bool unlimitedDownloads = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "UnlimitedDownloads")].Value);
                    //int maxNumberOfDownloads = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "MaxNumberOfDownloads")].Value);
                    //int downloadActivationTypeId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "DownloadActivationTypeId")].Value);
                    //bool hasSampleDownload = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "HasSampleDownload")].Value);
                    //int sampleDownloadId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "SampleDownloadId")].Value);
                    //bool hasUserAgreement = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "HasUserAgreement")].Value);
                    //string userAgreementText = worksheet.Cells[iRow, GetColumnIndex(properties, "UserAgreementText")].Value as string;
                    //bool isRecurring = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsRecurring")].Value);
                    //int recurringCycleLength = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "RecurringCycleLength")].Value);
                    //int recurringCyclePeriodId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "RecurringCyclePeriodId")].Value);
                    //int recurringTotalCycles = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "RecurringTotalCycles")].Value);
                    //bool isShipEnabled = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsShipEnabled")].Value);
                    //bool isFreeShipping = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsFreeShipping")].Value);
                    //decimal additionalShippingCharge = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "AdditionalShippingCharge")].Value);
                    //bool isTaxExempt = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsTaxExempt")].Value);
                    //int taxCategoryId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "TaxCategoryId")].Value);
                    //int manageInventoryMethodId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "ManageInventoryMethodId")].Value);
                    //int stockQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "StockQuantity")].Value);
                    //bool displayStockAvailability = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisplayStockAvailability")].Value);
                    //bool displayStockQuantity = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisplayStockQuantity")].Value);
                    //int minStockQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "MinStockQuantity")].Value);
                    //int lowStockActivityId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "LowStockActivityId")].Value);
                    //int notifyAdminForQuantityBelow = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "NotifyAdminForQuantityBelow")].Value);
                    //int backorderModeId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "BackorderModeId")].Value);
                    //bool allowBackInStockSubscriptions = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "AllowBackInStockSubscriptions")].Value);
                    //int orderMinimumQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "OrderMinimumQuantity")].Value);
                    //int orderMaximumQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "OrderMaximumQuantity")].Value);
                    //string allowedQuantities = worksheet.Cells[iRow, GetColumnIndex(properties, "AllowedQuantities")].Value as string;
                    //bool disableBuyButton = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisableBuyButton")].Value);
                    //bool disableWishlistButton = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisableWishlistButton")].Value);
                    //bool callForPrice = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "CallForPrice")].Value);
                    //decimal price = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Price")].Value);
                    //decimal oldPrice = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "OldPrice")].Value);
                    //decimal productCost = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "ProductCost")].Value);

                    //decimal? specialPrice = null;
                    //var specialPriceExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPrice")].Value;
                    //if (specialPriceExcel != null)
                    //    specialPrice = Convert.ToDecimal(specialPriceExcel);
                    //DateTime? specialPriceStartDateTimeUtc = null;
                    //var specialPriceStartDateTimeUtcExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPriceStartDateTimeUtc")].Value;
                    //if (specialPriceStartDateTimeUtcExcel != null)
                    //    specialPriceStartDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceStartDateTimeUtcExcel));
                    //DateTime? specialPriceEndDateTimeUtc = null;
                    //var specialPriceEndDateTimeUtcExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPriceEndDateTimeUtc")].Value;
                    //if (specialPriceEndDateTimeUtcExcel != null)
                    //    specialPriceEndDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceEndDateTimeUtcExcel));

                    //bool customerEntersPrice = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "CustomerEntersPrice")].Value);
                    //decimal minimumCustomerEnteredPrice = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "MinimumCustomerEnteredPrice")].Value);
                    //decimal maximumCustomerEnteredPrice = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "MaximumCustomerEnteredPrice")].Value);
                    //decimal weight = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Weight")].Value);
                    //decimal length = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Length")].Value);
                    //decimal width = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Width")].Value);
                    //decimal height = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Height")].Value);
                    //DateTime createdOnUtc = DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[iRow, GetColumnIndex(properties, "CreatedOnUtc")].Value));
                    //string categoryIds = worksheet.Cells[iRow, GetColumnIndex(properties, "CategoryIds")].Value as string;
                    //string manufacturerIds = worksheet.Cells[iRow, GetColumnIndex(properties, "ManufacturerIds")].Value as string;
                    //string picture1 = worksheet.Cells[iRow, GetColumnIndex(properties, "Picture1")].Value as string;
                    //string picture2 = worksheet.Cells[iRow, GetColumnIndex(properties, "Picture2")].Value as string;
                    //string picture3 = worksheet.Cells[iRow, GetColumnIndex(properties, "Picture3")].Value as string;


                    ProductVariant productVariant = null;
					// codehint: sm-edit
                    if (sku.HasValue())
                    {
                        productVariant = _productService.GetProductVariantBySku(sku);
                    }

                    if (productVariant == null && gtin.HasValue())
                    {
                        productVariant = _productService.GetProductVariantByGtin(gtin);
                    }

                    if (productVariant != null)
                    {
                        //product
                        var product = productVariant.Product;
                        product.Name = name;
                        product.ShortDescription = shortDescription;
                        product.FullDescription = fullDescription;
                        product.ProductTemplateId = productTemplateId;
                        product.ShowOnHomePage = showOnHomePage;
                        product.MetaKeywords = metaKeywords;
                        product.MetaDescription = metaDescription;
                        product.MetaTitle = metaTitle;
                        product.AllowCustomerReviews = allowCustomerReviews;
                        product.Published = published;
                        product.CreatedOnUtc = createdOnUtc;
                        product.UpdatedOnUtc = DateTime.UtcNow;
                        _productService.UpdateProduct(product);

                        //search engine name
                        _urlRecordService.SaveSlug(product, product.ValidateSeName(seName, product.Name, true), 0);

                        //variant
                        productVariant.Name = productVariantName;
                        productVariant.Sku = sku;
                        productVariant.ManufacturerPartNumber = manufacturerPartNumber;
                        productVariant.Gtin = gtin;
                        productVariant.IsGiftCard = isGiftCard;
                        productVariant.GiftCardTypeId = giftCardTypeId;
                        productVariant.RequireOtherProducts = requireOtherProducts;
                        productVariant.RequiredProductVariantIds = requiredProductVariantIds;
                        productVariant.AutomaticallyAddRequiredProductVariants = automaticallyAddRequiredProductVariants;
                        productVariant.IsDownload = isDownload;
                        productVariant.DownloadId = downloadId;
                        productVariant.UnlimitedDownloads = unlimitedDownloads;
                        productVariant.MaxNumberOfDownloads = maxNumberOfDownloads;
                        productVariant.DownloadActivationTypeId = downloadActivationTypeId;
                        productVariant.HasSampleDownload = hasSampleDownload;
                        productVariant.SampleDownloadId = sampleDownloadId;
                        productVariant.HasUserAgreement = hasUserAgreement;
                        productVariant.UserAgreementText = userAgreementText;
                        productVariant.IsRecurring = isRecurring;
                        productVariant.RecurringCycleLength = recurringCycleLength;
                        productVariant.RecurringCyclePeriodId = recurringCyclePeriodId;
                        productVariant.RecurringTotalCycles = recurringTotalCycles;
                        productVariant.IsShipEnabled = isShipEnabled;
                        productVariant.IsFreeShipping = isFreeShipping;
                        productVariant.AdditionalShippingCharge = additionalShippingCharge;
                        productVariant.IsTaxExempt = isTaxExempt;
                        productVariant.TaxCategoryId = taxCategoryId;
                        productVariant.ManageInventoryMethodId = manageInventoryMethodId;
                        productVariant.StockQuantity = stockQuantity;
                        productVariant.DisplayStockAvailability = displayStockAvailability;
                        productVariant.DisplayStockQuantity = displayStockQuantity;
                        productVariant.MinStockQuantity = minStockQuantity;
                        productVariant.LowStockActivityId = lowStockActivityId;
                        productVariant.NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow;
                        productVariant.BackorderModeId = backorderModeId;
                        productVariant.AllowBackInStockSubscriptions = allowBackInStockSubscriptions;
                        productVariant.OrderMinimumQuantity = orderMinimumQuantity;
                        productVariant.OrderMaximumQuantity = orderMaximumQuantity;
                        productVariant.AllowedQuantities = allowedQuantities;
                        productVariant.DisableBuyButton = disableBuyButton;
                        productVariant.DisableWishlistButton = disableWishlistButton;
                        productVariant.CallForPrice = callForPrice;
                        productVariant.Price = price;
                        productVariant.OldPrice = oldPrice;
                        productVariant.ProductCost = productCost;
                        productVariant.SpecialPrice = specialPrice;
                        productVariant.SpecialPriceStartDateTimeUtc = specialPriceStartDateTimeUtc;
                        productVariant.SpecialPriceEndDateTimeUtc = specialPriceEndDateTimeUtc;
                        productVariant.CustomerEntersPrice = customerEntersPrice;
                        productVariant.MinimumCustomerEnteredPrice = minimumCustomerEnteredPrice;
                        productVariant.MaximumCustomerEnteredPrice = maximumCustomerEnteredPrice;
                        productVariant.Weight = weight;
                        productVariant.Length = length;
                        productVariant.Width = width;
                        productVariant.Height = height;
                        productVariant.Published = published;
                        productVariant.CreatedOnUtc = createdOnUtc;
                        productVariant.UpdatedOnUtc = DateTime.UtcNow;

						// codehint: sm-add
						productVariant.DeliveryTimeId = deliveryTimeId;
						productVariant.BasePrice.Enabled = basePriceEnabled;
						productVariant.BasePrice.MeasureUnit = basePriceMeasureUnit;
						productVariant.BasePrice.Amount = basePriceAmount;
						productVariant.BasePrice.BaseAmount = basePriceBaseAmount;

                        _productService.UpdateProductVariant(productVariant);
                    }
                    else
                    {
                        //product
                        var product = new Product()
                        {
                            Name = name,
                            ShortDescription = shortDescription,
                            FullDescription = fullDescription,
                            ShowOnHomePage = showOnHomePage,
                            MetaKeywords = metaKeywords,
                            MetaDescription = metaDescription,
                            MetaTitle = metaTitle,
                            AllowCustomerReviews = allowCustomerReviews,
                            Published = published,
                            CreatedOnUtc = createdOnUtc,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        _productService.InsertProduct(product);

                        //search engine name
                        _urlRecordService.SaveSlug(product, product.ValidateSeName(seName, product.Name, true), 0);

                        //vairant
                        productVariant = new ProductVariant()
                        {
                            ProductId = product.Id,
                            Name = productVariantName,
                            Sku = sku,
                            ManufacturerPartNumber = manufacturerPartNumber,
                            Gtin = gtin,
                            IsGiftCard = isGiftCard,
                            GiftCardTypeId = giftCardTypeId,
                            RequireOtherProducts = requireOtherProducts,
                            RequiredProductVariantIds = requiredProductVariantIds,
                            AutomaticallyAddRequiredProductVariants = automaticallyAddRequiredProductVariants,
                            IsDownload = isDownload,
                            DownloadId = downloadId,
                            UnlimitedDownloads = unlimitedDownloads,
                            MaxNumberOfDownloads = maxNumberOfDownloads,
                            DownloadActivationTypeId = downloadActivationTypeId,
                            HasSampleDownload = hasSampleDownload,
                            SampleDownloadId = sampleDownloadId,
                            HasUserAgreement = hasUserAgreement,
                            UserAgreementText = userAgreementText,
                            IsRecurring = isRecurring,
                            RecurringCycleLength = recurringCycleLength,
                            RecurringCyclePeriodId = recurringCyclePeriodId,
                            RecurringTotalCycles = recurringTotalCycles,
                            IsShipEnabled = isShipEnabled,
                            IsFreeShipping = isFreeShipping,
                            AdditionalShippingCharge = additionalShippingCharge,
                            IsTaxExempt = isTaxExempt,
                            TaxCategoryId = taxCategoryId,
                            ManageInventoryMethodId = manageInventoryMethodId,
                            StockQuantity = stockQuantity,
                            DisplayStockAvailability = displayStockAvailability,
                            DisplayStockQuantity = displayStockQuantity,
                            MinStockQuantity = minStockQuantity,
                            LowStockActivityId = lowStockActivityId,
                            NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow,
                            BackorderModeId = backorderModeId,
                            AllowBackInStockSubscriptions = allowBackInStockSubscriptions,
                            OrderMinimumQuantity = orderMinimumQuantity,
                            OrderMaximumQuantity = orderMaximumQuantity,
                            AllowedQuantities = allowedQuantities,
                            DisableBuyButton = disableBuyButton,
                            CallForPrice = callForPrice,
                            Price = price,
                            OldPrice = oldPrice,
                            ProductCost = productCost,
                            SpecialPrice = specialPrice,
                            SpecialPriceStartDateTimeUtc = specialPriceStartDateTimeUtc,
                            SpecialPriceEndDateTimeUtc = specialPriceEndDateTimeUtc,
                            CustomerEntersPrice = customerEntersPrice,
                            MinimumCustomerEnteredPrice = minimumCustomerEnteredPrice,
                            MaximumCustomerEnteredPrice = maximumCustomerEnteredPrice,
                            Weight = weight,
                            Length = length,
                            Width = width,
                            Height = height,
                            Published = published,
                            CreatedOnUtc = createdOnUtc,
                            UpdatedOnUtc = DateTime.UtcNow,
							DeliveryTimeId = deliveryTimeId // codehint: sm-add
                        };

						// codehint: sm-add
						productVariant.BasePrice.Enabled = basePriceEnabled;
						productVariant.BasePrice.MeasureUnit = basePriceMeasureUnit;
						productVariant.BasePrice.Amount = basePriceAmount;
						productVariant.BasePrice.BaseAmount = basePriceBaseAmount;


                        _productService.InsertProductVariant(productVariant);
                    }

                    //category mappings
                    if (!String.IsNullOrEmpty(categoryIds))
                    {
                        foreach (var id in categoryIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
                        {
                            if (productVariant.Product.ProductCategories.Where(x => x.CategoryId == id).FirstOrDefault() == null)
                            {
                                //ensure that category exists
                                var category = _categoryService.GetCategoryById(id);
                                if (category != null)
                                {
                                    var productCategory = new ProductCategory()
                                    {
                                        ProductId = productVariant.Product.Id,
                                        CategoryId = category.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    _categoryService.InsertProductCategory(productCategory);
                                }
                            }
                        }
                    }

                    //manufacturer mappings
                    if (!String.IsNullOrEmpty(manufacturerIds))
                    {
                        foreach (var id in manufacturerIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
                        {
                            if (productVariant.Product.ProductManufacturers.Where(x => x.ManufacturerId == id).FirstOrDefault() == null)
                            {
                                //ensure that manufacturer exists
                                var manufacturer = _manufacturerService.GetManufacturerById(id);
                                if (manufacturer != null)
                                {
                                    var productManufacturer = new ProductManufacturer()
                                    {
                                        ProductId = productVariant.Product.Id,
                                        ManufacturerId = manufacturer.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    _manufacturerService.InsertProductManufacturer(productManufacturer);
                                }
                            }
                        }
                    }

                    //pictures
                    foreach (var picture in new string[] { picture1, picture2, picture3 })
                    {
                        if (String.IsNullOrEmpty(picture) || !File.Exists(picture))
                            continue;
                        
                        productVariant.Product.ProductPictures.Add(new ProductPicture()
                        {
                            Picture = _pictureService.InsertPicture(File.ReadAllBytes(picture), "image/jpeg", _pictureService.GetPictureSeName(name), true),
                            DisplayOrder = 1,
                        });
                        _productService.UpdateProduct(productVariant.Product);
                    }

                    //update "HasTierPrices" and "HasDiscountsApplied" properties
                    _productService.UpdateHasTierPricesProperty(productVariant);
                    _productService.UpdateHasDiscountsApplied(productVariant);

                    //next product
                    iRow++;
                }
            }
        }
    }
}
