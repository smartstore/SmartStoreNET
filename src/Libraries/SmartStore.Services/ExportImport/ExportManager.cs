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
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
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
        private readonly IProductService _productService;
		private readonly IProductTemplateService _productTemplateService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILanguageService _languageService;
		private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
			IProductTemplateService productTemplateService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILanguageService languageService,
			MediaSettings mediaSettings)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
			this._productTemplateService = productTemplateService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._languageService = languageService;
			this._mediaSettings = mediaSettings;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

        #endregion

        #region Utilities

        protected virtual void WriteCategories(XmlWriter xmlWriter, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            if (categories != null && categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    xmlWriter.WriteStartElement("Category");
                    xmlWriter.WriteElementString("Id", null, category.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, category.Name);
                    xmlWriter.WriteElementString("Description", null, category.Description);
                    xmlWriter.WriteElementString("CategoryTemplateId", null, category.CategoryTemplateId.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, category.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, category.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, category.MetaTitle);
                    xmlWriter.WriteElementString("SeName", null, category.GetSeName(0));
                    xmlWriter.WriteElementString("ParentCategoryId", null, category.ParentCategoryId.ToString());
                    xmlWriter.WriteElementString("PictureId", null, category.PictureId.ToString());
                    xmlWriter.WriteElementString("PageSize", null, category.PageSize.ToString());
                    xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, category.AllowCustomersToSelectPageSize.ToString());
                    xmlWriter.WriteElementString("PageSizeOptions", null, category.PageSizeOptions);
                    xmlWriter.WriteElementString("PriceRanges", null, category.PriceRanges);
                    xmlWriter.WriteElementString("ShowOnHomePage", null, category.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("Published", null, category.Published.ToString());
                    xmlWriter.WriteElementString("Deleted", null, category.Deleted.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, category.DisplayOrder.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, category.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, category.UpdatedOnUtc.ToString());


                    xmlWriter.WriteStartElement("Products");
                    var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, 0, int.MaxValue, true);
                    foreach (var productCategory in productCategories)
                    {
                        var product = productCategory.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productCategory.ProductId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SubCategories");
                    WriteCategories(xmlWriter, category.Id);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
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

                xmlWriter.WriteElementString("ManufacturerId", null, manufacturer.Id.ToString());
                xmlWriter.WriteElementString("Name", null, manufacturer.Name);
                xmlWriter.WriteElementString("Description", null, manufacturer.Description);
                xmlWriter.WriteElementString("ManufacturerTemplateId", null, manufacturer.ManufacturerTemplateId.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, manufacturer.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, manufacturer.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, manufacturer.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, manufacturer.GetSeName(0));
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
                            xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
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

			writer.WriteElementString("Id", null, product.Id.ToString());
			writer.WriteElementString("Name", null, product.Name.RemoveInvalidXmlChars());
			writer.WriteElementString("SEName", null, product.GetSeName(0, true, false));

			writer.WriteStartElement("ShortDescription");
			writer.WriteCData(product.ShortDescription.RemoveInvalidXmlChars());
			writer.WriteEndElement();

			writer.WriteStartElement("FullDescription");
			writer.WriteCData(product.FullDescription.RemoveInvalidXmlChars());
			writer.WriteEndElement();

			writer.WriteElementString("AdminComment", null, product.AdminComment.RemoveInvalidXmlChars());
			writer.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
			writer.WriteElementString("ProductTemplateViewPath", null, productTemplate == null ? "" : productTemplate.ViewPath);
			writer.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
			writer.WriteElementString("MetaKeywords", null, product.MetaKeywords.RemoveInvalidXmlChars());
			writer.WriteElementString("MetaDescription", null, product.MetaDescription.RemoveInvalidXmlChars());
			writer.WriteElementString("MetaTitle", null, product.MetaTitle.RemoveInvalidXmlChars());
			writer.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
			writer.WriteElementString("ApprovedRatingSum", null, product.ApprovedRatingSum.ToString());
			writer.WriteElementString("NotApprovedRatingSum", null, product.NotApprovedRatingSum.ToString());
			writer.WriteElementString("ApprovedTotalReviews", null, product.ApprovedTotalReviews.ToString());
			writer.WriteElementString("NotApprovedTotalReviews", null, product.NotApprovedTotalReviews.ToString());
			writer.WriteElementString("Published", null, product.Published.ToString());
			writer.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString(culture));
			writer.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString(culture));
			writer.WriteElementString("SubjectToAcl", null, product.SubjectToAcl.ToString());
			writer.WriteElementString("LimitedToStores", null, product.LimitedToStores.ToString());
			writer.WriteElementString("ProductTypeId", null, product.ProductTypeId.ToString());
			writer.WriteElementString("ParentGroupedProductId", null, product.ParentGroupedProductId.ToString());
			writer.WriteElementString("Sku", null, product.Sku);
			writer.WriteElementString("ManufacturerPartNumber", null, product.ManufacturerPartNumber);
			writer.WriteElementString("Gtin", null, product.Gtin);
			writer.WriteElementString("IsGiftCard", null, product.IsGiftCard.ToString());
			writer.WriteElementString("GiftCardTypeId", null, product.GiftCardTypeId.ToString());
			writer.WriteElementString("RequireOtherProducts", null, product.RequireOtherProducts.ToString());
			writer.WriteElementString("RequiredProductIds", null, product.RequiredProductIds);
			writer.WriteElementString("AutomaticallyAddRequiredProducts", null, product.AutomaticallyAddRequiredProducts.ToString());
			writer.WriteElementString("IsDownload", null, product.IsDownload.ToString());
			writer.WriteElementString("DownloadId", null, product.DownloadId.ToString());
			writer.WriteElementString("UnlimitedDownloads", null, product.UnlimitedDownloads.ToString());
			writer.WriteElementString("MaxNumberOfDownloads", null, product.MaxNumberOfDownloads.ToString());
			writer.WriteElementString("DownloadExpirationDays", null, product.DownloadExpirationDays.HasValue ? product.DownloadExpirationDays.ToString() : "");
			writer.WriteElementString("DownloadActivationType", null, product.DownloadActivationType.ToString());
			writer.WriteElementString("HasSampleDownload", null, product.HasSampleDownload.ToString());
			writer.WriteElementString("SampleDownloadId", null, product.SampleDownloadId.ToString());
			writer.WriteElementString("HasUserAgreement", null, product.HasUserAgreement.ToString());
			writer.WriteElementString("UserAgreementText", null, product.UserAgreementText.RemoveInvalidXmlChars());
			writer.WriteElementString("IsRecurring", null, product.IsRecurring.ToString());
			writer.WriteElementString("RecurringCycleLength", null, product.RecurringCycleLength.ToString());
			writer.WriteElementString("RecurringCyclePeriodId", null, product.RecurringCyclePeriodId.ToString());
			writer.WriteElementString("RecurringTotalCycles", null, product.RecurringTotalCycles.ToString());
			writer.WriteElementString("IsShipEnabled", null, product.IsShipEnabled.ToString());
			writer.WriteElementString("IsFreeShipping", null, product.IsFreeShipping.ToString());
			writer.WriteElementString("AdditionalShippingCharge", null, product.AdditionalShippingCharge.ToString(culture));
			writer.WriteElementString("IsTaxExempt", null, product.IsTaxExempt.ToString());
			writer.WriteElementString("TaxCategoryId", null, product.TaxCategoryId.ToString());
			writer.WriteElementString("ManageInventoryMethodId", null, product.ManageInventoryMethodId.ToString());
			writer.WriteElementString("StockQuantity", null, product.StockQuantity.ToString());
			writer.WriteElementString("DisplayStockAvailability", null, product.DisplayStockAvailability.ToString());
			writer.WriteElementString("DisplayStockQuantity", null, product.DisplayStockQuantity.ToString());
			writer.WriteElementString("MinStockQuantity", null, product.MinStockQuantity.ToString());
			writer.WriteElementString("LowStockActivityId", null, product.LowStockActivityId.ToString());
			writer.WriteElementString("NotifyAdminForQuantityBelow", null, product.NotifyAdminForQuantityBelow.ToString());
			writer.WriteElementString("BackorderModeId", null, product.BackorderModeId.ToString());
			writer.WriteElementString("AllowBackInStockSubscriptions", null, product.AllowBackInStockSubscriptions.ToString());
			writer.WriteElementString("OrderMinimumQuantity", null, product.OrderMinimumQuantity.ToString());
			writer.WriteElementString("OrderMaximumQuantity", null, product.OrderMaximumQuantity.ToString());
			writer.WriteElementString("AllowedQuantities", null, product.AllowedQuantities);
			writer.WriteElementString("DisableBuyButton", null, product.DisableBuyButton.ToString());
			writer.WriteElementString("DisableWishlistButton", null, product.DisableWishlistButton.ToString());
			writer.WriteElementString("AvailableForPreOrder", null, product.AvailableForPreOrder.ToString());
			writer.WriteElementString("CallForPrice", null, product.CallForPrice.ToString());
			writer.WriteElementString("Price", null, product.Price.ToString(culture));
			writer.WriteElementString("OldPrice", null, product.OldPrice.ToString(culture));
			writer.WriteElementString("ProductCost", null, product.ProductCost.ToString(culture));
			writer.WriteElementString("SpecialPrice", null, product.SpecialPrice.HasValue ? product.SpecialPrice.Value.ToString(culture) : "");
			writer.WriteElementString("SpecialPriceStartDateTimeUtc", null, product.SpecialPriceStartDateTimeUtc.HasValue ? product.SpecialPriceStartDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("SpecialPriceEndDateTimeUtc", null, product.SpecialPriceEndDateTimeUtc.HasValue ? product.SpecialPriceEndDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("CustomerEntersPrice", null, product.CustomerEntersPrice.ToString());
			writer.WriteElementString("MinimumCustomerEnteredPrice", null, product.MinimumCustomerEnteredPrice.ToString(culture));
			writer.WriteElementString("MaximumCustomerEnteredPrice", null, product.MaximumCustomerEnteredPrice.ToString(culture));
			writer.WriteElementString("HasTierPrices", null, product.HasTierPrices.ToString());
			writer.WriteElementString("HasDiscountsApplied", null, product.HasDiscountsApplied.ToString());
			writer.WriteElementString("Weight", null, product.Weight.ToString(culture));
			writer.WriteElementString("Length", null, product.Length.ToString(culture));
			writer.WriteElementString("Width", null, product.Width.ToString(culture));
			writer.WriteElementString("Height", null, product.Height.ToString(culture));
			writer.WriteElementString("AvailableStartDateTimeUtc", null, product.AvailableStartDateTimeUtc.HasValue ? product.AvailableStartDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("AvailableEndDateTimeUtc", null, product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value.ToString(culture) : "");
			writer.WriteElementString("DeliveryTimeId", null, product.DeliveryTimeId.HasValue ? product.DeliveryTimeId.Value.ToString() : "");
			writer.WriteElementString("BasePriceEnabled", null, product.BasePriceEnabled.ToString());
			writer.WriteElementString("BasePriceMeasureUnit", null, product.BasePriceMeasureUnit);
			writer.WriteElementString("BasePriceAmount", null, product.BasePriceAmount.HasValue ? product.BasePriceAmount.Value.ToString(culture) : "");
			writer.WriteElementString("BasePriceBaseAmount", null, product.BasePriceBaseAmount.HasValue ? product.BasePriceBaseAmount.Value.ToString() : "");
			writer.WriteElementString("VisibleIndividually", null, product.VisibleIndividually.ToString());
			writer.WriteElementString("DisplayOrder", null, product.DisplayOrder.ToString());
			writer.WriteElementString("BundleTitleText", null, product.BundleTitleText.RemoveInvalidXmlChars());
			writer.WriteElementString("BundlePerItemPricing", null, product.BundlePerItemPricing.ToString());
			writer.WriteElementString("BundlePerItemShipping", null, product.BundlePerItemShipping.ToString());
			writer.WriteElementString("BundlePerItemShoppingCart", null, product.BundlePerItemShoppingCart.ToString());
			writer.WriteElementString("LowestAttributeCombinationPrice", null, product.LowestAttributeCombinationPrice.HasValue ? product.LowestAttributeCombinationPrice.Value.ToString(culture) : "");
			writer.WriteElementString("IsEsd", null, product.IsEsd.ToString());

			writer.WriteStartElement("ProductDiscounts");
			foreach (var discount in product.AppliedDiscounts)
			{
				writer.WriteStartElement("ProductDiscount");
				writer.WriteElementString("DiscountId", null, discount.Id.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("TierPrices");
			foreach (var tierPrice in product.TierPrices)
			{
				writer.WriteStartElement("TierPrice");
				writer.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
				writer.WriteElementString("StoreId", null, tierPrice.StoreId.ToString());
				writer.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
				writer.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
				writer.WriteElementString("Price", null, tierPrice.Price.ToString(culture));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductAttributes");
			foreach (var attribute in product.ProductVariantAttributes)
			{
				writer.WriteStartElement("ProductVariantAttribute");
				writer.WriteElementString("ProductVariantAttributeId", null, attribute.Id.ToString());
				writer.WriteElementString("ProductAttributeId", null, attribute.ProductAttributeId.ToString());
				writer.WriteElementString("TextPrompt", null, attribute.TextPrompt.RemoveInvalidXmlChars());
				writer.WriteElementString("IsRequired", null, attribute.IsRequired.ToString());
				writer.WriteElementString("AttributeControlTypeId", null, attribute.AttributeControlTypeId.ToString());
				writer.WriteElementString("DisplayOrder", null, attribute.DisplayOrder.ToString());
				writer.WriteStartElement("ProductVariantAttributeValues");

				foreach (var attributeValue in attribute.ProductVariantAttributeValues)
				{
					writer.WriteStartElement("ProductVariantAttributeValue");
					writer.WriteElementString("ProductVariantAttributeValueId", null, attributeValue.Id.ToString());
					writer.WriteElementString("Name", null, attributeValue.Name.RemoveInvalidXmlChars());
					writer.WriteElementString("PriceAdjustment", null, attributeValue.PriceAdjustment.ToString(culture));
					writer.WriteElementString("WeightAdjustment", null, attributeValue.WeightAdjustment.ToString(culture));
					writer.WriteElementString("IsPreSelected", null, attributeValue.IsPreSelected.ToString());
					writer.WriteElementString("DisplayOrder", null, attributeValue.DisplayOrder.ToString());
					writer.WriteElementString("ValueTypeId", null, attributeValue.ValueTypeId.ToString());
					writer.WriteElementString("LinkedProductId", null, attributeValue.LinkedProductId.ToString());
					writer.WriteElementString("Quantity", null, attributeValue.Quantity.ToString());
					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
			writer.WriteEndElement();	// ProductAttributes

			writer.WriteStartElement("ProductVariantAttributeCombinations");
			foreach (var combination in product.ProductVariantAttributeCombinations)
			{
				writer.WriteStartElement("ProductVariantAttributeCombination");

				writer.WriteElementString("ProductVariantAttributeCombinationId", null, combination.Id.ToString());
				writer.WriteElementString("AllowOutOfStockOrders", null, combination.AllowOutOfStockOrders.ToString());
				writer.WriteElementString("StockQuantity", null, combination.StockQuantity.ToString());
				writer.WriteElementString("AssignedPictureIds", null, combination.AssignedPictureIds);

				writer.WriteStartElement("AttributesXml");
				writer.WriteCData(combination.AttributesXml);
				writer.WriteEndElement(); // AttributesXml

				writer.WriteElementString("IsActive", null, combination.IsActive.ToString());
				//xmlWriter.WriteElementString("IsDefaultCombination", null, combination.IsDefaultCombination.ToString());
				writer.WriteElementString("BasePriceAmount", null, combination.BasePriceAmount.HasValue ? combination.BasePriceAmount.Value.ToString(culture) : "");
				writer.WriteElementString("BasePriceBaseAmount", null, combination.BasePriceBaseAmount.HasValue ? combination.BasePriceBaseAmount.Value.ToString() : "");
				writer.WriteElementString("DeliveryTimeId", null, combination.DeliveryTimeId.HasValue ? combination.DeliveryTimeId.Value.ToString() : "");
				writer.WriteElementString("Length", null, combination.Length.HasValue ? combination.Length.Value.ToString(culture) : "");
				writer.WriteElementString("Width", null, combination.Width.HasValue ? combination.Width.Value.ToString(culture) : "");
				writer.WriteElementString("Height", null, combination.Height.HasValue ? combination.Height.Value.ToString(culture) : "");
				writer.WriteElementString("Height", null, combination.Height.HasValue ? combination.Height.Value.ToString(culture) : "");
				writer.WriteElementString("Gtin", null, combination.Gtin);
				writer.WriteElementString("Sku", null, combination.Sku);
				writer.WriteElementString("ManufacturerPartNumber", null, combination.ManufacturerPartNumber);
				writer.WriteElementString("Price", null, combination.Price.HasValue ? combination.Price.Value.ToString(culture) : "");

				writer.WriteEndElement();	// ProductVariantAttributeCombination
			}
			writer.WriteEndElement(); // ProductVariantAttributeCombinations

			writer.WriteStartElement("ProductPictures");
			foreach (var productPicture in product.ProductPictures)
			{
				writer.WriteStartElement("ProductPicture");
				writer.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
				writer.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());

				writer.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
				writer.WriteElementString("SeoFilename", null, productPicture.Picture.SeoFilename);
				writer.WriteElementString("MimeType", null, productPicture.Picture.MimeType);
				writer.WriteElementString("ThumbImageUrl", null, _pictureService.GetPictureUrl(productPicture.Picture, _mediaSettings.ProductThumbPictureSize, false));
				writer.WriteElementString("ImageUrl", null, _pictureService.GetPictureUrl(productPicture.Picture, _mediaSettings.ProductDetailsPictureSize, false));
				writer.WriteElementString("FullSizeImageUrl", null, _pictureService.GetPictureUrl(productPicture.Picture, 0, false));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductCategories");
			var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
			if (productCategories != null)
			{
				foreach (var productCategory in productCategories)
				{
					writer.WriteStartElement("ProductCategory");
					writer.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
					writer.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
					writer.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
					writer.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductManufacturers");
			var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
			if (productManufacturers != null)
			{
				foreach (var productManufacturer in productManufacturers)
				{
					writer.WriteStartElement("ProductManufacturer");
					writer.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
					writer.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
					writer.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
					writer.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductSpecificationAttributes");
			foreach (var productSpecificationAttribute in product.ProductSpecificationAttributes)
			{
				writer.WriteStartElement("ProductSpecificationAttribute");
				writer.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
				writer.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());
				writer.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
				writer.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
				writer.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ProductBundleItems");
			var bundleItems = _productService.GetBundleItems(product.Id, true);
			foreach (var bundleItem in bundleItems.Select(x => x.Item))
			{
				writer.WriteStartElement("ProductBundleItem");
				writer.WriteElementString("ProductId", null, bundleItem.ProductId.ToString());
				writer.WriteElementString("BundleProductId", null, bundleItem.BundleProductId.ToString());
				writer.WriteElementString("Quantity", null, bundleItem.Quantity.ToString());
				writer.WriteElementString("Discount", null, bundleItem.Discount.HasValue ? bundleItem.Discount.Value.ToString(culture) : "");
				writer.WriteElementString("DiscountPercentage", null, bundleItem.DiscountPercentage.ToString());
				writer.WriteElementString("Name", null, bundleItem.GetLocalizedName());
				writer.WriteElementString("ShortDescription", null, bundleItem.ShortDescription.RemoveInvalidXmlChars());
				writer.WriteElementString("FilterAttributes", null, bundleItem.FilterAttributes.ToString());
				writer.WriteElementString("HideThumbnail", null, bundleItem.HideThumbnail.ToString());
				writer.WriteElementString("Visible", null, bundleItem.Visible.ToString());
				writer.WriteElementString("Published", null, bundleItem.Published.ToString());
				writer.WriteElementString("DisplayOrder", null, bundleItem.DisplayOrder.ToString());
				writer.WriteElementString("CreatedOnUtc", null, bundleItem.CreatedOnUtc.ToString(culture));
				writer.WriteElementString("UpdatedOnUtc", null, bundleItem.UpdatedOnUtc.ToString(culture));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Localizations");
			foreach (var language in context.Languages)
			{
				writer.WriteStartElement("Localization");
				
				writer.WriteStartElement("Language");
				writer.WriteElementString("Id", null, language.Id.ToString());
				writer.WriteElementString("Name", null, language.Name);
				writer.WriteElementString("LanguageCulture", null, language.LanguageCulture);
				writer.WriteElementString("UniqueSeoCode", null, language.UniqueSeoCode);
				writer.WriteElementString("Published", null, language.Published.ToString());
				writer.WriteEndElement();

				writer.WriteStartElement("Product");
				writer.WriteElementString("Name", null, product.GetLocalized(x => x.Name, language.Id, false, false));
				writer.WriteElementString("ShortDescription", null, product.GetLocalized(x => x.ShortDescription, language.Id, false, false));
				writer.WriteElementString("FullDescription", null, product.GetLocalized(x => x.FullDescription, language.Id, false, false));
				writer.WriteElementString("MetaKeywords", null, product.GetLocalized(x => x.MetaKeywords, language.Id, false, false));
				writer.WriteElementString("MetaDescription", null, product.GetLocalized(x => x.MetaDescription, language.Id, false, false));
				writer.WriteElementString("MetaTitle", null, product.GetLocalized(x => x.MetaTitle, language.Id, false, false));
				writer.WriteElementString("SEName", null, product.GetSeName(language.Id, false, false));
				writer.WriteElementString("BundleTitleText", null, product.GetLocalized(x => x.BundleTitleText, language.Id, false, false));
				writer.WriteEndElement();

				writer.WriteEndElement();	// Localization
			}
			writer.WriteEndElement();		// Localizations
		}

        /// <summary>
        /// Export product list to XML
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportProductsToXml(IList<Product> products)
        {
			string result = ExportProductsToXml(writer =>
			{
				var context = new XmlExportContext()
				{
					ProductTemplates = _productTemplateService.GetAllProductTemplates(),
					Languages = _languageService.GetAllLanguages(true)
				};

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
			});

			return result;
        }

		/// <summary>
		/// Export product(s) to XML
		/// </summary>
		/// <param name="writeProducts">Action to export product entities</param>
		/// <param name="settings">XML writer settings</param>
		/// <returns>Result in XML format</returns>
		public virtual string ExportProductsToXml(Action<XmlWriter> writeProducts, XmlWriterSettings settings = null)
		{
			var sb = new StringBuilder();
			using (var stringWriter = new StringWriter(sb))
			using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement("Products");
				xmlWriter.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				writeProducts(xmlWriter);

				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
				xmlWriter.Close();

				return stringWriter.ToString();
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
					"BasePriceEnabled",
					"BasePriceMeasureUnit",
					"BasePriceAmount",
					"BasePriceBaseAmount",
					"BundleTitleText",
					"BundlePerItemShipping",
					"BundlePerItemPricing",
					"BundlePerItemShoppingCart",
					"BundleItemSkus"
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

			// EPPLus has serious memory leak problems.
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

                //codehint: sm-add begin
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitAccountHolder);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitAccountNumber);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitBankCode);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitBankName);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitBIC);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitCountry);
                xmlWriter.WriteElementString("DirectDebitAccountHolder", null, order.DirectDebitIban);
                //codehint: sm-add end

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
				xmlWriter.WriteElementString("RewardPointsRemaining", null, order.RewardPointsRemaining.HasValue ? order.RewardPointsRemaining.Value.ToString() : "");

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
                //Create Headers and format them
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
						"RewardPointsRemaining",
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

					worksheet.Cells[row, col].Value = (order.RewardPointsRemaining.HasValue ? order.RewardPointsRemaining.Value.ToString() : "");
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
                        "CustomerId",
                        "CustomerGuid",
                        "Email",
                        "Username",
                        "PasswordStr",//why can't we use 'Password' name?
                        "PasswordFormatId",
                        "PasswordSalt",
                        "LanguageId",
                        "CurrencyId",
                        "TaxDisplayTypeId",
                        "IsTaxExempt",
                        "VatNumber",
                        "VatNumberStatusId",
                        "TimeZoneId",
                        "AffiliateId",
                        "Active",
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

                    worksheet.Cells[row, col].Value = customer.IsTaxExempt;
                    col++;

					worksheet.Cells[row, col].Value = customer.AffiliateId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Active;
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

                    worksheet.Cells[row, col].Value = avatarPictureId;
                    col++;

					worksheet.Cells[row, col].Value = timeZoneId;
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
                xmlWriter.WriteElementString("CustomerId", null, customer.Id.ToString());
                xmlWriter.WriteElementString("CustomerGuid", null, customer.CustomerGuid.ToString());
                xmlWriter.WriteElementString("Email", null, customer.Email);
                xmlWriter.WriteElementString("Username", null, customer.Username);
                xmlWriter.WriteElementString("Password", null, customer.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, customer.PasswordFormatId.ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, customer.PasswordSalt);
                xmlWriter.WriteElementString("IsTaxExempt", null, customer.IsTaxExempt.ToString());
				xmlWriter.WriteElementString("AffiliateId", null, customer.AffiliateId.ToString());
                xmlWriter.WriteElementString("Active", null, customer.Active.ToString());


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

                xmlWriter.WriteEndElement();
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
	}
}
