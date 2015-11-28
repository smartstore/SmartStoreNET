using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
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
		private readonly IProductTemplateService _productTemplateService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILanguageService _languageService;
		private readonly MediaSettings _mediaSettings;
		private readonly ICommonServices _services;
        private readonly IStoreMappingService _storeMappingService;
        
        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
			IProductTemplateService productTemplateService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILanguageService languageService,
			MediaSettings mediaSettings,
			ICommonServices services,
            IStoreMappingService storeMappingService)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
			this._productTemplateService = productTemplateService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._languageService = languageService;
			this._mediaSettings = mediaSettings;
			this._services = services;
            this._storeMappingService = storeMappingService;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

        #endregion

        #region Utilities

		protected Action<XmlWriter, XmlExportContext, Action<Language>> WriteLocalized = (writer, context, content) =>
		{
			if (context.Languages.Count > 1)
			{
				writer.WriteStartElement("Localized");
				foreach (var language in context.Languages)
				{
					content(language);
				}
				writer.WriteEndElement();
			}
		};

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

		protected virtual void WritePicture(XmlWriter writer, XmlExportContext context, Picture picture, int thumbSize, int defaultSize)
		{
			if (picture != null)
			{
				writer.WriteStartElement("Picture");
				writer.Write("Id", picture.Id.ToString());
				writer.Write("SeoFileName", picture.SeoFilename);
				writer.Write("MimeType", picture.MimeType);
				writer.Write("ThumbImageUrl", _pictureService.GetPictureUrl(picture, thumbSize, false, context.Store.Url));
				writer.Write("ImageUrl", _pictureService.GetPictureUrl(picture, defaultSize, false, context.Store.Url));
				writer.Write("FullSizeImageUrl", _pictureService.GetPictureUrl(picture, 0, false, context.Store.Url));
				writer.WriteEndElement();
			}
		}

		protected virtual void WriteQuantityUnit(XmlWriter writer, XmlExportContext context, QuantityUnit quantityUnit)
		{
			if (quantityUnit != null)
			{
				writer.WriteStartElement("QuantityUnit");
				writer.Write("Id", quantityUnit.Id.ToString());
				writer.Write("Name", quantityUnit.Name);
				writer.Write("Description", quantityUnit.Description);
				writer.Write("DisplayLocale", quantityUnit.DisplayLocale);
				writer.Write("DisplayOrder", quantityUnit.DisplayOrder.ToString());
				writer.Write("IsDefault", quantityUnit.IsDefault.ToString());
				WriteLocalized(writer, context, lang =>
				{
					writer.Write("Name", quantityUnit.GetLocalized(x => x.Name, lang.Id, false, false), lang);
					writer.Write("Description", quantityUnit.GetLocalized(x => x.Description, lang.Id, false, false), lang);
				});
				writer.WriteEndElement();
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
		/// Writes a single product
		/// </summary>
		/// <param name="writer">The XML writer</param>
		/// <param name="product">The product</param>
		/// <param name="context">Context objects</param>
		public virtual void WriteProductToXml(XmlWriter writer, Product product, XmlExportContext context)
		{
			var culture = CultureInfo.InvariantCulture;
			var productTemplate = context.ProductTemplates.FirstOrDefault(x => x.Id == product.ProductTemplateId);

			writer.Write("Id", product.Id.ToString());
			writer.Write("Name", product.Name);
			writer.Write("SeName", product.GetSeName(0, true, false));

			writer.Write("ShortDescription", product.ShortDescription, null, true);
			writer.Write("FullDescription", product.FullDescription, null, true);

			writer.Write("AdminComment", product.AdminComment);
			writer.Write("ProductTemplateId", product.ProductTemplateId.ToString());
			writer.Write("ProductTemplateViewPath", productTemplate == null ? "" : productTemplate.ViewPath);
			writer.Write("ShowOnHomePage", product.ShowOnHomePage.ToString());
			writer.Write("HomePageDisplayOrder", product.HomePageDisplayOrder.ToString());
			writer.Write("MetaKeywords", product.MetaKeywords);
			writer.Write("MetaDescription", product.MetaDescription);
			writer.Write("MetaTitle", product.MetaTitle);
			writer.Write("AllowCustomerReviews", product.AllowCustomerReviews.ToString());
			writer.Write("ApprovedRatingSum", product.ApprovedRatingSum.ToString());
			writer.Write("NotApprovedRatingSum", product.NotApprovedRatingSum.ToString());
			writer.Write("ApprovedTotalReviews", product.ApprovedTotalReviews.ToString());
			writer.Write("NotApprovedTotalReviews", product.NotApprovedTotalReviews.ToString());
			writer.Write("Published", product.Published.ToString());
			writer.Write("CreatedOnUtc", product.CreatedOnUtc.ToString(culture));
			writer.Write("UpdatedOnUtc", product.UpdatedOnUtc.ToString(culture));
			writer.Write("SubjectToAcl", product.SubjectToAcl.ToString());
			writer.Write("LimitedToStores", product.LimitedToStores.ToString());
			writer.Write("ProductTypeId", product.ProductTypeId.ToString());
			writer.Write("ParentGroupedProductId", product.ParentGroupedProductId.ToString());
			writer.Write("Sku", product.Sku);
			writer.Write("ManufacturerPartNumber", product.ManufacturerPartNumber);
			writer.Write("Gtin", product.Gtin);
			writer.Write("IsGiftCard", product.IsGiftCard.ToString());
			writer.Write("GiftCardTypeId", product.GiftCardTypeId.ToString());
			writer.Write("RequireOtherProducts", product.RequireOtherProducts.ToString());
			writer.Write("RequiredProductIds", product.RequiredProductIds);
			writer.Write("AutomaticallyAddRequiredProducts", product.AutomaticallyAddRequiredProducts.ToString());
			writer.Write("IsDownload", product.IsDownload.ToString());
			writer.Write("DownloadId", product.DownloadId.ToString());
			writer.Write("UnlimitedDownloads", product.UnlimitedDownloads.ToString());
			writer.Write("MaxNumberOfDownloads", product.MaxNumberOfDownloads.ToString());
			writer.Write("DownloadExpirationDays", product.DownloadExpirationDays.HasValue ? product.DownloadExpirationDays.ToString() : "");
			writer.Write("DownloadActivationType", product.DownloadActivationType.ToString());
			writer.Write("HasSampleDownload", product.HasSampleDownload.ToString());
			writer.Write("SampleDownloadId", product.SampleDownloadId.ToString());
			writer.Write("HasUserAgreement", product.HasUserAgreement.ToString());
			writer.Write("UserAgreementText", product.UserAgreementText);
			writer.Write("IsRecurring", product.IsRecurring.ToString());
			writer.Write("RecurringCycleLength", product.RecurringCycleLength.ToString());
			writer.Write("RecurringCyclePeriodId", product.RecurringCyclePeriodId.ToString());
			writer.Write("RecurringTotalCycles", product.RecurringTotalCycles.ToString());
			writer.Write("IsShipEnabled", product.IsShipEnabled.ToString());
			writer.Write("IsFreeShipping", product.IsFreeShipping.ToString());
			writer.Write("AdditionalShippingCharge", product.AdditionalShippingCharge.ToString(culture));
			writer.Write("IsTaxExempt", product.IsTaxExempt.ToString());
			writer.Write("TaxCategoryId", product.TaxCategoryId.ToString());
			writer.Write("ManageInventoryMethodId", product.ManageInventoryMethodId.ToString());
			writer.Write("StockQuantity", product.StockQuantity.ToString());
			writer.Write("DisplayStockAvailability", product.DisplayStockAvailability.ToString());
			writer.Write("DisplayStockQuantity", product.DisplayStockQuantity.ToString());
			writer.Write("MinStockQuantity", product.MinStockQuantity.ToString());
			writer.Write("LowStockActivityId", product.LowStockActivityId.ToString());
			writer.Write("NotifyAdminForQuantityBelow", product.NotifyAdminForQuantityBelow.ToString());
			writer.Write("BackorderModeId", product.BackorderModeId.ToString());
			writer.Write("AllowBackInStockSubscriptions", product.AllowBackInStockSubscriptions.ToString());
			writer.Write("OrderMinimumQuantity", product.OrderMinimumQuantity.ToString());
			writer.Write("OrderMaximumQuantity", product.OrderMaximumQuantity.ToString());
			writer.Write("AllowedQuantities", product.AllowedQuantities);
			writer.Write("DisableBuyButton", product.DisableBuyButton.ToString());
			writer.Write("DisableWishlistButton", product.DisableWishlistButton.ToString());
			writer.Write("AvailableForPreOrder", product.AvailableForPreOrder.ToString());
			writer.Write("CallForPrice", product.CallForPrice.ToString());
			writer.Write("Price", product.Price.ToString(culture));
			writer.Write("OldPrice", product.OldPrice.ToString(culture));
			writer.Write("ProductCost", product.ProductCost.ToString(culture));
			writer.Write("SpecialPrice", product.SpecialPrice.HasValue ? product.SpecialPrice.Value.ToString(culture) : "");
			writer.Write("SpecialPriceStartDateTimeUtc", product.SpecialPriceStartDateTimeUtc.HasValue ? product.SpecialPriceStartDateTimeUtc.Value.ToString(culture) : "");
			writer.Write("SpecialPriceEndDateTimeUtc", product.SpecialPriceEndDateTimeUtc.HasValue ? product.SpecialPriceEndDateTimeUtc.Value.ToString(culture) : "");
			writer.Write("CustomerEntersPrice", product.CustomerEntersPrice.ToString());
			writer.Write("MinimumCustomerEnteredPrice", product.MinimumCustomerEnteredPrice.ToString(culture));
			writer.Write("MaximumCustomerEnteredPrice", product.MaximumCustomerEnteredPrice.ToString(culture));
			writer.Write("HasTierPrices", product.HasTierPrices.ToString());
			writer.Write("HasDiscountsApplied", product.HasDiscountsApplied.ToString());
			writer.Write("Weight", product.Weight.ToString(culture));
			writer.Write("Length", product.Length.ToString(culture));
			writer.Write("Width", product.Width.ToString(culture));
			writer.Write("Height", product.Height.ToString(culture));
			writer.Write("AvailableStartDateTimeUtc", product.AvailableStartDateTimeUtc.HasValue ? product.AvailableStartDateTimeUtc.Value.ToString(culture) : "");
			writer.Write("AvailableEndDateTimeUtc", product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value.ToString(culture) : "");
			writer.Write("BasePriceEnabled", product.BasePriceEnabled.ToString());
			writer.Write("BasePriceMeasureUnit", product.BasePriceMeasureUnit);
			writer.Write("BasePriceAmount", product.BasePriceAmount.HasValue ? product.BasePriceAmount.Value.ToString(culture) : "");
			writer.Write("BasePriceBaseAmount", product.BasePriceBaseAmount.HasValue ? product.BasePriceBaseAmount.Value.ToString() : "");
			writer.Write("VisibleIndividually", product.VisibleIndividually.ToString());
			writer.Write("DisplayOrder", product.DisplayOrder.ToString());
			writer.Write("BundleTitleText", product.BundleTitleText);
			writer.Write("BundlePerItemPricing", product.BundlePerItemPricing.ToString());
			writer.Write("BundlePerItemShipping", product.BundlePerItemShipping.ToString());
			writer.Write("BundlePerItemShoppingCart", product.BundlePerItemShoppingCart.ToString());
			writer.Write("LowestAttributeCombinationPrice", product.LowestAttributeCombinationPrice.HasValue ? product.LowestAttributeCombinationPrice.Value.ToString(culture) : "");
			writer.Write("IsEsd", product.IsEsd.ToString());

			WriteLocalized(writer, context, lang =>
			{
				writer.Write("Name", product.GetLocalized(x => x.Name, lang.Id, false, false), lang);
				writer.Write("SeName", product.GetSeName(lang.Id, false, false), lang);
				writer.Write("ShortDescription", product.GetLocalized(x => x.ShortDescription, lang.Id, false, false), lang, true);
				writer.Write("FullDescription", product.GetLocalized(x => x.FullDescription, lang.Id, false, false), lang, true);
				writer.Write("MetaKeywords", product.GetLocalized(x => x.MetaKeywords, lang.Id, false, false), lang);
				writer.Write("MetaDescription", product.GetLocalized(x => x.MetaDescription, lang.Id, false, false), lang);
				writer.Write("MetaTitle", product.GetLocalized(x => x.MetaTitle, lang.Id, false, false), lang);
				writer.Write("BundleTitleText", product.GetLocalized(x => x.BundleTitleText, lang.Id, false, false), lang);
			});

			if (product.DeliveryTime != null)
			{
				writer.WriteStartElement("DeliveryTime");
				writer.Write("Id", product.DeliveryTime.Id.ToString());
				writer.Write("Name", product.DeliveryTime.Name);
				writer.Write("DisplayLocale", product.DeliveryTime.DisplayLocale);
				writer.Write("ColorHexValue", product.DeliveryTime.ColorHexValue);
				writer.Write("DisplayOrder", product.DeliveryTime.DisplayOrder.ToString());
				WriteLocalized(writer, context, lang =>
				{
					writer.Write("Name", product.DeliveryTime.GetLocalized(x => x.Name, lang.Id, false, false), lang);
				});
				writer.WriteEndElement();
			}

			WriteQuantityUnit(writer, context, product.QuantityUnit);

			writer.WriteStartElement("ProductTags");
			foreach (var tag in product.ProductTags)
			{
				writer.WriteStartElement("ProductTag");
				writer.Write("Id", tag.Id.ToString());
				writer.Write("Name", tag.Name);

				WriteLocalized(writer, context, lang =>
				{
					writer.Write("Name", tag.GetLocalized(x => x.Name, lang.Id, false, false), lang);
				});

				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductDiscounts");
			foreach (var discount in product.AppliedDiscounts)
			{
				writer.WriteStartElement("ProductDiscount");
				writer.Write("DiscountId", discount.Id.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("TierPrices");
			foreach (var tierPrice in product.TierPrices)
			{
				writer.WriteStartElement("TierPrice");
				writer.Write("Id", tierPrice.Id.ToString());
				writer.Write("StoreId", tierPrice.StoreId.ToString());
				writer.Write("CustomerRoleId", tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
				writer.Write("Quantity", tierPrice.Quantity.ToString());
				writer.Write("Price", tierPrice.Price.ToString(culture));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductAttributes");
			foreach (var pva in product.ProductVariantAttributes.OrderBy(x => x.DisplayOrder))
			{
				writer.WriteStartElement("ProductAttribute");

				writer.Write("Id", pva.Id.ToString());
				writer.Write("TextPrompt", pva.TextPrompt);
				writer.Write("IsRequired", pva.IsRequired.ToString());
				writer.Write("AttributeControlTypeId", pva.AttributeControlTypeId.ToString());
				writer.Write("DisplayOrder", pva.DisplayOrder.ToString());

				writer.WriteStartElement("Attribute");
				writer.Write("Id", pva.ProductAttribute.Id.ToString());
				writer.Write("Alias", pva.ProductAttribute.Alias);
				writer.Write("Name", pva.ProductAttribute.Name);
				writer.Write("Description", pva.ProductAttribute.Description);
				WriteLocalized(writer, context, lang =>
				{
					writer.Write("Name", pva.ProductAttribute.GetLocalized(x => x.Name, lang.Id, false, false), lang);
					writer.Write("Description", pva.ProductAttribute.GetLocalized(x => x.Description, lang.Id, false, false), lang);
				});
				writer.WriteEndElement();	// Attribute

				writer.WriteStartElement("AttributeValues");
				foreach (var value in pva.ProductVariantAttributeValues.OrderBy(x => x.DisplayOrder))
				{
					writer.WriteStartElement("AttributeValue");
					writer.Write("Id", value.Id.ToString());
					writer.Write("Alias", value.Alias);
					writer.Write("Name", value.Name);
					writer.Write("ColorSquaresRgb", value.ColorSquaresRgb);
					writer.Write("PriceAdjustment", value.PriceAdjustment.ToString(culture));
					writer.Write("WeightAdjustment", value.WeightAdjustment.ToString(culture));
					writer.Write("IsPreSelected", value.IsPreSelected.ToString());
					writer.Write("DisplayOrder", value.DisplayOrder.ToString());
					writer.Write("ValueTypeId", value.ValueTypeId.ToString());
					writer.Write("LinkedProductId", value.LinkedProductId.ToString());
					writer.Write("Quantity", value.Quantity.ToString());
					WriteLocalized(writer, context, lang =>
					{
						writer.Write("Name", value.GetLocalized(x => x.Name, lang.Id, false, false), lang);
					});
					writer.WriteEndElement();	// AttributeValue
				}
				writer.WriteEndElement();	// AttributeValues

				writer.WriteEndElement();	// ProductAttribute
			}
			writer.WriteEndElement();	// ProductAttributes

			writer.WriteStartElement("ProductAttributeCombinations");
			foreach (var combination in product.ProductVariantAttributeCombinations)
			{
				writer.WriteStartElement("ProductAttributeCombination");

				writer.Write("Id", combination.Id.ToString());
				writer.Write("StockQuantity", combination.StockQuantity.ToString());
				writer.Write("AllowOutOfStockOrders", combination.AllowOutOfStockOrders.ToString());
				writer.Write("AttributesXml", combination.AttributesXml, null, true);
				writer.Write("Sku", combination.Sku);
				writer.Write("Gtin", combination.Gtin);
				writer.Write("ManufacturerPartNumber", combination.ManufacturerPartNumber);
				writer.Write("Price", combination.Price.HasValue ? combination.Price.Value.ToString(culture) : "");
				writer.Write("Length", combination.Length.HasValue ? combination.Length.Value.ToString(culture) : "");
				writer.Write("Width", combination.Width.HasValue ? combination.Width.Value.ToString(culture) : "");
				writer.Write("Height", combination.Height.HasValue ? combination.Height.Value.ToString(culture) : "");
				writer.Write("BasePriceAmount", combination.BasePriceAmount.HasValue ? combination.BasePriceAmount.Value.ToString(culture) : "");
				writer.Write("BasePriceBaseAmount", combination.BasePriceBaseAmount.HasValue ? combination.BasePriceBaseAmount.Value.ToString() : "");
				writer.Write("DeliveryTimeId", combination.DeliveryTimeId.HasValue ? combination.DeliveryTimeId.Value.ToString() : "");
				writer.Write("IsActive", combination.IsActive.ToString());

				WriteQuantityUnit(writer, context, combination.QuantityUnit);

				writer.WriteStartElement("Pictures");
				foreach (int pictureId in combination.GetAssignedPictureIds())
				{
					WritePicture(writer, context, _pictureService.GetPictureById(pictureId), _mediaSettings.ProductThumbPictureSize, _mediaSettings.ProductDetailsPictureSize);
				}
				writer.WriteEndElement();	// Pictures

				writer.WriteEndElement();	// ProductAttributeCombination
			}
			writer.WriteEndElement(); // ProductAttributeCombinations

			writer.WriteStartElement("ProductPictures");
			foreach (var productPicture in product.ProductPictures.OrderBy(x => x.DisplayOrder))
			{
				writer.WriteStartElement("ProductPicture");
				writer.Write("Id", productPicture.Id.ToString());
				writer.Write("DisplayOrder", productPicture.DisplayOrder.ToString());

				WritePicture(writer, context, productPicture.Picture, _mediaSettings.ProductThumbPictureSize, _mediaSettings.ProductDetailsPictureSize);

				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductCategories");
			var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
			if (productCategories != null)
			{
				foreach (var productCategory in productCategories.OrderBy(x => x.DisplayOrder))
				{
					var category = productCategory.Category;
					writer.WriteStartElement("ProductCategory");
					writer.Write("IsFeaturedProduct", productCategory.IsFeaturedProduct.ToString());
					writer.Write("DisplayOrder", productCategory.DisplayOrder.ToString());
					
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
					writer.Write("SeName", category.GetSeName(0));
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
					writer.Write("CreatedOnUtc", category.CreatedOnUtc.ToString(culture));
					writer.Write("UpdatedOnUtc", category.UpdatedOnUtc.ToString(culture));
					writer.Write("SubjectToAcl", category.SubjectToAcl.ToString());
					writer.Write("LimitedToStores", category.LimitedToStores.ToString());
					writer.Write("Alias", category.Alias);
					writer.Write("DefaultViewMode", category.DefaultViewMode);

					WritePicture(writer, context, category.Picture, _mediaSettings.CategoryThumbPictureSize, _mediaSettings.CategoryThumbPictureSize);

					WriteLocalized(writer, context, lang =>
					{
						writer.Write("Name", category.GetLocalized(x => x.Name, lang.Id, false, false), lang);
						writer.Write("FullName", category.GetLocalized(x => x.FullName, lang.Id, false, false), lang);
						writer.Write("Description", category.GetLocalized(x => x.Description, lang.Id, false, false), lang);
						writer.Write("BottomDescription", category.GetLocalized(x => x.BottomDescription, lang.Id, false, false), lang);
						writer.Write("MetaKeywords", category.GetLocalized(x => x.MetaKeywords, lang.Id, false, false), lang);
						writer.Write("MetaDescription", category.GetLocalized(x => x.MetaDescription, lang.Id, false, false), lang);
						writer.Write("MetaTitle", category.GetLocalized(x => x.MetaTitle, lang.Id, false, false), lang);
						writer.Write("SeName", category.GetSeName(lang.Id, false, false));
					});

					writer.WriteEndElement();
					
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductManufacturers");
			var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
			if (productManufacturers != null)
			{
				foreach (var productManufacturer in productManufacturers.OrderBy(x => x.DisplayOrder))
				{
					var manu = productManufacturer.Manufacturer;
					writer.WriteStartElement("ProductManufacturer");

					writer.Write("Id", productManufacturer.Id.ToString());
					writer.Write("IsFeaturedProduct", productManufacturer.IsFeaturedProduct.ToString());
					writer.Write("DisplayOrder", productManufacturer.DisplayOrder.ToString());

					writer.WriteStartElement("Manufacturer");
					writer.Write("Id", manu.Id.ToString());
					writer.Write("Name", manu.Name);
					writer.Write("SeName", manu.GetSeName(0, true, false));
					writer.Write("Description", manu.Description);
					writer.Write("MetaKeywords", manu.MetaKeywords);
					writer.Write("MetaDescription", manu.MetaDescription);
					writer.Write("MetaTitle", manu.MetaTitle);

					WritePicture(writer, context, manu.Picture, _mediaSettings.ManufacturerThumbPictureSize, _mediaSettings.ManufacturerThumbPictureSize);

					WriteLocalized(writer, context, lang =>
					{
						writer.Write("Name", manu.GetLocalized(x => x.Name, lang.Id, false, false), lang);
						writer.Write("SeName", manu.GetSeName(lang.Id, false, false), lang);
						writer.Write("Description", manu.GetLocalized(x => x.Description, lang.Id, false, false), lang);
						writer.Write("MetaKeywords", manu.GetLocalized(x => x.MetaKeywords, lang.Id, false, false), lang);
						writer.Write("MetaDescription", manu.GetLocalized(x => x.MetaDescription, lang.Id, false, false), lang);
						writer.Write("MetaTitle", manu.GetLocalized(x => x.MetaTitle, lang.Id, false, false), lang);
					});

					writer.WriteEndElement();

					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductSpecificationAttributes");
			foreach (var pca in product.ProductSpecificationAttributes.OrderBy(x => x.DisplayOrder))
			{
				writer.WriteStartElement("ProductSpecificationAttribute");
				writer.Write("Id", pca.Id.ToString());
				writer.Write("AllowFiltering", pca.AllowFiltering.ToString());
				writer.Write("ShowOnProductPage", pca.ShowOnProductPage.ToString());
				writer.Write("DisplayOrder", pca.DisplayOrder.ToString());

				writer.WriteStartElement("SpecificationAttributeOption");
				writer.Write("Id", pca.SpecificationAttributeOption.Id.ToString());
				writer.Write("DisplayOrder", pca.SpecificationAttributeOption.DisplayOrder.ToString());
				writer.Write("Name", pca.SpecificationAttributeOption.Name);
				WriteLocalized(writer, context, lang =>
				{
					writer.Write("Name", pca.SpecificationAttributeOption.GetLocalized(x => x.Name, lang.Id, false, false), lang);
				});

				writer.WriteStartElement("SpecificationAttribute");
				writer.Write("Id", pca.SpecificationAttributeOption.SpecificationAttribute.Id.ToString());
				writer.Write("DisplayOrder", pca.SpecificationAttributeOption.SpecificationAttribute.DisplayOrder.ToString());
				writer.Write("Name", pca.SpecificationAttributeOption.SpecificationAttribute.Name);
				WriteLocalized(writer, context, lang =>
				{
					writer.Write("Name", pca.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name, lang.Id, false, false), lang);
				});
				writer.WriteEndElement();	// SpecificationAttribute

				writer.WriteEndElement();	// SpecificationAttributeOption

				writer.WriteEndElement();	// ProductSpecificationAttribute
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductBundleItems");
			var bundleItems = _productService.GetBundleItems(product.Id, true);
			foreach (var bundleItem in bundleItems.Select(x => x.Item).OrderBy(x => x.DisplayOrder))
			{
				writer.WriteStartElement("ProductBundleItem");
				writer.Write("ProductId", bundleItem.ProductId.ToString());
				writer.Write("BundleProductId", bundleItem.BundleProductId.ToString());
				writer.Write("Quantity", bundleItem.Quantity.ToString());
				writer.Write("Discount", bundleItem.Discount.HasValue ? bundleItem.Discount.Value.ToString(culture) : "");
				writer.Write("DiscountPercentage", bundleItem.DiscountPercentage.ToString());
				writer.Write("Name", bundleItem.GetLocalizedName());
				writer.Write("ShortDescription", bundleItem.ShortDescription);
				writer.Write("FilterAttributes", bundleItem.FilterAttributes.ToString());
				writer.Write("HideThumbnail", bundleItem.HideThumbnail.ToString());
				writer.Write("Visible", bundleItem.Visible.ToString());
				writer.Write("Published", bundleItem.Published.ToString());
				writer.Write("DisplayOrder", bundleItem.DisplayOrder.ToString());
				writer.Write("CreatedOnUtc", bundleItem.CreatedOnUtc.ToString(culture));
				writer.Write("UpdatedOnUtc", bundleItem.UpdatedOnUtc.ToString(culture));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Export product list to XML
		/// </summary>
		/// <param name="stream">Stream to write</param>
		/// <param name="searchContext">Search context</param>
		public virtual void ExportProductsToXml(Stream stream, ProductSearchContext searchContext)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = new UTF8Encoding(false),
				CheckCharacters = false
			};

			var context = new XmlExportContext
			{
				ProductTemplates = _productTemplateService.GetAllProductTemplates(),
				Languages = _languageService.GetAllLanguages(true),
				Store = _services.StoreContext.CurrentStore
			};

			using (var writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("Products");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				for (int i = 0; i < 9999999; ++i)
				{
					searchContext.PageIndex = i;

					var products = _productService.SearchProducts(searchContext);

					foreach (var product in products)
					{
						writer.WriteStartElement("Product");

						try
						{
							WriteProductToXml(writer, product, context);
						}
						catch (Exception exc)
						{
							Logger.Error("{0} (Product.Id {1})".FormatWith(exc.Message, product.Id), exc);
						}

						writer.WriteEndElement();		// Product
					}

					if (!products.HasNextPage)
						break;
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				writer.Close();

				stream.Seek(0, SeekOrigin.Begin);
			}
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
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportOrdersToXml(IList<Order> orders)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Orders");
            xmlWriter.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);


            foreach (var order in orders)
            {
                xmlWriter.WriteStartElement("Order");

                xmlWriter.WriteElementString("OrderId", null, order.GetOrderNumber());
                xmlWriter.WriteElementString("OrderGuid", null, order.OrderGuid.ToString());
				xmlWriter.WriteElementString("StoreId", null, order.StoreId.ToString());
                xmlWriter.WriteElementString("CustomerId", null, order.CustomerId.ToString());
                xmlWriter.WriteElementString("CustomerLanguageId", null, order.CustomerLanguageId.ToString());
                xmlWriter.WriteElementString("CustomerTaxDisplayTypeId", null, order.CustomerTaxDisplayTypeId.ToString());
                xmlWriter.WriteElementString("CustomerIp", null, order.CustomerIp);
                xmlWriter.WriteElementString("OrderSubtotalInclTax", null, order.OrderSubtotalInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubtotalExclTax", null, order.OrderSubtotalExclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountInclTax", null, order.OrderSubTotalDiscountInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountExclTax", null, order.OrderSubTotalDiscountExclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingInclTax", null, order.OrderShippingInclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingExclTax", null, order.OrderShippingExclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeInclTax", null, order.PaymentMethodAdditionalFeeInclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeExclTax", null, order.PaymentMethodAdditionalFeeExclTax.ToString());
                xmlWriter.WriteElementString("TaxRates", null, order.TaxRates);
                xmlWriter.WriteElementString("OrderTax", null, order.OrderTax.ToString());
                xmlWriter.WriteElementString("OrderTotal", null, order.OrderTotal.ToString());
                xmlWriter.WriteElementString("RefundedAmount", null, order.RefundedAmount.ToString());
                xmlWriter.WriteElementString("OrderDiscount", null, order.OrderDiscount.ToString());
                xmlWriter.WriteElementString("CurrencyRate", null, order.CurrencyRate.ToString());
                xmlWriter.WriteElementString("CustomerCurrencyCode", null, order.CustomerCurrencyCode);
                xmlWriter.WriteElementString("AffiliateId", null, order.AffiliateId.ToString());
                xmlWriter.WriteElementString("OrderStatusId", null, order.OrderStatusId.ToString());
                xmlWriter.WriteElementString("AllowStoringCreditCardNumber", null, order.AllowStoringCreditCardNumber.ToString());
                xmlWriter.WriteElementString("CardType", null, order.CardType);
                xmlWriter.WriteElementString("CardName", null, order.CardName);
                xmlWriter.WriteElementString("CardNumber", null, order.CardNumber);
                xmlWriter.WriteElementString("MaskedCreditCardNumber", null, order.MaskedCreditCardNumber);
                xmlWriter.WriteElementString("CardCvv2", null, order.CardCvv2);
                xmlWriter.WriteElementString("CardExpirationMonth", null, order.CardExpirationMonth);
                xmlWriter.WriteElementString("CardExpirationYear", null, order.CardExpirationYear);

                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitAccountHolder);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitAccountNumber);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitBankCode);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitBankName);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitBIC);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitCountry);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitIban);

                xmlWriter.WriteElementString("PaymentMethodSystemName", null, order.PaymentMethodSystemName);
                xmlWriter.WriteElementString("AuthorizationTransactionId", null, order.AuthorizationTransactionId);
                xmlWriter.WriteElementString("AuthorizationTransactionCode", null, order.AuthorizationTransactionCode);
                xmlWriter.WriteElementString("AuthorizationTransactionResult", null, order.AuthorizationTransactionResult);
                xmlWriter.WriteElementString("CaptureTransactionId", null, order.CaptureTransactionId);
                xmlWriter.WriteElementString("CaptureTransactionResult", null, order.CaptureTransactionResult);
                xmlWriter.WriteElementString("SubscriptionTransactionId", null, order.SubscriptionTransactionId);
                xmlWriter.WriteElementString("PurchaseOrderNumber", null, order.PurchaseOrderNumber);
                xmlWriter.WriteElementString("PaymentStatusId", null, order.PaymentStatusId.ToString());
                xmlWriter.WriteElementString("PaidDateUtc", null, (order.PaidDateUtc == null) ? string.Empty : order.PaidDateUtc.Value.ToString());
                xmlWriter.WriteElementString("ShippingStatusId", null, order.ShippingStatusId.ToString());
                xmlWriter.WriteElementString("ShippingMethod", null, order.ShippingMethod);
                xmlWriter.WriteElementString("ShippingRateComputationMethodSystemName", null, order.ShippingRateComputationMethodSystemName);
                xmlWriter.WriteElementString("VatNumber", null, order.VatNumber);
                xmlWriter.WriteElementString("Deleted", null, order.Deleted.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, order.CreatedOnUtc.ToString());
				xmlWriter.WriteElementString("UpdatedOnUtc", null, order.UpdatedOnUtc.ToString());
                xmlWriter.WriteElementString("RewardPointsUsed", null, order.RedeemedRewardPointsEntry != null && order.RedeemedRewardPointsEntry.Points != 0 ? (order.RedeemedRewardPointsEntry.Points * (-1)).ToString() : "");
                var remainingRewardPoints = order.Customer.GetRewardPointsBalance();
                xmlWriter.WriteElementString("RewardPointsRemaining", null, remainingRewardPoints > 0 ? remainingRewardPoints.ToString() : "");
				xmlWriter.WriteElementString("HasNewPaymentNotification", null, order.HasNewPaymentNotification.ToString());

                //products
                var orderItems = order.OrderItems;
                if (orderItems.Count > 0)
                {
					xmlWriter.WriteStartElement("OrderItems");
                    foreach (var orderItem in orderItems)
                    {
						xmlWriter.WriteStartElement("OrderItem");
                        xmlWriter.WriteElementString("Id", null, orderItem.Id.ToString());
						xmlWriter.WriteElementString("OrderItemGuid", null, orderItem.OrderItemGuid.ToString());
                        xmlWriter.WriteElementString("ProductId", null, orderItem.ProductId.ToString());

						xmlWriter.WriteElementString("ProductName", null, orderItem.Product.Name);
                        xmlWriter.WriteElementString("UnitPriceInclTax", null, orderItem.UnitPriceInclTax.ToString());
                        xmlWriter.WriteElementString("UnitPriceExclTax", null, orderItem.UnitPriceExclTax.ToString());
                        xmlWriter.WriteElementString("PriceInclTax", null, orderItem.PriceInclTax.ToString());
                        xmlWriter.WriteElementString("PriceExclTax", null, orderItem.PriceExclTax.ToString());
                        xmlWriter.WriteElementString("AttributeDescription", null, orderItem.AttributeDescription);
                        xmlWriter.WriteElementString("AttributesXml", null, orderItem.AttributesXml);
                        xmlWriter.WriteElementString("Quantity", null, orderItem.Quantity.ToString());
                        xmlWriter.WriteElementString("DiscountAmountInclTax", null, orderItem.DiscountAmountInclTax.ToString());
                        xmlWriter.WriteElementString("DiscountAmountExclTax", null, orderItem.DiscountAmountExclTax.ToString());
                        xmlWriter.WriteElementString("DownloadCount", null, orderItem.DownloadCount.ToString());
                        xmlWriter.WriteElementString("IsDownloadActivated", null, orderItem.IsDownloadActivated.ToString());
                        xmlWriter.WriteElementString("LicenseDownloadId", null, orderItem.LicenseDownloadId.ToString());
						xmlWriter.WriteElementString("BundleData", null, orderItem.BundleData);
						xmlWriter.WriteElementString("ProductCost", null, orderItem.ProductCost.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }

                //shipments
                var shipments = order.Shipments.OrderBy(x => x.CreatedOnUtc).ToList();
                if (shipments.Count > 0)
                {
                    xmlWriter.WriteStartElement("Shipments");
                    foreach (var shipment in shipments)
                    {
                        xmlWriter.WriteStartElement("Shipment");
                        xmlWriter.WriteElementString("ShipmentId", null, shipment.Id.ToString());
                        xmlWriter.WriteElementString("TrackingNumber", null, shipment.TrackingNumber);
                        xmlWriter.WriteElementString("TotalWeight", null, shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value.ToString() : "");

                        xmlWriter.WriteElementString("ShippedDateUtc", null, shipment.ShippedDateUtc.HasValue ?
                            shipment.ShippedDateUtc.ToString() : "");
                        xmlWriter.WriteElementString("DeliveryDateUtc", null, shipment.DeliveryDateUtc.HasValue ?
                            shipment.DeliveryDateUtc.Value.ToString() : "");
                        xmlWriter.WriteElementString("CreatedOnUtc", null, shipment.CreatedOnUtc.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        public virtual void ExportOrdersToXlsx(Stream stream, IList<Order> orders)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Orders");
                // Create Headers and format them
                var properties = new string[]
                    {
                        //order properties
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
                        "ShippingFaxNumber",
                    };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var order in orders)
                {
                    int col = 1;

                    //order properties
                    worksheet.Cells[row, col].Value = order.GetOrderNumber();
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderGuid;
                    col++;

                    worksheet.Cells[row, col].Value = order.CustomerId;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubtotalInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubtotalExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubTotalDiscountInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubTotalDiscountExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderShippingInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderShippingExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentMethodAdditionalFeeInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentMethodAdditionalFeeExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.TaxRates;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderTotal;
                    col++;

                    worksheet.Cells[row, col].Value = order.RefundedAmount;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderDiscount;
                    col++;

                    worksheet.Cells[row, col].Value = order.CurrencyRate;
                    col++;

                    worksheet.Cells[row, col].Value = order.CustomerCurrencyCode;
                    col++;

					worksheet.Cells[row, col].Value = order.AffiliateId;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentMethodSystemName;
                    col++;

                    worksheet.Cells[row, col].Value = order.PurchaseOrderNumber;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingMethod;
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingRateComputationMethodSystemName;
                    col++;

                    worksheet.Cells[row, col].Value = order.VatNumber;
                    col++;

                    worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToOADate();
                    col++;

					worksheet.Cells[row, col].Value = order.UpdatedOnUtc.ToOADate();
					col++;

                    worksheet.Cells[row, col].Value = order.RedeemedRewardPointsEntry != null ? (order.RedeemedRewardPointsEntry.Points != 0 ? (order.RedeemedRewardPointsEntry.Points * (-1)).ToString() : "") : "";
                    col++;

                    var remainingRewardPoints = order.Customer.GetRewardPointsBalance();
                    worksheet.Cells[row, col].Value = (remainingRewardPoints > 0 ? remainingRewardPoints.ToString() : "");
					col++;

					worksheet.Cells[row, col].Value = order.HasNewPaymentNotification;
					col++;


                    //billing address
                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.FirstName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.LastName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Email : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Company : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null && order.BillingAddress.Country != null ? order.BillingAddress.Country.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null && order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.City : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Address1 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Address2 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.ZipPostalCode : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.PhoneNumber : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.FaxNumber : "";
                    col++;

                    //shipping address
                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.FirstName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.LastName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Email : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Company : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null && order.ShippingAddress.Country != null ? order.ShippingAddress.Country.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.City : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Address1 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Address2 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : "";
                    col++;

                    //next row
                    row++;
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


	public class XmlExportContext
	{
		public IList<ProductTemplate> ProductTemplates { get; set; }
		public IList<Language> Languages { get; set; }
		public Store Store { get; set; }
	}
}
