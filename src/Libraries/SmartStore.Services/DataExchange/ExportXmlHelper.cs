using System;
using System.Globalization;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	public class ExportXmlHelper
	{
		private XmlWriter _writer;
		private CultureInfo _culture;

		public ExportXmlHelper(XmlWriter writer, CultureInfo culture)
		{
			_writer = writer;
			_culture = culture;
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
				_writer.WriteStartElement("GenericAttribute");
				_writer.Write("Id", ((int)genericAttribute.Id).ToString());
				_writer.Write("EntityId", ((int)genericAttribute.EntityId).ToString());
				_writer.Write("KeyGroup", (string)genericAttribute.KeyGroup);
				_writer.Write("Key", (string)genericAttribute.Key);
				_writer.Write("Value", (string)genericAttribute.Value);
				_writer.Write("StoreId", ((int)genericAttribute.StoreId).ToString());
				_writer.WriteEndElement();	// GenericAttribute
			}
			_writer.WriteEndElement();	// GenericAttributes
		}

		public void WriteAddress(dynamic address, string node)
		{
			if (address == null)
				return;

			int? countryId = address.CountryId;
			int? stateProvinceId = address.StateProvinceId;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)address.Id).ToString());
			_writer.Write("FirstName", (string)address.FirstName);
			_writer.Write("LastName", (string)address.LastName);
			_writer.Write("Email", (string)address.Email);
			_writer.Write("Company", (string)address.Company);
			_writer.Write("CountryId", countryId.HasValue ? countryId.Value.ToString() : "");
			_writer.Write("StateProvinceId", stateProvinceId.HasValue ? stateProvinceId.Value.ToString() : "");
			_writer.Write("City", (string)address.City);
			_writer.Write("Address1", (string)address.Address1);
			_writer.Write("Address2", (string)address.Address2);
			_writer.Write("ZipPostalCode", (string)address.ZipPostalCode);
			_writer.Write("PhoneNumber", (string)address.PhoneNumber);
			_writer.Write("FaxNumber", (string)address.FaxNumber);
			_writer.Write("CreatedOnUtc", ((DateTime)address.CreatedOnUtc).ToString(_culture));

			if (address.Country != null)
			{
				dynamic country = address.Country;

				_writer.WriteStartElement("Country");
				_writer.Write("Id", ((int)country.Id).ToString());
				_writer.Write("Name", (string)country.Name);
				_writer.Write("AllowsBilling", ((bool)country.AllowsBilling).ToString());
				_writer.Write("AllowsShipping", ((bool)country.AllowsShipping).ToString());
				_writer.Write("TwoLetterIsoCode", (string)country.TwoLetterIsoCode);
				_writer.Write("ThreeLetterIsoCode", (string)country.ThreeLetterIsoCode);
				_writer.Write("NumericIsoCode", ((int)country.NumericIsoCode).ToString());
				_writer.Write("SubjectToVat", ((bool)country.SubjectToVat).ToString());
				_writer.Write("Published", ((bool)country.Published).ToString());
				_writer.Write("DisplayOrder", ((int)country.DisplayOrder).ToString());
				_writer.Write("LimitedToStores", ((bool)country.LimitedToStores).ToString());

				WriteLocalized(country);

				_writer.WriteEndElement();	// Country
			}

			if (address.StateProvince != null)
			{
				dynamic stateProvince = address.StateProvince;

				_writer.WriteStartElement("StateProvince");
				_writer.Write("Id", ((int)stateProvince.Id).ToString());
				_writer.Write("CountryId", ((int)stateProvince.CountryId).ToString());
				_writer.Write("Name", (string)stateProvince.Name);
				_writer.Write("Abbreviation", (string)stateProvince.Abbreviation);
				_writer.Write("Published", ((bool)stateProvince.Published).ToString());
				_writer.Write("DisplayOrder", ((int)stateProvince.DisplayOrder).ToString());

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

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)currency.Id).ToString());
			_writer.Write("Name", (string)currency.Name);
			_writer.Write("CurrencyCode", (string)currency.CurrencyCode);
			_writer.Write("Rate", ((decimal)currency.Rate).ToString(_culture));
			_writer.Write("DisplayLocale", (string)currency.DisplayLocale);
			_writer.Write("CustomFormatting", (string)currency.CustomFormatting);
			_writer.Write("LimitedToStores", ((bool)currency.LimitedToStores).ToString());
			_writer.Write("Published", ((bool)currency.Published).ToString());
			_writer.Write("DisplayOrder", ((int)currency.DisplayOrder).ToString());
			_writer.Write("CreatedOnUtc", ((DateTime)currency.CreatedOnUtc).ToString(_culture));
			_writer.Write("UpdatedOnUtc", ((DateTime)currency.UpdatedOnUtc).ToString(_culture));
			_writer.Write("DomainEndings", (string)currency.DomainEndings);

			WriteLocalized(currency);

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
				_writer.WriteStartElement("RewardPointsHistory");
				_writer.Write("Id", ((int)rewardPoint.Id).ToString());
				_writer.Write("CustomerId", ((int)rewardPoint.CustomerId).ToString());
				_writer.Write("Points", ((int)rewardPoint.Points).ToString());
				_writer.Write("PointsBalance", ((int)rewardPoint.PointsBalance).ToString());
				_writer.Write("UsedAmount", ((decimal)rewardPoint.UsedAmount).ToString(_culture));
				_writer.Write("Message", (string)rewardPoint.Message);
				_writer.Write("CreatedOnUtc", ((DateTime)rewardPoint.CreatedOnUtc).ToString(_culture));
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

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)deliveryTime.Id).ToString());
			_writer.Write("Name", (string)deliveryTime.Name);
			_writer.Write("DisplayLocale", (string)deliveryTime.DisplayLocale);
			_writer.Write("ColorHexValue", (string)deliveryTime.ColorHexValue);
			_writer.Write("DisplayOrder", ((int)deliveryTime.DisplayOrder).ToString());

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

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)quantityUnit.Id).ToString());
			_writer.Write("Name", (string)quantityUnit.Name);
			_writer.Write("Description", (string)quantityUnit.Description);
			_writer.Write("DisplayLocale", (string)quantityUnit.DisplayLocale);
			_writer.Write("DisplayOrder", ((int)quantityUnit.DisplayOrder).ToString());
			_writer.Write("IsDefault", ((bool)quantityUnit.IsDefault).ToString());

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

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)picture.Id).ToString());
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

			int? pictureId = category.PictureId;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)category.Id).ToString());
			_writer.Write("Name", (string)category.Name);
			_writer.Write("FullName", (string)category.FullName);
			_writer.Write("Description", (string)category.Description);
			_writer.Write("BottomDescription", (string)category.BottomDescription);
			_writer.Write("CategoryTemplateId", ((int)category.CategoryTemplateId).ToString());
			_writer.Write("MetaKeywords", (string)category.MetaKeywords);
			_writer.Write("MetaDescription", (string)category.MetaDescription);
			_writer.Write("MetaTitle", (string)category.MetaTitle);
			_writer.Write("SeName", (string)category.SeName);
			_writer.Write("ParentCategoryId", ((int)category.ParentCategoryId).ToString());
			_writer.Write("PictureId", pictureId.HasValue ? pictureId.Value.ToString() : "");
			_writer.Write("PageSize", ((int)category.PageSize).ToString());
			_writer.Write("AllowCustomersToSelectPageSize", ((bool)category.AllowCustomersToSelectPageSize).ToString());
			_writer.Write("PageSizeOptions", (string)category.PageSizeOptions);
			_writer.Write("PriceRanges", (string)category.PriceRanges);
			_writer.Write("ShowOnHomePage", ((bool)category.ShowOnHomePage).ToString());
			_writer.Write("HasDiscountsApplied", ((bool)category.HasDiscountsApplied).ToString());
			_writer.Write("Published", ((bool)category.Published).ToString());
			_writer.Write("Deleted", ((bool)category.Deleted).ToString());
			_writer.Write("DisplayOrder", ((int)category.DisplayOrder).ToString());
			_writer.Write("CreatedOnUtc", ((DateTime)category.CreatedOnUtc).ToString(_culture));
			_writer.Write("UpdatedOnUtc", ((DateTime)category.UpdatedOnUtc).ToString(_culture));
			_writer.Write("SubjectToAcl", ((bool)category.SubjectToAcl).ToString());
			_writer.Write("LimitedToStores", ((bool)category.LimitedToStores).ToString());
			_writer.Write("Alias", (string)category.Alias);
			_writer.Write("DefaultViewMode", (string)category.DefaultViewMode);

			WritePicture(category.Picture, "Picture");

			WriteLocalized(category);

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}

		public void WriteManufacturer(dynamic manufacturer, string node)
		{
			if (manufacturer == null)
				return;

			int? pictureId = manufacturer.PictureId;

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			_writer.Write("Id", ((int)manufacturer.Id).ToString());
			_writer.Write("Name", (string)manufacturer.Name);
			_writer.Write("SeName", (string)manufacturer.SeName);
			_writer.Write("Description", (string)manufacturer.Description);
			_writer.Write("ManufacturerTemplateId", ((int)manufacturer.ManufacturerTemplateId).ToString());
			_writer.Write("MetaKeywords", (string)manufacturer.MetaKeywords);
			_writer.Write("MetaDescription", (string)manufacturer.MetaDescription);
			_writer.Write("MetaTitle", (string)manufacturer.MetaTitle);
			_writer.Write("PictureId", pictureId.HasValue ? pictureId.Value.ToString() : "");
			_writer.Write("PageSize", ((int)manufacturer.PageSize).ToString());
			_writer.Write("AllowCustomersToSelectPageSize", ((bool)manufacturer.AllowCustomersToSelectPageSize).ToString());
			_writer.Write("PageSizeOptions", (string)manufacturer.PageSizeOptions);
			_writer.Write("PriceRanges", (string)manufacturer.PriceRanges);
			_writer.Write("Published", ((bool)manufacturer.Published).ToString());
			_writer.Write("Deleted", ((bool)manufacturer.Deleted).ToString());
			_writer.Write("DisplayOrder", ((int)manufacturer.DisplayOrder).ToString());
			_writer.Write("CreatedOnUtc", ((DateTime)manufacturer.CreatedOnUtc).ToString(_culture));
			_writer.Write("UpdatedOnUtc", ((DateTime)manufacturer.UpdatedOnUtc).ToString(_culture));

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

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			int? downloadExpirationDays = product.DownloadExpirationDays;
			int? sampleDownloadId = product.SampleDownloadId;
			decimal? specialPrice = product.SpecialPrice;
			DateTime? specialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc;
			DateTime? specialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc;
			DateTime? availableStartDateTimeUtc = product.AvailableStartDateTimeUtc;
			DateTime? availableEndDateTimeUtc = product.AvailableEndDateTimeUtc;
			decimal? basePriceAmount = product.BasePriceAmount;
			int? basePriceBaseAmount = product.BasePriceBaseAmount;
			decimal? lowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice;

			_writer.Write("Id", ((int)product.Id).ToString());
			_writer.Write("Name", (string)product.Name);
			_writer.Write("SeName", (string)product.SeName);
			_writer.Write("ShortDescription", (string)product.ShortDescription);
			_writer.Write("FullDescription", (string)product.FullDescription);
			_writer.Write("AdminComment", (string)product.AdminComment);
			_writer.Write("ProductTemplateId", ((int)product.ProductTemplateId).ToString());
			_writer.Write("ProductTemplateViewPath", (string)product._ProductTemplateViewPath);
			_writer.Write("ShowOnHomePage", ((bool)product.ShowOnHomePage).ToString());
			_writer.Write("HomePageDisplayOrder", ((int)product.HomePageDisplayOrder).ToString());
			_writer.Write("MetaKeywords", (string)product.MetaKeywords);
			_writer.Write("MetaDescription", (string)product.MetaDescription);
			_writer.Write("MetaTitle", (string)product.MetaTitle);
			_writer.Write("AllowCustomerReviews", ((bool)product.AllowCustomerReviews).ToString());
			_writer.Write("ApprovedRatingSum", ((int)product.ApprovedRatingSum).ToString());
			_writer.Write("NotApprovedRatingSum", ((int)product.NotApprovedRatingSum).ToString());
			_writer.Write("ApprovedTotalReviews", ((int)product.ApprovedTotalReviews).ToString());
			_writer.Write("NotApprovedTotalReviews", ((int)product.NotApprovedTotalReviews).ToString());
			_writer.Write("Published", ((bool)product.Published).ToString());
			_writer.Write("CreatedOnUtc", ((DateTime)product.CreatedOnUtc).ToString(_culture));
			_writer.Write("UpdatedOnUtc", ((DateTime)product.UpdatedOnUtc).ToString(_culture));
			_writer.Write("SubjectToAcl", ((bool)product.SubjectToAcl).ToString());
			_writer.Write("LimitedToStores", ((bool)product.LimitedToStores).ToString());
			_writer.Write("ProductTypeId", ((int)product.ProductTypeId).ToString());
			_writer.Write("ParentGroupedProductId", ((int)product.ParentGroupedProductId).ToString());
			_writer.Write("Sku", (string)product.Sku);
			_writer.Write("ManufacturerPartNumber", (string)product.ManufacturerPartNumber);
			_writer.Write("Gtin", (string)product.Gtin);
			_writer.Write("IsGiftCard", ((bool)product.IsGiftCard).ToString());
			_writer.Write("GiftCardTypeId", ((int)product.GiftCardTypeId).ToString());
			_writer.Write("RequireOtherProducts", ((bool)product.RequireOtherProducts).ToString());
			_writer.Write("RequiredProductIds", (string)product.RequiredProductIds);
			_writer.Write("AutomaticallyAddRequiredProducts", ((bool)product.AutomaticallyAddRequiredProducts).ToString());
			_writer.Write("IsDownload", ((bool)product.IsDownload).ToString());
			_writer.Write("DownloadId", ((int)product.DownloadId).ToString());
			_writer.Write("UnlimitedDownloads", ((bool)product.UnlimitedDownloads).ToString());
			_writer.Write("MaxNumberOfDownloads", ((int)product.MaxNumberOfDownloads).ToString());
			_writer.Write("DownloadExpirationDays", downloadExpirationDays.HasValue ? downloadExpirationDays.Value.ToString() : "");
			_writer.Write("DownloadActivationTypeId", ((int)product.DownloadActivationTypeId).ToString());
			_writer.Write("HasSampleDownload", ((bool)product.HasSampleDownload).ToString());
			_writer.Write("SampleDownloadId", sampleDownloadId.HasValue ? sampleDownloadId.Value.ToString() : "");
			_writer.Write("HasUserAgreement", ((bool)product.HasUserAgreement).ToString());
			_writer.Write("UserAgreementText", (string)product.UserAgreementText);
			_writer.Write("IsRecurring", ((bool)product.IsRecurring).ToString());
			_writer.Write("RecurringCycleLength", ((int)product.RecurringCycleLength).ToString());
			_writer.Write("RecurringCyclePeriodId", ((int)product.RecurringCyclePeriodId).ToString());
			_writer.Write("RecurringTotalCycles", ((int)product.RecurringTotalCycles).ToString());
			_writer.Write("IsShipEnabled", ((bool)product.IsShipEnabled).ToString());
			_writer.Write("IsFreeShipping", ((bool)product.IsFreeShipping).ToString());
			_writer.Write("AdditionalShippingCharge", ((decimal)product.AdditionalShippingCharge).ToString(_culture));
			_writer.Write("IsTaxExempt", ((bool)product.IsTaxExempt).ToString());
			_writer.Write("TaxCategoryId", ((int)product.TaxCategoryId).ToString());
			_writer.Write("ManageInventoryMethodId", ((int)product.ManageInventoryMethodId).ToString());
			_writer.Write("StockQuantity", ((int)product.StockQuantity).ToString());
			_writer.Write("DisplayStockAvailability", ((bool)product.DisplayStockAvailability).ToString());
			_writer.Write("DisplayStockQuantity", ((bool)product.DisplayStockQuantity).ToString());
			_writer.Write("MinStockQuantity", ((int)product.MinStockQuantity).ToString());
			_writer.Write("LowStockActivityId", ((int)product.LowStockActivityId).ToString());
			_writer.Write("NotifyAdminForQuantityBelow", ((int)product.NotifyAdminForQuantityBelow).ToString());
			_writer.Write("BackorderModeId", ((int)product.BackorderModeId).ToString());
			_writer.Write("AllowBackInStockSubscriptions", ((bool)product.AllowBackInStockSubscriptions).ToString());
			_writer.Write("OrderMinimumQuantity", ((int)product.OrderMinimumQuantity).ToString());
			_writer.Write("OrderMaximumQuantity", ((int)product.OrderMaximumQuantity).ToString());
			_writer.Write("AllowedQuantities", (string)product.AllowedQuantities);
			_writer.Write("DisableBuyButton", ((bool)product.DisableBuyButton).ToString());
			_writer.Write("DisableWishlistButton", ((bool)product.DisableWishlistButton).ToString());
			_writer.Write("AvailableForPreOrder", ((bool)product.AvailableForPreOrder).ToString());
			_writer.Write("CallForPrice", ((bool)product.CallForPrice).ToString());
			_writer.Write("Price", ((decimal)product.Price).ToString(_culture));
			_writer.Write("OldPrice", ((decimal)product.OldPrice).ToString(_culture));
			_writer.Write("ProductCost", ((decimal)product.ProductCost).ToString(_culture));
			_writer.Write("SpecialPrice", specialPrice.HasValue ? specialPrice.Value.ToString(_culture) : "");
			_writer.Write("SpecialPriceStartDateTimeUtc", specialPriceStartDateTimeUtc.HasValue ? specialPriceStartDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("SpecialPriceEndDateTimeUtc", specialPriceEndDateTimeUtc.HasValue ? specialPriceEndDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("CustomerEntersPrice", ((bool)product.CustomerEntersPrice).ToString());
			_writer.Write("MinimumCustomerEnteredPrice", ((decimal)product.MinimumCustomerEnteredPrice).ToString(_culture));
			_writer.Write("MaximumCustomerEnteredPrice", ((decimal)product.MaximumCustomerEnteredPrice).ToString(_culture));
			_writer.Write("HasTierPrices", ((bool)product.HasTierPrices).ToString());
			_writer.Write("HasDiscountsApplied", ((bool)product.HasDiscountsApplied).ToString());
			_writer.Write("Weight", ((decimal)product.Weight).ToString(_culture));
			_writer.Write("Length", ((decimal)product.Length).ToString(_culture));
			_writer.Write("Width", ((decimal)product.Width).ToString(_culture));
			_writer.Write("Height", ((decimal)product.Height).ToString(_culture));
			_writer.Write("AvailableStartDateTimeUtc", availableStartDateTimeUtc.HasValue ? availableStartDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("AvailableEndDateTimeUtc", availableEndDateTimeUtc.HasValue ? availableEndDateTimeUtc.Value.ToString(_culture) : "");
			_writer.Write("BasePriceEnabled", ((bool)product.BasePriceEnabled).ToString());
			_writer.Write("BasePriceMeasureUnit", (string)product.BasePriceMeasureUnit);
			_writer.Write("BasePriceAmount", basePriceAmount.HasValue ? basePriceAmount.Value.ToString(_culture) : "");
			_writer.Write("BasePriceBaseAmount", basePriceBaseAmount.HasValue ? basePriceBaseAmount.Value.ToString() : "");
			_writer.Write("BasePriceHasValue", ((bool)product.BasePriceHasValue).ToString());
			_writer.Write("BasePriceInfo", (string)product._BasePriceInfo);
			_writer.Write("VisibleIndividually", ((bool)product.VisibleIndividually).ToString());
			_writer.Write("DisplayOrder", ((int)product.DisplayOrder).ToString());
			_writer.Write("BundleTitleText", (string)product.BundleTitleText);
			_writer.Write("BundlePerItemPricing", ((bool)product.BundlePerItemPricing).ToString());
			_writer.Write("BundlePerItemShipping", ((bool)product.BundlePerItemShipping).ToString());
			_writer.Write("BundlePerItemShoppingCart", ((bool)product.BundlePerItemShoppingCart).ToString());
			_writer.Write("LowestAttributeCombinationPrice", lowestAttributeCombinationPrice.HasValue ? lowestAttributeCombinationPrice.Value.ToString(_culture) : "");
			_writer.Write("IsEsd", ((bool)product.IsEsd).ToString());

			WriteLocalized(product);

			WriteDeliveryTime(product.DeliveryTime, "DeliveryTime");

			WriteQuantityUnit(product.QuantityUnit, "QuantityUnit");

			if (product.AppliedDiscounts != null)
			{
				_writer.WriteStartElement("AppliedDiscounts");
				foreach (dynamic discount in product.AppliedDiscounts)
				{
					DateTime? startDateUtc = discount.StartDateUtc;
					DateTime? endDateUtc = discount.EndDateUtc;

					_writer.WriteStartElement("AppliedDiscount");
					_writer.Write("Id", ((int)discount.Id).ToString());
					_writer.Write("Name", (string)discount.Name);
					_writer.Write("DiscountTypeId", ((int)discount.DiscountTypeId).ToString());
					_writer.Write("UsePercentage", ((bool)discount.UsePercentage).ToString());
					_writer.Write("DiscountPercentage", ((decimal)discount.DiscountPercentage).ToString(_culture));
					_writer.Write("DiscountAmount", ((decimal)discount.DiscountAmount).ToString(_culture));
					_writer.Write("StartDateUtc", startDateUtc.HasValue ? startDateUtc.Value.ToString(_culture) : "");
					_writer.Write("EndDateUtc", endDateUtc.HasValue ? endDateUtc.Value.ToString(_culture) : "");
					_writer.Write("RequiresCouponCode", ((bool)discount.RequiresCouponCode).ToString());
					_writer.Write("CouponCode", (string)discount.CouponCode);
					_writer.Write("DiscountLimitationId", ((int)discount.DiscountLimitationId).ToString());
					_writer.Write("LimitationTimes", ((int)discount.LimitationTimes).ToString());
					_writer.WriteEndElement();	// AppliedDiscount
				}
				_writer.WriteEndElement();	// AppliedDiscounts
			}

			if (product.TierPrices != null)
			{
				_writer.WriteStartElement("TierPrices");
				foreach (dynamic tierPrice in product.TierPrices)
				{
					int? customerRoleId = tierPrice.CustomerRoleId;

					_writer.WriteStartElement("TierPrice");
					_writer.Write("Id", ((int)tierPrice.Id).ToString());
					_writer.Write("ProductId", ((int)tierPrice.ProductId).ToString());
					_writer.Write("StoreId", ((int)tierPrice.StoreId).ToString());
					_writer.Write("CustomerRoleId", customerRoleId.HasValue ? customerRoleId.Value.ToString() : "");
					_writer.Write("Quantity", ((int)tierPrice.Quantity).ToString());
					_writer.Write("Price", ((decimal)tierPrice.Price).ToString(_culture));
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
				foreach (dynamic pa in product.ProductAttributes)
				{
					_writer.WriteStartElement("ProductAttribute");
					_writer.Write("Id", ((int)pa.Id).ToString());
					_writer.Write("TextPrompt", (string)pa.TextPrompt);
					_writer.Write("IsRequired", ((bool)pa.IsRequired).ToString());
					_writer.Write("AttributeControlTypeId", ((int)pa.AttributeControlTypeId).ToString());
					_writer.Write("DisplayOrder", ((int)pa.DisplayOrder).ToString());

					_writer.WriteStartElement("Attribute");
					_writer.Write("Id", ((int)pa.Attribute.Id).ToString());
					_writer.Write("Alias", (string)pa.Attribute.Alias);
					_writer.Write("Name", (string)pa.Attribute.Name);
					_writer.Write("Description", (string)pa.Attribute.Description);

					WriteLocalized(pa.Attribute);

					_writer.WriteEndElement();	// Attribute

					_writer.WriteStartElement("AttributeValues");
					foreach (dynamic value in pa.Attribute.Values)
					{
						_writer.WriteStartElement("AttributeValue");
						_writer.Write("Id", ((int)value.Id).ToString());
						_writer.Write("Alias", (string)value.Alias);
						_writer.Write("Name", (string)value.Name);
						_writer.Write("ColorSquaresRgb", (string)value.ColorSquaresRgb);
						_writer.Write("PriceAdjustment", ((decimal)value.PriceAdjustment).ToString(_culture));
						_writer.Write("WeightAdjustment", ((decimal)value.WeightAdjustment).ToString(_culture));
						_writer.Write("IsPreSelected", ((bool)value.IsPreSelected).ToString());
						_writer.Write("DisplayOrder", ((int)value.DisplayOrder).ToString());
						_writer.Write("ValueTypeId", ((int)value.ValueTypeId).ToString());
						_writer.Write("LinkedProductId", ((int)value.LinkedProductId).ToString());
						_writer.Write("Quantity", ((int)value.Quantity).ToString());

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
					decimal? price = combination.Price;
					decimal? length = combination.Length;
					decimal? width = combination.Width;
					decimal? height = combination.Height;
					decimal? bpAmount = combination.BasePriceAmount;
					int? bpbAmount = combination.BasePriceBaseAmount;
					int? dlvTimeId = combination.DeliveryTimeId;

					_writer.WriteStartElement("ProductAttributeCombination");
					_writer.Write("Id", ((int)combination.Id).ToString());
					_writer.Write("StockQuantity", ((int)combination.StockQuantity).ToString());
					_writer.Write("AllowOutOfStockOrders", ((bool)combination.AllowOutOfStockOrders).ToString());
					_writer.Write("AttributesXml", (string)combination.AttributesXml);
					_writer.Write("Sku", (string)combination.Sku);
					_writer.Write("Gtin", (string)combination.Gtin);
					_writer.Write("ManufacturerPartNumber", (string)combination.ManufacturerPartNumber);
					_writer.Write("Price", price.HasValue ? price.Value.ToString(_culture) : "");
					_writer.Write("Length", length.HasValue ? length.Value.ToString(_culture) : "");
					_writer.Write("Width", width.HasValue ? width.Value.ToString(_culture) : "");
					_writer.Write("Height", height.HasValue ? height.Value.ToString(_culture) : "");
					_writer.Write("BasePriceAmount", bpAmount.HasValue ? bpAmount.Value.ToString(_culture) : "");
					_writer.Write("BasePriceBaseAmount", bpbAmount.HasValue ? bpbAmount.Value.ToString() : "");
					_writer.Write("AssignedPictureIds", (string)combination.AssignedPictureIds);
					_writer.Write("DeliveryTimeId", dlvTimeId.HasValue ? dlvTimeId.Value.ToString() : "");
					_writer.Write("IsActive", ((bool)combination.IsActive).ToString());

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
					_writer.WriteStartElement("ProductPicture");
					_writer.Write("Id", ((int)productPicture.Id).ToString());
					_writer.Write("DisplayOrder", ((int)productPicture.DisplayOrder).ToString());

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
					_writer.WriteStartElement("ProductCategory");
					_writer.Write("Id", ((int)productCategory.Id).ToString());
					_writer.Write("DisplayOrder", ((int)productCategory.DisplayOrder).ToString());
					_writer.Write("IsFeaturedProduct", ((bool)productCategory.IsFeaturedProduct).ToString());

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
					_writer.WriteStartElement("ProductManufacturer");

					_writer.Write("Id", ((int)productManu.Id).ToString());
					_writer.Write("DisplayOrder", ((int)productManu.DisplayOrder).ToString());
					_writer.Write("IsFeaturedProduct", ((bool)productManu.IsFeaturedProduct).ToString());

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
					_writer.WriteStartElement("ProductSpecificationAttribute");

					_writer.Write("Id", ((int)psa.Id).ToString());
					_writer.Write("ProductId", ((int)psa.ProductId).ToString());
					_writer.Write("SpecificationAttributeOptionId", ((int)psa.SpecificationAttributeOptionId).ToString());
					_writer.Write("AllowFiltering", ((bool)psa.AllowFiltering).ToString());
					_writer.Write("ShowOnProductPage", ((bool)psa.ShowOnProductPage).ToString());
					_writer.Write("DisplayOrder", ((int)psa.DisplayOrder).ToString());

					dynamic option = psa.SpecificationAttributeOption;

					_writer.WriteStartElement("SpecificationAttributeOption");
					_writer.Write("Id", ((int)option.Id).ToString());
					_writer.Write("SpecificationAttributeId", ((int)option.SpecificationAttributeId).ToString());
					_writer.Write("DisplayOrder", ((int)option.DisplayOrder).ToString());
					_writer.Write("Name", (string)option.Name);

					WriteLocalized(option);

					_writer.WriteStartElement("SpecificationAttribute");
					_writer.Write("Id", ((int)option.SpecificationAttribute.Id).ToString());
					_writer.Write("Name", (string)option.SpecificationAttribute.Name);
					_writer.Write("DisplayOrder", ((int)option.SpecificationAttribute.DisplayOrder).ToString());

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
					decimal? bundleItemDiscount = bundleItem.Discount;

					_writer.WriteStartElement("ProductBundleItem");
					_writer.Write("Id", ((int)bundleItem.Id).ToString());
					_writer.Write("ProductId", ((int)bundleItem.ProductId).ToString());
					_writer.Write("BundleProductId", ((int)bundleItem.BundleProductId).ToString());
					_writer.Write("Quantity", ((int)bundleItem.Quantity).ToString());
					_writer.Write("Discount", bundleItemDiscount.HasValue ? bundleItemDiscount.Value.ToString(_culture) : "");
					_writer.Write("DiscountPercentage", ((bool)bundleItem.DiscountPercentage).ToString());
					_writer.Write("Name", (string)bundleItem.Name);
					_writer.Write("ShortDescription", (string)bundleItem.ShortDescription);
					_writer.Write("FilterAttributes", ((bool)bundleItem.FilterAttributes).ToString());
					_writer.Write("HideThumbnail", ((bool)bundleItem.HideThumbnail).ToString());
					_writer.Write("Visible", ((bool)bundleItem.Visible).ToString());
					_writer.Write("Published", ((bool)bundleItem.Published).ToString());
					_writer.Write("DisplayOrder", ((int)bundleItem.DisplayOrder).ToString());
					_writer.Write("CreatedOnUtc", ((DateTime)bundleItem.CreatedOnUtc).ToString(_culture));
					_writer.Write("UpdatedOnUtc", ((DateTime)bundleItem.UpdatedOnUtc).ToString(_culture));

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

			if (node.HasValue())
			{
				_writer.WriteStartElement(node);
			}

			DateTime? lastLoginDateUtc = customer.LastLoginDateUtc;

			_writer.Write("Id", ((int)customer.Id).ToString());
			_writer.Write("CustomerGuid", ((Guid)customer.CustomerGuid).ToString());
			_writer.Write("Username", (string)customer.Username);
			_writer.Write("Email", (string)customer.Email);
			_writer.Write("PasswordFormatId", ((int)customer.PasswordFormatId).ToString());
			_writer.Write("AdminComment", (string)customer.AdminComment);
			_writer.Write("IsTaxExempt", ((bool)customer.IsTaxExempt).ToString());
			_writer.Write("AffiliateId", ((int)customer.AffiliateId).ToString());
			_writer.Write("Active", ((bool)customer.Active).ToString());
			_writer.Write("Deleted", ((bool)customer.Deleted).ToString());
			_writer.Write("IsSystemAccount", ((bool)customer.IsSystemAccount).ToString());
			_writer.Write("SystemName", (string)customer.SystemName);
			_writer.Write("LastIpAddress", (string)customer.LastIpAddress);
			_writer.Write("CreatedOnUtc", ((DateTime)customer.CreatedOnUtc).ToString(_culture));
			_writer.Write("LastLoginDateUtc", lastLoginDateUtc.HasValue ? lastLoginDateUtc.Value.ToString(_culture) : "");
			_writer.Write("LastActivityDateUtc", ((DateTime)customer.LastActivityDateUtc).ToString(_culture));
			_writer.Write("RewardPointsBalance", ((int)customer._RewardPointsBalance).ToString());

			if (customer.CustomerRoles != null)
			{
				_writer.WriteStartElement("CustomerRoles");
				foreach (dynamic role in customer.CustomerRoles)
				{
					int? taxDisplayType = role.TaxDisplayType;

					_writer.WriteStartElement("CustomerRole");
					_writer.Write("Id", ((int)role.Id).ToString());
					_writer.Write("Name", (string)role.Name);
					_writer.Write("FreeShipping", ((bool)role.FreeShipping).ToString());
					_writer.Write("TaxExempt", ((bool)role.TaxExempt).ToString());
					_writer.Write("TaxDisplayType", taxDisplayType.HasValue ? taxDisplayType.Value.ToString() : "");
					_writer.Write("Active", ((bool)role.Active).ToString());
					_writer.Write("IsSystemRole", ((bool)role.IsSystemRole).ToString());
					_writer.Write("SystemName", (string)role.SystemName);
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
	}
}
