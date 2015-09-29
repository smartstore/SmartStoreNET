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

			if (product.DeliveryTime != null)
			{
				_writer.WriteStartElement("DeliveryTime");

				_writer.Write("Id", ((int)product.DeliveryTime.Id).ToString());
				_writer.Write("Name", (string)product.DeliveryTime.Name);
				_writer.Write("DisplayLocale", (string)product.DeliveryTime.DisplayLocale);
				_writer.Write("ColorHexValue", (string)product.DeliveryTime.ColorHexValue);
				_writer.Write("DisplayOrder", ((int)product.DeliveryTime.DisplayOrder).ToString());

				_writer.WriteEndElement();	// DeliveryTime
			}

			if (product.QuantityUnit != null)
			{
				_writer.WriteStartElement("QuantityUnit");

				_writer.Write("Id", ((int)product.QuantityUnit.Id).ToString());
				_writer.Write("Name", (string)product.QuantityUnit.Name);
				_writer.Write("Description", (string)product.QuantityUnit.Description);
				_writer.Write("DisplayLocale", (string)product.QuantityUnit.DisplayLocale);
				_writer.Write("DisplayOrder", ((int)product.QuantityUnit.DisplayOrder).ToString());
				_writer.Write("IsDefault", ((bool)product.QuantityUnit.IsDefault).ToString());

				_writer.WriteEndElement();	// QuantityUnit
			}

			if (node.HasValue())
			{
				_writer.WriteEndElement();
			}
		}
	}
}
