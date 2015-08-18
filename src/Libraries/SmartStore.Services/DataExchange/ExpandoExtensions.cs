using System.Dynamic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;

namespace SmartStore.Services.DataExchange
{
	public static class ExpandoExtensions
	{
		public static ExpandoObject ToExpando(this DeliveryTime deliveryTime, int languageId)
		{
			if (deliveryTime == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando.Id = deliveryTime.Id;
			expando.Name = deliveryTime.GetLocalized(x => x.Name, languageId, true, false);
			expando.DisplayLocale = deliveryTime.DisplayLocale;
			expando.ColorHexValue = deliveryTime.ColorHexValue;
			expando.DisplayOrder = deliveryTime.DisplayOrder;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this QuantityUnit quantityUnit, int languageId)
		{
			if (quantityUnit == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando.Id = quantityUnit.Id;
			expando.Name = quantityUnit.GetLocalized(x => x.Name, languageId, true, false);
			expando.Description = quantityUnit.GetLocalized(x => x.Description, languageId, true, false);
			expando.DisplayLocale = quantityUnit.DisplayLocale;
			expando.DisplayOrder = quantityUnit.DisplayOrder;
			expando.IsDefault = quantityUnit.IsDefault;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Picture picture, IPictureService pictureService, Store store, int thumbPictureSize, int detailsPictureSize)
		{
			if (picture == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando.Id = picture.Id;
			expando.SeoFileName = picture.SeoFilename;
			expando.MimeType = picture.MimeType;
			expando.ThumbImageUrl = pictureService.GetPictureUrl(picture, thumbPictureSize, false, store.Url);
			expando.ImageUrl = pictureService.GetPictureUrl(picture, detailsPictureSize, false, store.Url);
			expando.FullSizeImageUrl = pictureService.GetPictureUrl(picture, 0, false, store.Url);

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Manufacturer manufacturer, int languageId)
		{
			if (manufacturer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando.Id = manufacturer.Id;
			expando.Name = manufacturer.GetLocalized(x => x.Name, languageId, true, false);
			expando.SeName = manufacturer.GetSeName(languageId, true, false);
			expando.Description = manufacturer.GetLocalized(x => x.Description, languageId, true, false);
			expando.ManufacturerTemplateId = manufacturer.ManufacturerTemplateId;
			expando.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, languageId, true, false);
			expando.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, languageId, true, false);
			expando.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, languageId, true, false);
			expando.PictureId = manufacturer.PictureId;
			expando.PageSize = manufacturer.PageSize;
			expando.AllowCustomersToSelectPageSize = manufacturer.AllowCustomersToSelectPageSize;
			expando.PageSizeOptions = manufacturer.PageSizeOptions;
			expando.PriceRanges = manufacturer.PriceRanges;
			expando.Published = manufacturer.Published;
			expando.Deleted = manufacturer.Deleted;
			expando.DisplayOrder = manufacturer.DisplayOrder;
			expando.CreatedOnUtc = manufacturer.CreatedOnUtc;
			expando.UpdatedOnUtc = manufacturer.UpdatedOnUtc;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Category category, int languageId)
		{
			if (category == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando.Id = category.Id;
			expando.Name = category.GetLocalized(x => x.Name, languageId, true, false);
			expando.FullName = category.GetLocalized(x => x.FullName, languageId, true, false);
			expando.Description = category.GetLocalized(x => x.Description, languageId, true, false);
			expando.BottomDescription = category.GetLocalized(x => x.BottomDescription, languageId, true, false);
			expando.CategoryTemplateId = category.CategoryTemplateId;
			expando.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, languageId, true, false);
			expando.MetaDescription = category.GetLocalized(x => x.MetaDescription, languageId, true, false);
			expando.MetaTitle = category.GetLocalized(x => x.MetaTitle, languageId, true, false);
			expando.SeName = category.GetSeName(languageId, true, false);
			expando.ParentCategoryId = category.ParentCategoryId;
			expando.PageSize = category.PageSize;
			expando.AllowCustomersToSelectPageSize = category.AllowCustomersToSelectPageSize;
			expando.PageSizeOptions = category.PageSizeOptions;
			expando.PriceRanges = category.PriceRanges;
			expando.ShowOnHomePage = category.ShowOnHomePage;
			expando.HasDiscountsApplied = category.HasDiscountsApplied;
			expando.Published = category.Published;
			expando.Deleted = category.Deleted;
			expando.DisplayOrder = category.DisplayOrder;
			expando.CreatedOnUtc = category.CreatedOnUtc;
			expando.UpdatedOnUtc = category.UpdatedOnUtc;
			expando.SubjectToAcl = category.SubjectToAcl;
			expando.LimitedToStores = category.LimitedToStores;
			expando.Alias = category.Alias;
			expando.DefaultViewMode = category.DefaultViewMode;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Product product, int languageId)
		{
			if (product == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando.Id = product.Id;
			expando.Name = product.GetLocalized(x => x.Name, languageId, true, false);
			expando.SeName = product.GetSeName(languageId, true, false);
			expando.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, true, false);
			expando.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, true, false);
			expando.AdminComment = product.AdminComment;
			expando.ProductTemplateId = product.ProductTemplateId;
			expando.ShowOnHomePage = product.ShowOnHomePage;
			expando.HomePageDisplayOrder = product.HomePageDisplayOrder;
			expando.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, true, false);
			expando.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, true, false);
			expando.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, true, false);
			expando.AllowCustomerReviews = product.AllowCustomerReviews;
			expando.ApprovedRatingSum = product.ApprovedRatingSum;
			expando.NotApprovedRatingSum = product.NotApprovedRatingSum;
			expando.ApprovedTotalReviews = product.ApprovedTotalReviews;
			expando.NotApprovedTotalReviews = product.NotApprovedTotalReviews;
			expando.Published = product.Published;
			expando.CreatedOnUtc = product.CreatedOnUtc;
			expando.UpdatedOnUtc = product.UpdatedOnUtc;
			expando.SubjectToAcl = product.SubjectToAcl;
			expando.LimitedToStores = product.LimitedToStores;
			expando.ProductTypeId = product.ProductTypeId;
			expando.ParentGroupedProductId = product.ParentGroupedProductId;
			expando.Sku = product.Sku;
			expando.ManufacturerPartNumber = product.ManufacturerPartNumber;
			expando.Gtin = product.Gtin;
			expando.IsGiftCard = product.IsGiftCard;
			expando.GiftCardTypeId = product.GiftCardTypeId;
			expando.RequireOtherProducts = product.RequireOtherProducts;
			expando.RequiredProductIds = product.RequiredProductIds;
			expando.AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts;
			expando.IsDownload = product.IsDownload;
			expando.DownloadId = product.DownloadId;
			expando.UnlimitedDownloads = product.UnlimitedDownloads;
			expando.MaxNumberOfDownloads = product.MaxNumberOfDownloads;
			expando.DownloadExpirationDays = product.DownloadExpirationDays;
			expando.DownloadActivationType = product.DownloadActivationType;
			expando.HasSampleDownload = product.HasSampleDownload;
			expando.SampleDownloadId = product.SampleDownloadId;
			expando.HasUserAgreement = product.HasUserAgreement;
			expando.UserAgreementText = product.UserAgreementText;
			expando.IsRecurring = product.IsRecurring;
			expando.RecurringCycleLength = product.RecurringCycleLength;
			expando.RecurringCyclePeriodId = product.RecurringCyclePeriodId;
			expando.RecurringTotalCycles = product.RecurringTotalCycles;
			expando.IsShipEnabled = product.IsShipEnabled;
			expando.IsFreeShipping = product.IsFreeShipping;
			expando.AdditionalShippingCharge = product.AdditionalShippingCharge;
			expando.IsTaxExempt = product.IsTaxExempt;
			expando.TaxCategoryId = product.TaxCategoryId;
			expando.ManageInventoryMethodId = product.ManageInventoryMethodId;
			expando.StockQuantity = product.StockQuantity;
			expando.DisplayStockAvailability = product.DisplayStockAvailability;
			expando.DisplayStockQuantity = product.DisplayStockQuantity;
			expando.MinStockQuantity = product.MinStockQuantity;
			expando.LowStockActivityId = product.LowStockActivityId;
			expando.NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow;
			expando.BackorderModeId = product.BackorderModeId;
			expando.AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions;
			expando.OrderMinimumQuantity = product.OrderMinimumQuantity;
			expando.OrderMaximumQuantity = product.OrderMaximumQuantity;
			expando.AllowedQuantities = product.AllowedQuantities;
			expando.DisableBuyButton = product.DisableBuyButton;
			expando.DisableWishlistButton = product.DisableWishlistButton;
			expando.AvailableForPreOrder = product.AvailableForPreOrder;
			expando.CallForPrice = product.CallForPrice;
			expando.Price = product.Price;
			expando.OldPrice = product.OldPrice;
			expando.ProductCost = product.ProductCost;
			expando.SpecialPrice = product.SpecialPrice;
			expando.SpecialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc;
			expando.SpecialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc;
			expando.CustomerEntersPrice = product.CustomerEntersPrice;
			expando.MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice;
			expando.MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice;
			expando.HasTierPrices = product.HasTierPrices;
			expando.HasDiscountsApplied = product.HasDiscountsApplied;
			expando.Weight = product.Weight;
			expando.Length = product.Length;
			expando.Width = product.Width;
			expando.Height = product.Height;
			expando.AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc;
			expando.AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc;
			expando.BasePriceEnabled = product.BasePriceEnabled;
			expando.BasePriceMeasureUnit = product.BasePriceMeasureUnit;
			expando.BasePriceAmount = product.BasePriceAmount;
			expando.BasePriceBaseAmount = product.BasePriceBaseAmount;
			expando.VisibleIndividually = product.VisibleIndividually;
			expando.DisplayOrder = product.DisplayOrder;
			expando.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, languageId, true, false);
			expando.BundlePerItemPricing = product.BundlePerItemPricing;
			expando.BundlePerItemShipping = product.BundlePerItemShipping;
			expando.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;
			expando.LowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice;
			expando.IsEsd = product.IsEsd;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Product product, int languageId, IPictureService pictureService, MediaSettings mediaSettings, Store store)
		{
			dynamic expando = product.ToExpando(languageId);

			expando.DeliveryTime = product.DeliveryTimeId == 0 ? null : product.DeliveryTime.ToExpando(languageId);
			expando.QuantityUnit = product.QuantityUnitId == 0 ? null : product.QuantityUnit.ToExpando(languageId);

			// pictures
			expando.ProductPictures = product.ProductPictures
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.Picture = x.Picture.ToExpando(pictureService, store, mediaSettings.ProductThumbPictureSize, mediaSettings.ProductDetailsPictureSize);

					return exp as ExpandoObject;
				})
				.ToList();

			// manufacturers
			expando.ProductManufacturers = product.ProductManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.Manufacturer = x.Manufacturer.ToExpando(languageId);

					return exp as ExpandoObject;
				})
				.ToList();

			// categories
			expando.ProductCategories = product.ProductCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.Category = x.Category.ToExpando(languageId);

					return exp as ExpandoObject;
				})
				.ToList();

			return expando as ExpandoObject;
		}
	}
}
