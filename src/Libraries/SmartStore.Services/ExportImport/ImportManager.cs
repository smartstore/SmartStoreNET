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
            "ProductTypeId",
            "ParentProductId",
			"VisibleIndividually",
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

		private bool HasValue(ExcelWorksheet worksheet, int rowIndex, string columnName)
		{
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

					int productTypeId = GetValue<int>(worksheet, iRow, "ProductTypeId");
					int parentProductId = GetValue<int>(worksheet, iRow, "ParentProductId");
					bool visibleIndividually = GetValue<bool>(worksheet, iRow, "VisibleIndividually");
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
					string requiredProductIds = GetValue<string>(worksheet, iRow, "RequiredProductIds");
					bool automaticallyAddRequiredProducts = GetValue<bool>(worksheet, iRow, "AutomaticallyAddRequiredProducts");
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

                    bool customerEntersPrice = GetValue<bool>(worksheet, iRow, "CustomerEntersPrice");
                    decimal minimumCustomerEnteredPrice = GetValue<decimal>(worksheet, iRow, "MinimumCustomerEnteredPrice");
                    decimal maximumCustomerEnteredPrice = GetValue<decimal>(worksheet, iRow, "MaximumCustomerEnteredPrice");
                    decimal weight = GetValue<decimal>(worksheet, iRow, "Weight");
                    decimal length = GetValue<decimal>(worksheet, iRow, "Length");
                    decimal width = GetValue<decimal>(worksheet, iRow, "Width");
                    decimal height = GetValue<decimal>(worksheet, iRow, "Height");

                    DateTime createdOnUtc = new DateTime();
                    var createdOnUtcExcel = GetValue<double>(worksheet, iRow, "CreatedOnUtc");
                    createdOnUtc = createdOnUtcExcel == 0 ? DateTime.UtcNow : DateTime.FromOADate(createdOnUtcExcel);
                    
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

					bool newProduct = false;
					Product product = null;

                    if (sku.HasValue())
                    {
                        product = _productService.GetProductBySku(sku);
                    }

                    if (product == null && gtin.HasValue())
                    {
                        product = _productService.GetProductByGtin(gtin);
                    }

					if (product == null)
					{
						product = new Product();
						newProduct = true;
					}

					product.ProductTypeId = productTypeId;
					product.ParentProductId = parentProductId;
					product.VisibleIndividually = visibleIndividually;
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
					product.Name = productVariantName;
					product.Sku = sku;
					product.ManufacturerPartNumber = manufacturerPartNumber;
					product.Gtin = gtin;
					product.IsGiftCard = isGiftCard;
					product.GiftCardTypeId = giftCardTypeId;
					product.RequireOtherProducts = requireOtherProducts;
					product.RequiredProductIds = requiredProductIds;
					product.AutomaticallyAddRequiredProducts = automaticallyAddRequiredProducts;
					product.IsDownload = isDownload;
					product.DownloadId = downloadId;
					product.UnlimitedDownloads = unlimitedDownloads;
					product.MaxNumberOfDownloads = maxNumberOfDownloads;
					product.DownloadActivationTypeId = downloadActivationTypeId;
					product.HasSampleDownload = hasSampleDownload;
					product.SampleDownloadId = sampleDownloadId;
					product.HasUserAgreement = hasUserAgreement;
					product.UserAgreementText = userAgreementText;
					product.IsRecurring = isRecurring;
					product.RecurringCycleLength = recurringCycleLength;
					product.RecurringCyclePeriodId = recurringCyclePeriodId;
					product.RecurringTotalCycles = recurringTotalCycles;
					product.IsShipEnabled = isShipEnabled;
					product.IsFreeShipping = isFreeShipping;
					product.AdditionalShippingCharge = additionalShippingCharge;
					product.IsTaxExempt = isTaxExempt;
					product.TaxCategoryId = taxCategoryId;
					product.ManageInventoryMethodId = manageInventoryMethodId;
					product.StockQuantity = stockQuantity;
					product.DisplayStockAvailability = displayStockAvailability;
					product.DisplayStockQuantity = displayStockQuantity;
					product.MinStockQuantity = minStockQuantity;
					product.LowStockActivityId = lowStockActivityId;
					product.NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow;
					product.BackorderModeId = backorderModeId;
					product.AllowBackInStockSubscriptions = allowBackInStockSubscriptions;
					product.OrderMinimumQuantity = orderMinimumQuantity;
					product.OrderMaximumQuantity = orderMaximumQuantity;
					product.AllowedQuantities = allowedQuantities;
					product.DisableBuyButton = disableBuyButton;
					product.DisableWishlistButton = disableWishlistButton;
					product.CallForPrice = callForPrice;
					product.Price = price;
					product.OldPrice = oldPrice;
					product.ProductCost = productCost;
					product.SpecialPrice = specialPrice;
					product.SpecialPriceStartDateTimeUtc = specialPriceStartDateTimeUtc;
					product.SpecialPriceEndDateTimeUtc = specialPriceEndDateTimeUtc;
					product.CustomerEntersPrice = customerEntersPrice;
					product.MinimumCustomerEnteredPrice = minimumCustomerEnteredPrice;
					product.MaximumCustomerEnteredPrice = maximumCustomerEnteredPrice;
					product.Weight = weight;
					product.Length = length;
					product.Width = width;
					product.Height = height;
					product.Published = published;
					product.CreatedOnUtc = createdOnUtc;
					product.UpdatedOnUtc = DateTime.UtcNow;

					// codehint: sm-add
					product.DeliveryTimeId = deliveryTimeId;
					product.BasePrice.Enabled = basePriceEnabled;
					product.BasePrice.MeasureUnit = basePriceMeasureUnit;
					product.BasePrice.Amount = basePriceAmount;
					product.BasePrice.BaseAmount = basePriceBaseAmount;

					if (newProduct)
					{
						_productService.InsertProduct(product);
					}
					else
					{
						_productService.UpdateProduct(product);
					}

					//search engine name
					_urlRecordService.SaveSlug(product, product.ValidateSeName(seName, product.Name, true), 0);



                    //category mappings
                    if (!String.IsNullOrEmpty(categoryIds))
                    {
                        foreach (var id in categoryIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
                        {
                            if (product.ProductCategories.Where(x => x.CategoryId == id).FirstOrDefault() == null)
                            {
                                //ensure that category exists
                                var category = _categoryService.GetCategoryById(id);
                                if (category != null)
                                {
                                    var productCategory = new ProductCategory()
                                    {
                                        ProductId = product.Id,
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
                            if (product.ProductManufacturers.Where(x => x.ManufacturerId == id).FirstOrDefault() == null)
                            {
                                //ensure that manufacturer exists
                                var manufacturer = _manufacturerService.GetManufacturerById(id);
                                if (manufacturer != null)
                                {
                                    var productManufacturer = new ProductManufacturer()
                                    {
                                        ProductId = product.Id,
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
                    bool hasNewPictures = false;
                    foreach (var picture in new string[] { picture1, picture2, picture3 })
                    {
                        if (String.IsNullOrEmpty(picture) || !File.Exists(picture))
                            continue;

                        var pictureBinary = FindEqualPicture(picture, product.ProductPictures);
                        if (pictureBinary != null && pictureBinary.Length > 0)
                        {
                            // no equal picture found in sequence
							product.ProductPictures.Add(new ProductPicture()
                            {
                                Picture = _pictureService.InsertPicture(pictureBinary, "image/jpeg", _pictureService.GetPictureSeName(name), true, true),
                                DisplayOrder = 1,
                            });
                            hasNewPictures = true;
                        }           
                    }

                    if (hasNewPictures)
                    {
                        _productService.UpdateProduct(product);
                    }

                    //update "HasTierPrices" and "HasDiscountsApplied" properties
                    _productService.UpdateHasTierPricesProperty(product);
                    _productService.UpdateHasDiscountsApplied(product);

                    //next product
                    iRow++;
                }
            }
        }

        /// <summary>
        /// Finds an equal picture by comparing the binary buffer
        /// </summary>
        /// <param name="path">The picture to find a duplicate for</param>
        /// <param name="productPictures">The sequence of product pictures to seek within for duplicates</param>
        /// <returns>The picture binary for <c>path</c> when no picture euqals in the sequence, <c>null</c> otherwise.</returns>
        private byte[] FindEqualPicture(string path, IEnumerable<ProductPicture> productPictures)
        {
            try
            {
                var myBuffer = File.ReadAllBytes(path);

                foreach (var pictureMap in productPictures.Where(x => x.Id > 0))
                {
                    var otherBuffer = _pictureService.LoadPictureBinary(pictureMap.Picture);
                    using (var myStream = new MemoryStream(myBuffer))
                    {
                        using (var otherStream = new MemoryStream(otherBuffer))
                        {
                            var equals = myStream.ContentsEqual(otherStream);
                            if (equals)
                            {
                                return null;
                            }
                        }
                    }
                }

                return myBuffer;
            }
            catch
            {
                return null;
            }
        }

    }
}
