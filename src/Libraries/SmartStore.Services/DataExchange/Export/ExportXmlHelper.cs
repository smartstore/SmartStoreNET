using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.DataExchange.Export
{
	public class ExportXmlHelper : IDisposable
	{
		protected XmlWriter _writer;
		protected CultureInfo _culture;
		protected bool _doNotDispose;

		public ExportXmlHelper(XmlWriter writer, bool doNotDispose = false, CultureInfo culture = null)
		{
			_writer = writer;
			_doNotDispose = doNotDispose;
			_culture = (culture == null ? CultureInfo.InvariantCulture : culture);
		}
		public ExportXmlHelper(Stream stream, XmlWriterSettings settings = null, CultureInfo culture = null)
		{
			if (settings == null)
			{
				settings = DefaultSettings;
			}

			_writer = XmlWriter.Create(stream, settings);			
			_culture = (culture == null ? CultureInfo.InvariantCulture : culture);
		}

		public static XmlWriterSettings DefaultSettings
		{
			get
			{
				return new XmlWriterSettings
				{
					Encoding = Encoding.UTF8,
					CheckCharacters = false,
					Indent = true,
					IndentChars = "\t"
				};
			}
		}

		public ExportXmlExclude Exclude { get; set; }

		public XmlWriter Writer => _writer;

		public void Dispose()
		{
			if (_writer != null && !_doNotDispose)
			{
				_writer.Dispose();
			}
		}

		public void WriteLocalized(dynamic parentNode)
		{
			if (parentNode == null || parentNode._Localized == null)
				return;

			_writer.WriteStartElement("Localized");
			foreach (dynamic item in parentNode._Localized)
			{
				_writer.WriteStartElement((string)item.LocaleKey);
				_writer.WriteAttributeString("culture", (string)item.Culture);
				_writer.WriteString(((string)item.LocaleValue).RemoveInvalidXmlChars());
				_writer.WriteEndElement();	// item.LocaleKey
			}
			_writer.WriteEndElement();	// Localized
		}

		public void WriteGenericAttributes(dynamic parentNode)
		{
			if (parentNode == null || parentNode._GenericAttributes == null)
				return;

			_writer.WriteStartElement("GenericAttributes");
			foreach (dynamic genericAttribute in parentNode._GenericAttributes)
			{
				GenericAttribute entity = genericAttribute.Entity;

				_writer.WriteStartElement("GenericAttribute");
				_writer.Write("Id", entity.Id.ToString());
				_writer.Write("EntityId", entity.EntityId.ToString());
				_writer.Write("KeyGroup", entity.KeyGroup);
				_writer.Write("Key", entity.Key);
				_writer.Write("Value", (string)genericAttribute.Value);
				_writer.Write("StoreId", entity.StoreId.ToString());
				_writer.WriteEndElement();	// GenericAttribute
			}
			_writer.WriteEndElement();	// GenericAttributes
		}

		public void WriteAddress(dynamic address, string node)
		{
			if (address == null)
				return;

			Address entity = address.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Salutation", entity.Salutation);
			_writer.Write("Title", entity.Title);
			_writer.Write("FirstName", entity.FirstName);
			_writer.Write("LastName", entity.LastName);
			_writer.Write("Email", entity.Email);
			_writer.Write("Company", entity.Company);
			_writer.Write("CountryId", entity.CountryId.HasValue ? entity.CountryId.Value.ToString() : "");
			_writer.Write("StateProvinceId", entity.StateProvinceId.HasValue ? entity.StateProvinceId.Value.ToString() : "");
			_writer.Write("City", entity.City);
			_writer.Write("Address1", entity.Address1);
			_writer.Write("Address2", entity.Address2);
			_writer.Write("ZipPostalCode", entity.ZipPostalCode);
			_writer.Write("PhoneNumber", entity.PhoneNumber);
			_writer.Write("FaxNumber", entity.FaxNumber);
			_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));

			if (address.Country != null)
			{
				dynamic country = address.Country;
				Country entityCountry = address.Country.Entity;

				_writer.WriteStartElement("Country");
				_writer.Write("Id", entityCountry.Id.ToString());
				_writer.Write("Name", (string)country.Name);
				_writer.Write("AllowsBilling", entityCountry.AllowsBilling.ToString());
				_writer.Write("AllowsShipping", entityCountry.AllowsShipping.ToString());
				_writer.Write("TwoLetterIsoCode", entityCountry.TwoLetterIsoCode);
				_writer.Write("ThreeLetterIsoCode", entityCountry.ThreeLetterIsoCode);
				_writer.Write("NumericIsoCode", entityCountry.NumericIsoCode.ToString());
				_writer.Write("SubjectToVat", entityCountry.SubjectToVat.ToString());
				_writer.Write("Published", entityCountry.Published.ToString());
				_writer.Write("DisplayOrder", entityCountry.DisplayOrder.ToString());
				_writer.Write("LimitedToStores", entityCountry.LimitedToStores.ToString());

				WriteLocalized(country);

				_writer.WriteEndElement();	// Country
			}

			if (address.StateProvince != null)
			{
				dynamic stateProvince = address.StateProvince;
				StateProvince entityStateProvince = address.StateProvince.Entity;

				_writer.WriteStartElement("StateProvince");
				_writer.Write("Id", entityStateProvince.Id.ToString());
				_writer.Write("CountryId", entityStateProvince.CountryId.ToString());
				_writer.Write("Name", (string)stateProvince.Name);
				_writer.Write("Abbreviation", (string)stateProvince.Abbreviation);
				_writer.Write("Published", entityStateProvince.Published.ToString());
				_writer.Write("DisplayOrder", entityStateProvince.DisplayOrder.ToString());

				WriteLocalized(stateProvince);

				_writer.WriteEndElement();	// StateProvince
			}

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteCurrency(dynamic currency, string node)
		{
			if (currency == null)
				return;

			Currency entity = currency.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Name", (string)currency.Name);
			_writer.Write("CurrencyCode", entity.CurrencyCode);
			_writer.Write("Rate", entity.Rate.ToString(_culture));
			_writer.Write("DisplayLocale", entity.DisplayLocale);
			_writer.Write("CustomFormatting", entity.CustomFormatting);
			_writer.Write("LimitedToStores", entity.LimitedToStores.ToString());
			_writer.Write("Published", entity.Published.ToString());
			_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
			_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
			_writer.Write("UpdatedOnUtc", entity.UpdatedOnUtc.ToString(_culture));
			_writer.Write("DomainEndings", entity.DomainEndings);
            _writer.Write("RoundOrderItemsEnabled", entity.RoundOrderItemsEnabled.ToString());
            _writer.Write("RoundNumDecimals", entity.RoundNumDecimals.ToString());
            _writer.Write("RoundOrderTotalEnabled", entity.RoundOrderTotalEnabled.ToString());
            _writer.Write("RoundOrderTotalDenominator", entity.RoundOrderTotalDenominator.ToString(_culture));
            _writer.Write("RoundOrderTotalRule", ((int)entity.RoundOrderTotalRule).ToString());

            WriteLocalized(currency);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteCountry(dynamic country, string node)
		{
			if (country == null)
				return;

			Country entity = country.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Name", entity.Name);
			_writer.Write("AllowsBilling", entity.AllowsBilling.ToString());
			_writer.Write("AllowsShipping", entity.AllowsShipping.ToString());
			_writer.Write("TwoLetterIsoCode", entity.TwoLetterIsoCode);
			_writer.Write("ThreeLetterIsoCode", entity.ThreeLetterIsoCode);
			_writer.Write("NumericIsoCode", entity.NumericIsoCode.ToString());
			_writer.Write("SubjectToVat", entity.SubjectToVat.ToString());
			_writer.Write("Published", entity.Published.ToString());
			_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
			_writer.Write("LimitedToStores", entity.LimitedToStores.ToString());

			WriteLocalized(country);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteRewardPointsHistory(dynamic rewardPoints, string node)
		{
			if (rewardPoints == null)
				return;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			foreach (dynamic rewardPoint in rewardPoints)
			{
				RewardPointsHistory entity = rewardPoint.Entity;

				_writer.WriteStartElement("RewardPointsHistory");
				_writer.Write("Id", entity.ToString());
				_writer.Write("CustomerId", entity.ToString());
				_writer.Write("Points", entity.Points.ToString());
				_writer.Write("PointsBalance", entity.PointsBalance.ToString());
				_writer.Write("UsedAmount", entity.UsedAmount.ToString(_culture));
				_writer.Write("Message", (string)rewardPoint.Message);
				_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
				_writer.WriteEndElement();	// RewardPointsHistory
			}

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteDeliveryTime(dynamic deliveryTime, string node)
		{
			if (deliveryTime == null)
				return;

			DeliveryTime entity = deliveryTime.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Name", (string)deliveryTime.Name);
			_writer.Write("DisplayLocale", entity.DisplayLocale);
			_writer.Write("ColorHexValue", entity.ColorHexValue);
			_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
			_writer.Write("IsDefault", entity.IsDefault.ToString());

			WriteLocalized(deliveryTime);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteQuantityUnit(dynamic quantityUnit, string node)
		{
			if (quantityUnit == null)
				return;

			QuantityUnit entity = quantityUnit.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Name", (string)quantityUnit.Name);
			_writer.Write("Description", (string)quantityUnit.Description);
			_writer.Write("DisplayLocale", entity.DisplayLocale);
			_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
			_writer.Write("IsDefault", entity.IsDefault.ToString());

			WriteLocalized(quantityUnit);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WritePicture(dynamic picture, string node)
		{
			if (picture == null)
				return;

			Picture entity = picture.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("SeoFilename", (string)picture.SeoFilename);
			_writer.Write("MimeType", (string)picture.MimeType);
			_writer.Write("ThumbImageUrl", (string)picture._ThumbImageUrl);
			_writer.Write("ImageUrl", (string)picture._ImageUrl);
			_writer.Write("FullSizeImageUrl", (string)picture._FullSizeImageUrl);
			_writer.Write("FileName", (string)picture._FileName);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteCategory(dynamic category, string node)
		{
			if (category == null)
				return;

			Category entity = category.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());

			if (!Exclude.HasFlag(ExportXmlExclude.Category))
			{
				_writer.Write("Name", (string)category.Name);
				_writer.Write("FullName", (string)category.FullName);
				_writer.Write("Description", (string)category.Description);
				_writer.Write("BottomDescription", (string)category.BottomDescription);
				_writer.Write("CategoryTemplateId", entity.CategoryTemplateId.ToString());
				_writer.Write("CategoryTemplateViewPath", (string)category._CategoryTemplateViewPath);
				_writer.Write("MetaKeywords", (string)category.MetaKeywords);
				_writer.Write("MetaDescription", (string)category.MetaDescription);
				_writer.Write("MetaTitle", (string)category.MetaTitle);
				_writer.Write("SeName", (string)category.SeName);
				_writer.Write("ParentCategoryId", entity.ParentCategoryId.ToString());
				_writer.Write("PictureId", entity.PictureId.ToString());
				_writer.Write("PageSize", entity.PageSize.ToString());
				_writer.Write("AllowCustomersToSelectPageSize", entity.AllowCustomersToSelectPageSize.ToString());
				_writer.Write("PageSizeOptions", entity.PageSizeOptions);
				_writer.Write("ShowOnHomePage", entity.ShowOnHomePage.ToString());
				_writer.Write("HasDiscountsApplied", entity.HasDiscountsApplied.ToString());
				_writer.Write("Published", entity.Published.ToString());
				_writer.Write("Deleted", entity.Deleted.ToString());
				_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
				_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
				_writer.Write("UpdatedOnUtc", entity.UpdatedOnUtc.ToString(_culture));
				_writer.Write("SubjectToAcl", entity.SubjectToAcl.ToString());
				_writer.Write("LimitedToStores", entity.LimitedToStores.ToString());
				_writer.Write("Alias", (string)category.Alias);
				_writer.Write("DefaultViewMode", entity.DefaultViewMode);

				WritePicture(category.Picture, "Picture");

				WriteLocalized(category);
			}

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteManufacturer(dynamic manufacturer, string node)
		{
			if (manufacturer == null)
				return;

			Manufacturer entity = manufacturer.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Name", (string)manufacturer.Name);
			_writer.Write("SeName", (string)manufacturer.SeName);
			_writer.Write("Description", (string)manufacturer.Description);
			_writer.Write("ManufacturerTemplateId", entity.ManufacturerTemplateId.ToString());
			_writer.Write("MetaKeywords", (string)manufacturer.MetaKeywords);
			_writer.Write("MetaDescription", (string)manufacturer.MetaDescription);
			_writer.Write("MetaTitle", (string)manufacturer.MetaTitle);
			_writer.Write("PictureId", entity.PictureId.ToString());
			_writer.Write("PageSize", entity.PageSize.ToString());
			_writer.Write("AllowCustomersToSelectPageSize", entity.AllowCustomersToSelectPageSize.ToString());
			_writer.Write("PageSizeOptions", entity.PageSizeOptions);
			_writer.Write("Published", entity.Published.ToString());
			_writer.Write("Deleted", entity.Deleted.ToString());
			_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
			_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
			_writer.Write("UpdatedOnUtc", entity.UpdatedOnUtc.ToString(_culture));
			_writer.Write("HasDiscountsApplied", entity.HasDiscountsApplied.ToString());

			WritePicture(manufacturer.Picture, "Picture");

			WriteLocalized(manufacturer);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteProduct(dynamic product, string node)
		{
			if (product == null)
				return;

			Product entity = product.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			decimal? basePriceAmount = product.BasePriceAmount;
			int? basePriceBaseAmount = product.BasePriceBaseAmount;
			decimal? lowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice;

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("Name", (string)product.Name);
			_writer.Write("SeName", (string)product.SeName);
			_writer.Write("ShortDescription", (string)product.ShortDescription);
			_writer.Write("FullDescription", (string)product.FullDescription);
			_writer.Write("AdminComment", (string)product.AdminComment);
			_writer.Write("ProductTemplateId", entity.ProductTemplateId.ToString());
			_writer.Write("ProductTemplateViewPath", (string)product._ProductTemplateViewPath);
			_writer.Write("ShowOnHomePage", entity.ShowOnHomePage.ToString());
			_writer.Write("HomePageDisplayOrder", entity.HomePageDisplayOrder.ToString());
			_writer.Write("MetaKeywords", (string)product.MetaKeywords);
			_writer.Write("MetaDescription", (string)product.MetaDescription);
			_writer.Write("MetaTitle", (string)product.MetaTitle);
			_writer.Write("AllowCustomerReviews", entity.AllowCustomerReviews.ToString());
			_writer.Write("ApprovedRatingSum", entity.ApprovedRatingSum.ToString());
			_writer.Write("NotApprovedRatingSum", entity.NotApprovedRatingSum.ToString());
			_writer.Write("ApprovedTotalReviews", entity.ApprovedTotalReviews.ToString());
			_writer.Write("NotApprovedTotalReviews", entity.NotApprovedTotalReviews.ToString());
			_writer.Write("Published", entity.Published.ToString());
			_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
			_writer.Write("UpdatedOnUtc", entity.UpdatedOnUtc.ToString(_culture));
			_writer.Write("SubjectToAcl", entity.SubjectToAcl.ToString());
			_writer.Write("LimitedToStores", entity.LimitedToStores.ToString());
			_writer.Write("ProductTypeId", entity.ProductTypeId.ToString());
			_writer.Write("ParentGroupedProductId", entity.ParentGroupedProductId.ToString());
			_writer.Write("Sku", (string)product.Sku);
			_writer.Write("ManufacturerPartNumber", (string)product.ManufacturerPartNumber);
			_writer.Write("Gtin", (string)product.Gtin);
			_writer.Write("IsGiftCard", entity.IsGiftCard.ToString());
			_writer.Write("GiftCardTypeId", entity.GiftCardTypeId.ToString());
			_writer.Write("RequireOtherProducts", entity.RequireOtherProducts.ToString());
			_writer.Write("RequiredProductIds", entity.RequiredProductIds);
			_writer.Write("AutomaticallyAddRequiredProducts", entity.AutomaticallyAddRequiredProducts.ToString());
			_writer.Write("IsDownload", entity.IsDownload.ToString());
			_writer.Write("DownloadId", entity.DownloadId.ToString());
			_writer.Write("UnlimitedDownloads", entity.UnlimitedDownloads.ToString());
			_writer.Write("MaxNumberOfDownloads", entity.MaxNumberOfDownloads.ToString());
			_writer.Write("DownloadExpirationDays", entity.DownloadExpirationDays.HasValue ? entity.DownloadExpirationDays.Value.ToString() : "");
			_writer.Write("DownloadActivationTypeId", entity.DownloadActivationTypeId.ToString());
			_writer.Write("HasSampleDownload", entity.HasSampleDownload.ToString());
			_writer.Write("SampleDownloadId", entity.SampleDownloadId.HasValue ? entity.SampleDownloadId.Value.ToString() : "");
			_writer.Write("HasUserAgreement", entity.HasUserAgreement.ToString());
			_writer.Write("UserAgreementText", entity.UserAgreementText);
			_writer.Write("IsRecurring", entity.IsRecurring.ToString());
			_writer.Write("RecurringCycleLength", entity.RecurringCycleLength.ToString());
			_writer.Write("RecurringCyclePeriodId", entity.RecurringCyclePeriodId.ToString());
			_writer.Write("RecurringTotalCycles", entity.RecurringTotalCycles.ToString());
			_writer.Write("IsShipEnabled", entity.IsShipEnabled.ToString());
			_writer.Write("IsFreeShipping", entity.IsFreeShipping.ToString());
			_writer.Write("AdditionalShippingCharge", entity.AdditionalShippingCharge.ToString(_culture));
			_writer.Write("IsTaxExempt", entity.IsTaxExempt.ToString());
			_writer.Write("TaxCategoryId", entity.TaxCategoryId.ToString());
			_writer.Write("ManageInventoryMethodId", entity.ManageInventoryMethodId.ToString());
			_writer.Write("StockQuantity", entity.StockQuantity.ToString());
			_writer.Write("DisplayStockAvailability", entity.DisplayStockAvailability.ToString());
			_writer.Write("DisplayStockQuantity", entity.DisplayStockQuantity.ToString());
			_writer.Write("MinStockQuantity", entity.MinStockQuantity.ToString());
			_writer.Write("LowStockActivityId", entity.LowStockActivityId.ToString());
			_writer.Write("NotifyAdminForQuantityBelow", entity.NotifyAdminForQuantityBelow.ToString());
			_writer.Write("BackorderModeId", entity.BackorderModeId.ToString());
			_writer.Write("AllowBackInStockSubscriptions", entity.AllowBackInStockSubscriptions.ToString());
			_writer.Write("OrderMinimumQuantity", entity.OrderMinimumQuantity.ToString());
			_writer.Write("OrderMaximumQuantity", entity.OrderMaximumQuantity.ToString());
			_writer.Write("QuantityStep", entity.QuantityStep.ToString());
			_writer.Write("QuantiyControlType", ((int)entity.QuantiyControlType).ToString());
			_writer.Write("HideQuantityControl", entity.HideQuantityControl.ToString());
            _writer.Write("AllowedQuantities", entity.AllowedQuantities);
			_writer.Write("DisableBuyButton", entity.DisableBuyButton.ToString());
			_writer.Write("DisableWishlistButton", entity.DisableWishlistButton.ToString());
			_writer.Write("AvailableForPreOrder", entity.AvailableForPreOrder.ToString());
			_writer.Write("CallForPrice", entity.CallForPrice.ToString());
			_writer.Write("Price", entity.Price.ToString(_culture));
			_writer.Write("OldPrice", entity.OldPrice.ToString(_culture));
			_writer.Write("ProductCost", entity.ProductCost.ToString(_culture));
			_writer.Write("SpecialPrice", entity.SpecialPrice.HasValue ? entity.SpecialPrice.Value.ToString(_culture) : "");
			_writer.Write("SpecialPriceStartDateTimeUtc", entity.SpecialPriceStartDateTimeUtc.HasValue ? entity.SpecialPriceStartDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("SpecialPriceEndDateTimeUtc", entity.SpecialPriceEndDateTimeUtc.HasValue ? entity.SpecialPriceEndDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("CustomerEntersPrice", entity.CustomerEntersPrice.ToString());
			_writer.Write("MinimumCustomerEnteredPrice", entity.MinimumCustomerEnteredPrice.ToString(_culture));
			_writer.Write("MaximumCustomerEnteredPrice", entity.MaximumCustomerEnteredPrice.ToString(_culture));
			_writer.Write("HasTierPrices", entity.HasTierPrices.ToString());
			_writer.Write("HasDiscountsApplied", entity.HasDiscountsApplied.ToString());
			_writer.Write("MainPictureId", entity.MainPictureId.HasValue ? entity.MainPictureId.Value.ToString() : "");
			_writer.Write("Weight", ((decimal)product.Weight).ToString(_culture));
			_writer.Write("Length", ((decimal)product.Length).ToString(_culture));
			_writer.Write("Width", ((decimal)product.Width).ToString(_culture));
			_writer.Write("Height", ((decimal)product.Height).ToString(_culture));
			_writer.Write("AvailableStartDateTimeUtc", entity.AvailableStartDateTimeUtc.HasValue ? entity.AvailableStartDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("AvailableEndDateTimeUtc", entity.AvailableEndDateTimeUtc.HasValue ? entity.AvailableEndDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("BasePriceEnabled", ((bool)product.BasePriceEnabled).ToString());
			_writer.Write("BasePriceMeasureUnit", (string)product.BasePriceMeasureUnit);
			_writer.Write("BasePriceAmount", basePriceAmount.HasValue ? basePriceAmount.Value.ToString(_culture) : "");
			_writer.Write("BasePriceBaseAmount", basePriceBaseAmount.HasValue ? basePriceBaseAmount.Value.ToString() : "");
			_writer.Write("BasePriceHasValue", ((bool)product.BasePriceHasValue).ToString());
			_writer.Write("BasePriceInfo", (string)product._BasePriceInfo);
			_writer.Write("VisibleIndividually", entity.VisibleIndividually.ToString());
			_writer.Write("DisplayOrder", entity.DisplayOrder.ToString());
			_writer.Write("IsSystemProduct", entity.IsSystemProduct.ToString());
			_writer.Write("BundleTitleText", entity.BundleTitleText);
			_writer.Write("BundlePerItemPricing", entity.BundlePerItemPricing.ToString());
			_writer.Write("BundlePerItemShipping", entity.BundlePerItemShipping.ToString());
			_writer.Write("BundlePerItemShoppingCart", entity.BundlePerItemShoppingCart.ToString());
			_writer.Write("LowestAttributeCombinationPrice", lowestAttributeCombinationPrice.HasValue ? lowestAttributeCombinationPrice.Value.ToString(_culture) : "");
			_writer.Write("IsEsd", entity.IsEsd.ToString());
			_writer.Write("CustomsTariffNumber", entity.CustomsTariffNumber);

			WriteLocalized(product);

			WriteDeliveryTime(product.DeliveryTime, "DeliveryTime");

			WriteQuantityUnit(product.QuantityUnit, "QuantityUnit");

			WriteCountry(product.CountryOfOrigin, "CountryOfOrigin");

			if (product.AppliedDiscounts != null)
			{
				_writer.WriteStartElement("AppliedDiscounts");
				foreach (dynamic discount in product.AppliedDiscounts)
				{
					Discount entityDiscount = discount.Entity;

					_writer.WriteStartElement("AppliedDiscount");
					_writer.Write("Id", entityDiscount.Id.ToString());
					_writer.Write("Name", (string)discount.Name);
					_writer.Write("DiscountTypeId", entityDiscount.DiscountTypeId.ToString());
					_writer.Write("UsePercentage", entityDiscount.UsePercentage.ToString());
					_writer.Write("DiscountPercentage", entityDiscount.DiscountPercentage.ToString(_culture));
					_writer.Write("DiscountAmount", entityDiscount.DiscountAmount.ToString(_culture));
					_writer.Write("StartDateUtc", entityDiscount.StartDateUtc.HasValue ? entityDiscount.StartDateUtc.Value.ToString(_culture) : "");
					_writer.Write("EndDateUtc", entityDiscount.EndDateUtc.HasValue ? entityDiscount.EndDateUtc.Value.ToString(_culture) : "");
					_writer.Write("RequiresCouponCode", entityDiscount.RequiresCouponCode.ToString());
					_writer.Write("CouponCode", entityDiscount.CouponCode);
					_writer.Write("DiscountLimitationId", entityDiscount.DiscountLimitationId.ToString());
					_writer.Write("LimitationTimes", entityDiscount.LimitationTimes.ToString());
					_writer.WriteEndElement();	// AppliedDiscount
				}
				_writer.WriteEndElement();	// AppliedDiscounts
			}

			if (product.TierPrices != null)
			{
				_writer.WriteStartElement("TierPrices");
				foreach (dynamic tierPrice in product.TierPrices)
				{
					TierPrice entityTierPrice = tierPrice.Entity;

					_writer.WriteStartElement("TierPrice");
					_writer.Write("Id", entityTierPrice.Id.ToString());
					_writer.Write("ProductId", entityTierPrice.ProductId.ToString());
					_writer.Write("StoreId", entityTierPrice.StoreId.ToString());
					_writer.Write("CustomerRoleId", entityTierPrice.CustomerRoleId.HasValue ? entityTierPrice.CustomerRoleId.Value.ToString() : "");
					_writer.Write("Quantity", entityTierPrice.Quantity.ToString());
					_writer.Write("Price", entityTierPrice.Price.ToString(_culture));
                    _writer.Write("CalculationMethod", ((int)entityTierPrice.CalculationMethod).ToString());
                    _writer.WriteEndElement();	// TierPrice
				}
				_writer.WriteEndElement();	// TierPrices
			}

			if (product.ProductTags != null)
			{
				_writer.WriteStartElement("ProductTags");
				foreach (dynamic tag in product.ProductTags)
				{
					_writer.WriteStartElement("ProductTag");
					_writer.Write("Id", ((int)tag.Id).ToString());
					_writer.Write("Name", (string)tag.Name);
					_writer.Write("SeName", (string)tag.SeName);

					WriteLocalized(tag);

					_writer.WriteEndElement();	// ProductTag
				}
				_writer.WriteEndElement();	// ProductTags
			}

			if (product.ProductAttributes != null)
			{
				_writer.WriteStartElement("ProductAttributes");
				foreach (dynamic pva in product.ProductAttributes)
				{
					ProductVariantAttribute entityPva = pva.Entity;
                    ProductAttribute entityPa = pva.Attribute.Entity;

                    _writer.WriteStartElement("ProductAttribute");
					_writer.Write("Id", entityPva.Id.ToString());
					_writer.Write("TextPrompt", (string)pva.TextPrompt);
					_writer.Write("IsRequired", entityPva.IsRequired.ToString());
					_writer.Write("AttributeControlTypeId", entityPva.AttributeControlTypeId.ToString());
					_writer.Write("DisplayOrder", entityPva.DisplayOrder.ToString());

					_writer.WriteStartElement("Attribute");
					_writer.Write("Id", entityPa.Id.ToString());
					_writer.Write("Alias", entityPa.Alias);
					_writer.Write("Name", entityPa.Name);
					_writer.Write("Description", entityPa.Description);
                    _writer.Write("AllowFiltering", entityPa.AllowFiltering.ToString());
                    _writer.Write("DisplayOrder", entityPa.DisplayOrder.ToString());
                    _writer.Write("FacetTemplateHint", ((int)entityPa.FacetTemplateHint).ToString());
                    _writer.Write("IndexOptionNames", entityPa.IndexOptionNames.ToString());

                    WriteLocalized(pva.Attribute);

					_writer.WriteEndElement();	// Attribute

					_writer.WriteStartElement("AttributeValues");
					foreach (dynamic value in pva.Attribute.Values)
					{
						ProductVariantAttributeValue entityPvav = value.Entity;

						_writer.WriteStartElement("AttributeValue");
						_writer.Write("Id", entityPvav.Id.ToString());
						_writer.Write("Alias", (string)value.Alias);
						_writer.Write("Name", (string)value.Name);
						_writer.Write("Color", (string)value.Color);
						_writer.Write("PriceAdjustment", ((decimal)value.PriceAdjustment).ToString(_culture));
						_writer.Write("WeightAdjustment", ((decimal)value.WeightAdjustment).ToString(_culture));
						_writer.Write("IsPreSelected", entityPvav.IsPreSelected.ToString());
						_writer.Write("DisplayOrder", entityPvav.DisplayOrder.ToString());
						_writer.Write("ValueTypeId", entityPvav.ValueTypeId.ToString());
						_writer.Write("LinkedProductId", entityPvav.LinkedProductId.ToString());
						_writer.Write("Quantity", entityPvav.Quantity.ToString());

						WriteLocalized(value);

						_writer.WriteEndElement();	// AttributeValue
					}
					_writer.WriteEndElement();	// AttributeValues

					_writer.WriteEndElement();	// ProductAttribute
				}
				_writer.WriteEndElement();	// ProductAttributes
			}

			if (product.ProductAttributeCombinations != null)
			{
				_writer.WriteStartElement("ProductAttributeCombinations");
				foreach (dynamic combination in product.ProductAttributeCombinations)
				{
					ProductVariantAttributeCombination entityPvac = combination.Entity;

					_writer.WriteStartElement("ProductAttributeCombination");
					_writer.Write("Id", entityPvac.Id.ToString());
					_writer.Write("StockQuantity", entityPvac.StockQuantity.ToString());
					_writer.Write("AllowOutOfStockOrders", entityPvac.AllowOutOfStockOrders.ToString());
					_writer.Write("AttributesXml", entityPvac.AttributesXml);
					_writer.Write("Sku", entityPvac.Sku);
					_writer.Write("Gtin", entityPvac.Gtin);
					_writer.Write("ManufacturerPartNumber", entityPvac.ManufacturerPartNumber);
					_writer.Write("Price", entityPvac.Price.HasValue ? entityPvac.Price.Value.ToString(_culture) : "");
					_writer.Write("Length", entityPvac.Length.HasValue ? entityPvac.Length.Value.ToString(_culture) : "");
					_writer.Write("Width", entityPvac.Width.HasValue ? entityPvac.Width.Value.ToString(_culture) : "");
					_writer.Write("Height", entityPvac.Height.HasValue ? entityPvac.Height.Value.ToString(_culture) : "");
					_writer.Write("BasePriceAmount", entityPvac.BasePriceAmount.HasValue ? entityPvac.BasePriceAmount.Value.ToString(_culture) : "");
					_writer.Write("BasePriceBaseAmount", entityPvac.BasePriceBaseAmount.HasValue ? entityPvac.BasePriceBaseAmount.Value.ToString() : "");
					_writer.Write("AssignedPictureIds", entityPvac.AssignedPictureIds);
					_writer.Write("DeliveryTimeId", entityPvac.DeliveryTimeId.HasValue ? entityPvac.DeliveryTimeId.Value.ToString() : "");
					_writer.Write("IsActive", entityPvac.IsActive.ToString());

					WriteDeliveryTime(combination.DeliveryTime, "DeliveryTime");

					WriteQuantityUnit(combination.QuantityUnit, "QuantityUnit");

					_writer.WriteStartElement("Pictures");
					foreach (dynamic assignedPicture in combination.Pictures)
					{
						WritePicture(assignedPicture, "Picture");
					}
					_writer.WriteEndElement();	// Pictures

					_writer.WriteEndElement();	// ProductAttributeCombination
				}
				_writer.WriteEndElement(); // ProductAttributeCombinations
			}

			if (product.ProductPictures != null)
			{
				_writer.WriteStartElement("ProductPictures");
				foreach (dynamic productPicture in product.ProductPictures)
				{
					ProductPicture entityProductPicture = productPicture.Entity;

					_writer.WriteStartElement("ProductPicture");
					_writer.Write("Id", entityProductPicture.Id.ToString());
					_writer.Write("DisplayOrder", entityProductPicture.DisplayOrder.ToString());

					WritePicture(productPicture.Picture, "Picture");

					_writer.WriteEndElement();	// ProductPicture
				}
				_writer.WriteEndElement();	// ProductPictures
			}

			if (product.ProductCategories != null)
			{
				_writer.WriteStartElement("ProductCategories");
				foreach (dynamic productCategory in product.ProductCategories)
				{
					ProductCategory entityProductCategory = productCategory.Entity;

					_writer.WriteStartElement("ProductCategory");
					_writer.Write("Id", entityProductCategory.Id.ToString());
					_writer.Write("DisplayOrder", entityProductCategory.DisplayOrder.ToString());
					_writer.Write("IsFeaturedProduct", entityProductCategory.IsFeaturedProduct.ToString());

					WriteCategory(productCategory.Category, "Category");

					_writer.WriteEndElement();	// ProductCategory
				}
				_writer.WriteEndElement();	// ProductCategories
			}

			if (product.ProductManufacturers != null)
			{
				_writer.WriteStartElement("ProductManufacturers");
				foreach (dynamic productManu in product.ProductManufacturers)
				{
					ProductManufacturer entityProductManu = productManu.Entity;

					_writer.WriteStartElement("ProductManufacturer");

					_writer.Write("Id", entityProductManu.Id.ToString());
					_writer.Write("DisplayOrder", entityProductManu.DisplayOrder.ToString());
					_writer.Write("IsFeaturedProduct", entityProductManu.IsFeaturedProduct.ToString());

					WriteManufacturer(productManu.Manufacturer, "Manufacturer");

					_writer.WriteEndElement();	// ProductManufacturer
				}
				_writer.WriteEndElement();	// ProductManufacturers
			}

			if (product.ProductSpecificationAttributes != null)
			{
				_writer.WriteStartElement("ProductSpecificationAttributes");
				foreach (dynamic psa in product.ProductSpecificationAttributes)
				{
					ProductSpecificationAttribute entityPsa = psa.Entity;

					_writer.WriteStartElement("ProductSpecificationAttribute");

					_writer.Write("Id", entityPsa.Id.ToString());
					_writer.Write("ProductId", entityPsa.ProductId.ToString());
					_writer.Write("SpecificationAttributeOptionId", entityPsa.SpecificationAttributeOptionId.ToString());
					_writer.Write("AllowFiltering", entityPsa.AllowFiltering.ToString());
					_writer.Write("ShowOnProductPage", entityPsa.ShowOnProductPage.ToString());
					_writer.Write("DisplayOrder", entityPsa.DisplayOrder.ToString());

					dynamic option = psa.SpecificationAttributeOption;
					SpecificationAttributeOption entitySao = option.Entity;
					SpecificationAttribute entitySa = option.SpecificationAttribute.Entity;

					_writer.WriteStartElement("SpecificationAttributeOption");
					_writer.Write("Id", entitySao.Id.ToString());
					_writer.Write("SpecificationAttributeId", entitySao.SpecificationAttributeId.ToString());
					_writer.Write("DisplayOrder", entitySao.DisplayOrder.ToString());
					_writer.Write("Name", (string)option.Name);
					_writer.Write("Alias", (string)option.Alias);

					WriteLocalized(option);

					_writer.WriteStartElement("SpecificationAttribute");
					_writer.Write("Id", entitySa.Id.ToString());
					_writer.Write("Name", (string)option.SpecificationAttribute.Name);
					_writer.Write("Alias", (string)option.SpecificationAttribute.Alias);
					_writer.Write("DisplayOrder", entitySa.DisplayOrder.ToString());
					_writer.Write("AllowFiltering", entitySa.AllowFiltering.ToString());
					_writer.Write("ShowOnProductPage", entitySa.ShowOnProductPage.ToString());
					_writer.Write("FacetSorting", ((int)entitySa.FacetSorting).ToString());
					_writer.Write("FacetTemplateHint", ((int)entitySa.FacetTemplateHint).ToString());
                    _writer.Write("IndexOptionNames", entitySa.IndexOptionNames.ToString());

					WriteLocalized(option.SpecificationAttribute);

					_writer.WriteEndElement();	// SpecificationAttribute
					_writer.WriteEndElement();	// SpecificationAttributeOption

					_writer.WriteEndElement();	// ProductSpecificationAttribute
				}
				_writer.WriteEndElement();	// ProductSpecificationAttributes
			}

			if (product.ProductBundleItems != null)
			{
				_writer.WriteStartElement("ProductBundleItems");
				foreach (dynamic bundleItem in product.ProductBundleItems)
				{
					ProductBundleItem entityPbi = bundleItem.Entity;

					_writer.WriteStartElement("ProductBundleItem");
					_writer.Write("Id", entityPbi.Id.ToString());
					_writer.Write("ProductId", entityPbi.ProductId.ToString());
					_writer.Write("BundleProductId", entityPbi.BundleProductId.ToString());
					_writer.Write("Quantity", entityPbi.Quantity.ToString());
					_writer.Write("Discount", entityPbi.Discount.HasValue ? entityPbi.Discount.Value.ToString(_culture) : "");
					_writer.Write("DiscountPercentage", entityPbi.DiscountPercentage.ToString());
					_writer.Write("Name", (string)bundleItem.Name);
					_writer.Write("ShortDescription", (string)bundleItem.ShortDescription);
					_writer.Write("FilterAttributes", entityPbi.FilterAttributes.ToString());
					_writer.Write("HideThumbnail", entityPbi.HideThumbnail.ToString());
					_writer.Write("Visible", entityPbi.Visible.ToString());
					_writer.Write("Published", entityPbi.Published.ToString());
					_writer.Write("DisplayOrder", ((int)bundleItem.DisplayOrder).ToString());
					_writer.Write("CreatedOnUtc", entityPbi.CreatedOnUtc.ToString(_culture));
					_writer.Write("UpdatedOnUtc", entityPbi.UpdatedOnUtc.ToString(_culture));

					WriteLocalized(bundleItem);

					_writer.WriteEndElement();	// ProductBundleItem
				}
				_writer.WriteEndElement();	// ProductBundleItems
			}

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteCustomer(dynamic customer, string node)
		{
			if (customer == null)
				return;

			Customer entity = customer.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("CustomerGuid", entity.CustomerGuid.ToString());
			_writer.Write("Username", entity.Username);
			_writer.Write("Email", entity.Email);
			_writer.Write("AdminComment", entity.AdminComment);
			_writer.Write("IsTaxExempt", entity.IsTaxExempt.ToString());
			_writer.Write("AffiliateId", entity.AffiliateId.ToString());
			_writer.Write("Active", entity.Active.ToString());
			_writer.Write("Deleted", entity.Deleted.ToString());
			_writer.Write("IsSystemAccount", entity.IsSystemAccount.ToString());
			_writer.Write("SystemName", entity.SystemName);
			_writer.Write("LastIpAddress", entity.LastIpAddress);
			_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
			_writer.Write("LastLoginDateUtc", entity.LastLoginDateUtc.HasValue ? entity.LastLoginDateUtc.Value.ToString(_culture) : "");
			_writer.Write("LastActivityDateUtc", entity.LastActivityDateUtc.ToString(_culture));
			_writer.Write("RewardPointsBalance", ((int)customer._RewardPointsBalance).ToString());

			if (customer.CustomerRoles != null)
			{
				_writer.WriteStartElement("CustomerRoles");
				foreach (dynamic role in customer.CustomerRoles)
				{
					CustomerRole entityRole = role.Entity;

					_writer.WriteStartElement("CustomerRole");
					_writer.Write("Id", entityRole.Id.ToString());
					_writer.Write("Name", (string)role.Name);
					_writer.Write("FreeShipping", entityRole.FreeShipping.ToString());
					_writer.Write("TaxExempt", entityRole.TaxExempt.ToString());
					_writer.Write("TaxDisplayType", entityRole.TaxDisplayType.HasValue ? entityRole.TaxDisplayType.Value.ToString() : "");
					_writer.Write("Active", entityRole.Active.ToString());
					_writer.Write("IsSystemRole", entityRole.IsSystemRole.ToString());
					_writer.Write("SystemName", entityRole.SystemName);
					_writer.WriteEndElement();	// CustomerRole
				}
				_writer.WriteEndElement();	// CustomerRoles
			}

			WriteRewardPointsHistory(customer.RewardPointsHistory, "RewardPointsHistories");
			WriteAddress(customer.BillingAddress, "BillingAddress");
			WriteAddress(customer.ShippingAddress, "ShippingAddress");

			if (customer.Addresses != null)
			{
				_writer.WriteStartElement("Addresses");
				foreach (dynamic address in customer.Addresses)
				{
					WriteAddress(address, "Address");
				}
				_writer.WriteEndElement();	// Addresses
			}

			WriteGenericAttributes(customer);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteShoppingCartItem(dynamic shoppingCartItem, string node)
		{
			if (shoppingCartItem == null)
				return;

			ShoppingCartItem entity = shoppingCartItem.Entity;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", entity.Id.ToString());
			_writer.Write("StoreId", entity.StoreId.ToString());
			_writer.Write("ParentItemId", entity.ParentItemId.HasValue ? entity.ParentItemId.Value.ToString() : "");
			_writer.Write("BundleItemId", entity.BundleItemId.HasValue ? entity.BundleItemId.Value.ToString() : "");
			_writer.Write("ShoppingCartTypeId", entity.ShoppingCartTypeId.ToString());
			_writer.Write("CustomerId", entity.CustomerId.ToString());
			_writer.Write("ProductId", entity.ProductId.ToString());
			_writer.Write("AttributesXml", entity.AttributesXml, null, true);
			_writer.Write("CustomerEnteredPrice", entity.CustomerEnteredPrice.ToString(_culture));
			_writer.Write("Quantity", entity.Quantity.ToString());
			_writer.Write("CreatedOnUtc", entity.CreatedOnUtc.ToString(_culture));
			_writer.Write("UpdatedOnUtc", entity.UpdatedOnUtc.ToString(_culture));

			WriteCustomer(shoppingCartItem.Customer, "Customer");
			WriteProduct(shoppingCartItem.Product, "Product");

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}
	}


	/// <summary>
	/// Allows to exclude XML nodes from export
	/// </summary>
	[Flags]
	public enum ExportXmlExclude
	{
		None = 0,
		Category = 1
	}
}
