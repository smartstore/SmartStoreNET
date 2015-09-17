using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;

namespace SmartStore.Services.DataExchange
{
	public static class ExpandoExtensions
	{
		public static ExpandoObject ToExpando(this Currency currency, int languageId)
		{
			if (currency == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = currency;

			expando.Id = currency.Id;
			expando.Name = currency.GetLocalized(x => x.Name, languageId, true, false);
			expando.CurrencyCode = currency.CurrencyCode;
			expando.Rate = currency.Rate;
			expando.DisplayLocale = currency.DisplayLocale;
			expando.CustomFormatting = currency.CustomFormatting;
			expando.LimitedToStores = currency.LimitedToStores;
			expando.Published = currency.Published;
			expando.DisplayOrder = currency.DisplayOrder;
			expando.CreatedOnUtc = currency.CreatedOnUtc;
			expando.UpdatedOnUtc = currency.UpdatedOnUtc;
			expando.DomainEndings = currency.DomainEndings;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Country country, int languageId)
		{
			if (country == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = country;

			expando.Id = country.Id;
			expando.Name = country.GetLocalized(x => x.Name, languageId, true, false);
			expando.AllowsBilling = country.AllowsBilling;
			expando.AllowsShipping = country.AllowsShipping;
			expando.TwoLetterIsoCode = country.TwoLetterIsoCode;
			expando.ThreeLetterIsoCode = country.ThreeLetterIsoCode;
			expando.NumericIsoCode = country.NumericIsoCode;
			expando.SubjectToVat = country.SubjectToVat;
			expando.Published = country.Published;
			expando.DisplayOrder = country.DisplayOrder;
			expando.LimitedToStores = country.LimitedToStores;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Address address, int languageId)
		{
			if (address == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = address;

			expando.Id = address.Id;
			expando.FirstName = address.FirstName;
			expando.LastName = address.LastName;
			expando.Email = address.Email;
			expando.Company = address.Company;
			expando.CountryId = address.CountryId;
			expando.StateProvinceId = address.StateProvinceId;
			expando.City = address.City;
			expando.Address1 = address.Address1;
			expando.Address2 = address.Address2;
			expando.ZipPostalCode = address.ZipPostalCode;
			expando.PhoneNumber = address.PhoneNumber;
			expando.FaxNumber = address.FaxNumber;
			expando.CreatedOnUtc = address.CreatedOnUtc;

			expando.Country = address.Country.ToExpando(languageId);

			return expando as ExpandoObject;
		}
		
		public static ExpandoObject ToExpando(this Customer customer)
		{
			if (customer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = customer;

			expando.Id = customer.Id;
			expando.CustomerGuid = customer.CustomerGuid;
			expando.Username = customer.Username;
			expando.Email = customer.Email;
			//Password... we not provide that data
			expando.PasswordFormatId = customer.PasswordFormatId;
			expando.PasswordFormat = customer.PasswordFormat;
			expando.AdminComment = customer.AdminComment;
			expando.IsTaxExempt = customer.IsTaxExempt;
			expando.AffiliateId = customer.AffiliateId;
			expando.Active = customer.Active;
			expando.Deleted = customer.Deleted;
			expando.IsSystemAccount = customer.IsSystemAccount;
			expando.SystemName = customer.SystemName;
			expando.LastIpAddress = customer.LastIpAddress;
			expando.CreatedOnUtc = customer.CreatedOnUtc;
			expando.LastLoginDateUtc = customer.LastLoginDateUtc;
			expando.LastActivityDateUtc = customer.LastActivityDateUtc;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Store store, int languageId)
		{
			if (store == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = store;

			expando.Id = store.Id;
			expando.Name = store.Name;
			expando.Url = store.Url;
			expando.SslEnabled = store.SslEnabled;
			expando.SecureUrl = store.SecureUrl;
			expando.Hosts = store.Hosts;
			expando.LogoPictureId = store.LogoPictureId;
			expando.DisplayOrder = store.DisplayOrder;
			expando.HtmlBodyId = store.HtmlBodyId;
			expando.ContentDeliveryNetwork = store.ContentDeliveryNetwork;
			expando.PrimaryStoreCurrencyId = store.PrimaryStoreCurrencyId;
			expando.PrimaryExchangeRateCurrencyId = store.PrimaryExchangeRateCurrencyId;

			expando.PrimaryStoreCurrency = store.PrimaryStoreCurrency.ToExpando(languageId);
			expando.PrimaryExchangeRateCurrency = store.PrimaryExchangeRateCurrency.ToExpando(languageId);

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this DeliveryTime deliveryTime, int languageId)
		{
			if (deliveryTime == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = deliveryTime;

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
			expando._Entity = quantityUnit;

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
			expando._Entity = picture;

			expando.Id = picture.Id;
			expando.SeoFileName = picture.SeoFilename;
			expando.MimeType = picture.MimeType;

			expando._ThumbImageUrl = pictureService.GetPictureUrl(picture, thumbPictureSize, false, store.Url);
			expando._ImageUrl = pictureService.GetPictureUrl(picture, detailsPictureSize, false, store.Url);
			expando._FullSizeImageUrl = pictureService.GetPictureUrl(picture, 0, false, store.Url);

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this ProductVariantAttribute pva, int languageId)
		{
			if (pva == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = pva;

			expando.Id = pva.Id;
			expando.TextPrompt = pva.TextPrompt;
			expando.IsRequired = pva.IsRequired;
			expando.AttributeControlTypeId = pva.AttributeControlTypeId;
			expando.DisplayOrder = pva.DisplayOrder;

			dynamic attribute = new ExpandoObject();
			attribute._Entity = pva.ProductAttribute;
			attribute.Id = pva.ProductAttribute.Id;
			attribute.Alias = pva.ProductAttribute.Alias;
			attribute.Name = pva.ProductAttribute.GetLocalized(y => y.Name, languageId, true, false);
			attribute.Description = pva.ProductAttribute.GetLocalized(y => y.Description, languageId, true, false);

			attribute.Values = pva.ProductVariantAttributeValues
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic value = new ExpandoObject();
					value._Entity = x;
					value.Id = x.Id;
					value.Alias = x.Alias;
					value.Name = x.GetLocalized(z => z.Name, languageId, true, false);
					value.ColorSquaresRgb = x.ColorSquaresRgb;
					value.PriceAdjustment = x.PriceAdjustment;
					value.WeightAdjustment = x.WeightAdjustment;
					value.IsPreSelected = x.IsPreSelected;
					value.DisplayOrder = x.DisplayOrder;
					value.ValueTypeId = x.ValueTypeId;
					value.LinkedProductId = x.LinkedProductId;
					value.Quantity = x.Quantity;

					return value as ExpandoObject;
				})
				.ToList();

			expando.Attribute = attribute as ExpandoObject;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this ProductVariantAttributeCombination pvac)
		{
			if (pvac == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = pvac;

			expando.Id = pvac.Id;
			expando.StockQuantity = pvac.StockQuantity;
			expando.AllowOutOfStockOrders = pvac.AllowOutOfStockOrders;
			expando.AttributesXml = pvac.AttributesXml;
			expando.Sku = pvac.Sku;
			expando.Gtin = pvac.Gtin;
			expando.ManufacturerPartNumber = pvac.ManufacturerPartNumber;
			expando.Price = pvac.Price;
			expando.Length = pvac.Length;
			expando.Width = pvac.Width;
			expando.Height = pvac.Height;
			expando.BasePriceAmount = pvac.BasePriceAmount;
			expando.BasePriceBaseAmount = pvac.BasePriceBaseAmount;
			expando.DeliveryTimeId = pvac.DeliveryTimeId;
			expando.IsActive = pvac.IsActive;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this Manufacturer manufacturer, int languageId)
		{
			if (manufacturer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = manufacturer;

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
			expando._Entity = category;

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
			expando._Entity = product;

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
			expando.BasePriceHasValue = product.BasePriceHasValue;
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

		public static ExpandoObject ToExpando(this Order order, int languageId, ILocalizationService localization)
		{
			if (order == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = order;

			expando.Id = order.Id;
			expando.OrderNumber = order.GetOrderNumber();
			expando.OrderGuid = order.OrderGuid;
			expando.StoreId = order.StoreId;
			expando.CustomerId = order.CustomerId;
			expando.BillingAddressId = order.BillingAddressId;
			expando.ShippingAddressId = order.ShippingAddressId;
			expando.OrderStatusId = order.OrderStatusId;
			expando.ShippingStatusId = order.ShippingStatusId;
			expando.PaymentStatusId = order.PaymentStatusId;
			expando.PaymentMethodSystemName = order.PaymentMethodSystemName;
			expando.CustomerCurrencyCode = order.CustomerCurrencyCode;
			expando.CurrencyRate = order.CurrencyRate;
			expando.CustomerTaxDisplayTypeId = order.CustomerTaxDisplayTypeId;
			expando.VatNumber = order.VatNumber;
			expando.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
			expando.OrderSubtotalExclTax = order.OrderSubtotalExclTax;
			expando.OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountInclTax;
			expando.OrderSubTotalDiscountExclTax = order.OrderSubTotalDiscountExclTax;
			expando.OrderShippingInclTax = order.OrderShippingInclTax;
			expando.OrderShippingExclTax = order.OrderShippingExclTax;
			expando.OrderShippingTaxRate = order.OrderShippingTaxRate;
			expando.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
			expando.PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax;
			expando.PaymentMethodAdditionalFeeTaxRate = order.PaymentMethodAdditionalFeeTaxRate;
			expando.TaxRates = order.TaxRates;
			expando.OrderTax = order.OrderTax;
			expando.OrderDiscount = order.OrderDiscount;
			expando.OrderTotal = order.OrderTotal;
			expando.RefundedAmount = order.RefundedAmount;
			expando.RewardPointsWereAdded = order.RewardPointsWereAdded;
			expando.CheckoutAttributeDescription = order.CheckoutAttributeDescription;
			expando.CheckoutAttributesXml = order.CheckoutAttributesXml;
			expando.CustomerLanguageId = order.CustomerLanguageId;
			expando.AffiliateId = order.AffiliateId;
			expando.CustomerIp = order.CustomerIp;
			expando.AllowStoringCreditCardNumber = order.AllowStoringCreditCardNumber;
			expando.CardType = order.CardType;
			expando.CardName = order.CardName;
			expando.CardNumber = order.CardNumber;
			expando.MaskedCreditCardNumber = order.MaskedCreditCardNumber;
			expando.CardCvv2 = order.CardCvv2;
			expando.CardExpirationMonth = order.CardExpirationMonth;
			expando.CardExpirationYear = order.CardExpirationYear;
			expando.AllowStoringDirectDebit = order.AllowStoringDirectDebit;
			expando.DirectDebitAccountHolder = order.DirectDebitAccountHolder;
			expando.DirectDebitAccountNumber = order.DirectDebitAccountNumber;
			expando.DirectDebitBankCode = order.DirectDebitBankCode;
			expando.DirectDebitBankName = order.DirectDebitBankName;
			expando.DirectDebitBIC = order.DirectDebitBIC;
			expando.DirectDebitCountry = order.DirectDebitCountry;
			expando.DirectDebitIban = order.DirectDebitIban;
			expando.CustomerOrderComment = order.CustomerOrderComment;
			expando.AuthorizationTransactionId = order.AuthorizationTransactionId;
			expando.AuthorizationTransactionCode = order.AuthorizationTransactionCode;
			expando.AuthorizationTransactionResult = order.AuthorizationTransactionResult;
			expando.CaptureTransactionId = order.CaptureTransactionId;
			expando.CaptureTransactionResult = order.CaptureTransactionResult;
			expando.SubscriptionTransactionId = order.SubscriptionTransactionId;
			expando.PurchaseOrderNumber = order.PurchaseOrderNumber;
			expando.PaidDateUtc = order.PaidDateUtc;
			expando.ShippingMethod = order.ShippingMethod;
			expando.ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName;
			expando.Deleted = order.Deleted;
			expando.CreatedOnUtc = order.CreatedOnUtc;
			expando.UpdatedOnUtc = order.UpdatedOnUtc;
			expando.RewardPointsRemaining = order.RewardPointsRemaining;
			expando.HasNewPaymentNotification = order.HasNewPaymentNotification;
			expando.OrderStatus = order.OrderStatus.GetLocalizedEnum(localization, languageId);
			expando.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(localization, languageId);
			expando.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(localization, languageId);
			expando.CustomerTaxDisplayType = order.CustomerTaxDisplayType;
			expando.TaxRatesDictionary = order.TaxRatesDictionary;

			return expando as ExpandoObject;
		}

		public static ExpandoObject ToExpando(this OrderItem orderItem, int languageId)
		{
			if (orderItem == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = orderItem;

			expando.Id = orderItem.Id;
			expando.OrderItemGuid = orderItem.OrderItemGuid;
			expando.OrderId = orderItem.OrderId;
			expando.ProductId = orderItem.ProductId;
			expando.Quantity = orderItem.Quantity;
			expando.UnitPriceInclTax = orderItem.UnitPriceInclTax;
			expando.UnitPriceExclTax = orderItem.UnitPriceExclTax;
			expando.PriceInclTax = orderItem.PriceInclTax;
			expando.PriceExclTax = orderItem.PriceExclTax;
			expando.TaxRate = orderItem.TaxRate;
			expando.DiscountAmountInclTax = orderItem.DiscountAmountInclTax;
			expando.DiscountAmountExclTax = orderItem.DiscountAmountExclTax;
			expando.AttributeDescription = orderItem.AttributeDescription;
			expando.AttributesXml = orderItem.AttributesXml;
			expando.DownloadCount = orderItem.DownloadCount;
			expando.IsDownloadActivated = orderItem.IsDownloadActivated;
			expando.LicenseDownloadId = orderItem.LicenseDownloadId;
			expando.ItemWeight = orderItem.ItemWeight;
			expando.BundleData = orderItem.BundleData;
			expando.ProductCost = orderItem.ProductCost;

			expando.Product = orderItem.Product.ToExpando(languageId);

			return expando as ExpandoObject;
		}
	}
}
