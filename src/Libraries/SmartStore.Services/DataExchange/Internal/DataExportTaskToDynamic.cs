using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;

namespace SmartStore.Services.DataExchange.Internal
{
	internal partial class DataExportTask
	{
		private List<dynamic> GetLocalized<T>(DataExportTaskContext ctx, T entity, params Expression<Func<T, string>>[] keySelectors)
			where T : BaseEntity, ILocalizedEntity
		{
			if (ctx.Languages.Count <= 1)
				return null;

			var localized = new List<dynamic>();

			var localeKeyGroup = typeof(T).Name;
			var isSlugSupported = typeof(ISlugSupported).IsAssignableFrom(typeof(T));

			foreach (var language in ctx.Languages)
			{
				var languageCulture = language.Value.LanguageCulture.EmptyNull().ToLower();

				// add SeName
				if (isSlugSupported)
				{
					var value = _urlRecordService.GetActiveSlug(entity.Id, localeKeyGroup, language.Value.Id);
					if (value.HasValue())
					{
						dynamic exp = new HybridExpando();
						exp.Culture = languageCulture;
						exp.LocaleKey = "SeName";
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}

				foreach (var keySelector in keySelectors)
				{
					var member = keySelector.Body as MemberExpression;
					var propInfo = member.Member as PropertyInfo;
					string localeKey = propInfo.Name;
					var value = _localizedEntityService.GetLocalizedValue(language.Value.Id, entity.Id, localeKeyGroup, localeKey);

					// we better not export empty values. the risk is to high that they are imported and unnecessary fill databases.
					if (value.HasValue())
					{
						dynamic exp = new HybridExpando();
						exp.Culture = languageCulture;
						exp.LocaleKey = localeKey;
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}
			}

			return (localized.Count == 0 ? null : localized);
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Currency currency)
		{
			if (currency == null)
				return null;

			dynamic result = new DynamicEntity(currency);

			result.Name = currency.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result._Localized = GetLocalized(ctx, currency, x => x.Name);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Language language)
		{
			if (language == null)
				return null;

			dynamic result = new DynamicEntity(language);
			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Country country)
		{
			if (country == null)
				return null;

			dynamic result = new DynamicEntity(country);

			result.Name = country.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result._Localized = GetLocalized(ctx, country, x => x.Name);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Address address)
		{
			if (address == null)
				return null;

			dynamic result = new DynamicEntity(address);

			result.Country = ToDynamic(ctx, address.Country);

			if (address.StateProvinceId.GetValueOrDefault() > 0)
			{
				dynamic sp = new DynamicEntity(address.StateProvince);

				sp.Name = address.StateProvince.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
				sp._Localized = GetLocalized(ctx, address.StateProvince, x => x.Name);

				result.StateProvince = sp;
			}
			else
			{
				result.StateProvince = null;
			}

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, RewardPointsHistory points)
		{
			if (points == null)
				return null;

			dynamic result = new DynamicEntity(points);

			result.Id = points.Id;
			result.CustomerId = points.CustomerId;
			result.Points = points.Points;
			result.PointsBalance = points.PointsBalance;
			result.UsedAmount = points.UsedAmount;
			result.Message = points.Message;
			result.CreatedOnUtc = points.CreatedOnUtc;

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Customer customer)
		{
			if (customer == null)
				return null;

			dynamic result = new DynamicEntity(customer);

			result.Id = customer.Id;
			result.CustomerGuid = customer.CustomerGuid;
			result.Username = customer.Username;
			result.Email = customer.Email;
			result.Password = customer.Password;   // not so good but referenced in Excel export
			result.PasswordFormatId = customer.PasswordFormatId;
			result.PasswordSalt = customer.PasswordSalt;
			result.AdminComment = customer.AdminComment;
			result.IsTaxExempt = customer.IsTaxExempt;
			result.AffiliateId = customer.AffiliateId;
			result.Active = customer.Active;
			result.Deleted = customer.Deleted;
			result.IsSystemAccount = customer.IsSystemAccount;
			result.SystemName = customer.SystemName;
			result.LastIpAddress = customer.LastIpAddress;
			result.CreatedOnUtc = customer.CreatedOnUtc;
			result.LastLoginDateUtc = customer.LastLoginDateUtc;
			result.LastActivityDateUtc = customer.LastActivityDateUtc;

			result.BillingAddress = null;
			result.ShippingAddress = null;
			result.Addresses = null;
			result.CustomerRoles = null;

			result.RewardPointsHistory = null;
			result._RewardPointsBalance = 0;

			result._GenericAttributes = null;
			result._HasNewsletterSubscription = false;

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Store store)
		{
			if (store == null)
				return null;

			dynamic result = new DynamicEntity(store);

			result.Id = store.Id;
			result.Name = store.Name;
			result.Url = store.Url;
			result.SslEnabled = store.SslEnabled;
			result.SecureUrl = store.SecureUrl;
			result.Hosts = store.Hosts;
			result.LogoPictureId = store.LogoPictureId;
			result.DisplayOrder = store.DisplayOrder;
			result.HtmlBodyId = store.HtmlBodyId;
			result.ContentDeliveryNetwork = store.ContentDeliveryNetwork;
			result.PrimaryStoreCurrencyId = store.PrimaryStoreCurrencyId;
			result.PrimaryExchangeRateCurrencyId = store.PrimaryExchangeRateCurrencyId;

			result.PrimaryStoreCurrency = ToDynamic(ctx, store.PrimaryStoreCurrency);
			result.PrimaryExchangeRateCurrency = ToDynamic(ctx, store.PrimaryExchangeRateCurrency);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, DeliveryTime deliveryTime)
		{
			if (deliveryTime == null)
				return null;

			dynamic result = new DynamicEntity(deliveryTime);

			result.Id = deliveryTime.Id;
			result.Name = deliveryTime.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.DisplayLocale = deliveryTime.DisplayLocale;
			result.ColorHexValue = deliveryTime.ColorHexValue;
			result.DisplayOrder = deliveryTime.DisplayOrder;

			result._Localized = GetLocalized(ctx, deliveryTime, x => x.Name);

			return result;
		}

		private void ToDeliveryTime(DataExportTaskContext ctx, dynamic parent, int? deliveryTimeId)
		{
			if (ctx.DeliveryTimes != null)
			{
				if (deliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(deliveryTimeId.Value))
					parent.DeliveryTime = ToDynamic(ctx, ctx.DeliveryTimes[deliveryTimeId.Value]);
				else
					parent.DeliveryTime = null;
			}
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, QuantityUnit quantityUnit)
		{
			if (quantityUnit == null)
				return null;

			dynamic result = new DynamicEntity(quantityUnit);

			result.Id = quantityUnit.Id;
			result.Name = quantityUnit.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.Description = quantityUnit.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			result.DisplayLocale = quantityUnit.DisplayLocale;
			result.DisplayOrder = quantityUnit.DisplayOrder;
			result.IsDefault = quantityUnit.IsDefault;

			result._Localized = GetLocalized(ctx, quantityUnit,
				x => x.Name,
				x => x.Description);

			return result;
		}

		private void ToQuantityUnit(DataExportTaskContext ctx, dynamic parent, int? quantityUnitId)
		{
			if (ctx.QuantityUnits != null)
			{
				if (quantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(quantityUnitId.Value))
					parent.QuantityUnit = ToDynamic(ctx, ctx.QuantityUnits[quantityUnitId.Value]);
				else
					parent.QuantityUnit = null;
			}
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Picture picture, int thumbPictureSize, int detailsPictureSize)
		{
			if (picture == null)
				return null;

			dynamic result = new DynamicEntity(picture);

			result.Id = picture.Id;
			result.SeoFilename = picture.SeoFilename;
			result.MimeType = picture.MimeType;

			result._ThumbImageUrl = _pictureService.GetPictureUrl(picture, thumbPictureSize, false, ctx.Store.Url);
			result._ImageUrl = _pictureService.GetPictureUrl(picture, detailsPictureSize, false, ctx.Store.Url);
			result._FullSizeImageUrl = _pictureService.GetPictureUrl(picture, 0, false, ctx.Store.Url);

			var relativeUrl = _pictureService.GetPictureUrl(picture);
			result._FileName = relativeUrl.Substring(relativeUrl.LastIndexOf("/") + 1);

			result._ThumbLocalPath = _pictureService.GetThumbLocalPath(picture);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, ProductVariantAttribute pva)
		{
			if (pva == null)
				return null;

			dynamic result = new DynamicEntity(pva);

			result.Id = pva.Id;
			result.TextPrompt = pva.TextPrompt;
			result.IsRequired = pva.IsRequired;
			result.AttributeControlTypeId = pva.AttributeControlTypeId;
			result.DisplayOrder = pva.DisplayOrder;

			dynamic attribute = new DynamicEntity(pva.ProductAttribute);

			attribute.Id = pva.ProductAttribute.Id;
			attribute.Alias = pva.ProductAttribute.Alias;
			attribute.Name = pva.ProductAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			attribute.Description = pva.ProductAttribute.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);

			attribute.Values = pva.ProductVariantAttributeValues
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic value = new DynamicEntity(x);

					value.Id = x.Id;
					value.Alias = x.Alias;
					value.Name = x.GetLocalized(y => y.Name, ctx.Projection.LanguageId ?? 0, true, false);
					value.ColorSquaresRgb = x.ColorSquaresRgb;
					value.PriceAdjustment = x.PriceAdjustment;
					value.WeightAdjustment = x.WeightAdjustment;
					value.IsPreSelected = x.IsPreSelected;
					value.DisplayOrder = x.DisplayOrder;
					value.ValueTypeId = x.ValueTypeId;
					value.LinkedProductId = x.LinkedProductId;
					value.Quantity = x.Quantity;

					value._Localized = GetLocalized(ctx, x, y => y.Name);

					return value;
				})
				.ToList();

			attribute._Localized = GetLocalized(ctx, pva.ProductAttribute,
				x => x.Name,
				x => x.Description);

			result.Attribute = attribute;

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, ProductVariantAttributeCombination pvac)
		{
			if (pvac == null)
				return null;

			dynamic result = new DynamicEntity(pvac);

			ToDeliveryTime(ctx, result, pvac.DeliveryTimeId);
			ToQuantityUnit(ctx, result, pvac.QuantityUnitId);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Manufacturer manufacturer)
		{
			if (manufacturer == null)
				return null;

			dynamic result = new DynamicEntity(manufacturer);

			result.Name = manufacturer.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.SeName = manufacturer.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			result.Description = manufacturer.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);

			result.Picture = null;

			result._Localized = GetLocalized(ctx, manufacturer,
				x => x.Name,
				x => x.Description,
				x => x.MetaKeywords,
				x => x.MetaDescription,
				x => x.MetaTitle);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Category category)
		{
			if (category == null)
				return null;

			dynamic result = new DynamicEntity(category);

			result.Name = category.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.SeName = category.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			result.FullName = category.GetLocalized(x => x.FullName, ctx.Projection.LanguageId ?? 0, true, false);
			result.Description = category.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			result.BottomDescription = category.GetLocalized(x => x.BottomDescription, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaDescription = category.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaTitle = category.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);

			result.Picture = null;

			result._Localized = GetLocalized(ctx, category,
				x => x.Name,
				x => x.FullName,
				x => x.Description,
				x => x.BottomDescription,
				x => x.MetaKeywords,
				x => x.MetaDescription,
				x => x.MetaTitle);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Product product)
		{
			if (product == null)
				return null;

			dynamic result = new DynamicEntity(product);

			result.Name = product.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.SeName = product.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			result.ShortDescription = product.GetLocalized(x => x.ShortDescription, ctx.Projection.LanguageId ?? 0, true, false);
			result.FullDescription = product.GetLocalized(x => x.FullDescription, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaDescription = product.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			result.MetaTitle = product.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			result.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, ctx.Projection.LanguageId ?? 0, true, false);

			result.AppliedDiscounts = null;
			result.TierPrices = null;
			result.ProductAttributes = null;
			result.ProductAttributeCombinations = null;
			result.ProductPictures = null;
			result.ProductCategories = null;
			result.ProductManufacturers = null;
			result.ProductTags = null;
			result.ProductSpecificationAttributes = null;
			result.ProductBundleItems = null;

			result._Localized = GetLocalized(ctx, product,
				x => x.Name,
				x => x.ShortDescription,
				x => x.FullDescription,
				x => x.MetaKeywords,
				x => x.MetaDescription,
				x => x.MetaTitle,
				x => x.BundleTitleText);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Order order)
		{
			if (order == null)
				return null;

			dynamic result = new DynamicEntity(order);

			result.Id = order.Id;
			result.OrderNumber = order.GetOrderNumber();
			result.OrderGuid = order.OrderGuid;
			result.StoreId = order.StoreId;
			result.CustomerId = order.CustomerId;
			result.BillingAddressId = order.BillingAddressId;
			result.ShippingAddressId = order.ShippingAddressId;
			result.OrderStatusId = order.OrderStatusId;
			result.ShippingStatusId = order.ShippingStatusId;
			result.PaymentStatusId = order.PaymentStatusId;
			result.PaymentMethodSystemName = order.PaymentMethodSystemName;
			result.CustomerCurrencyCode = order.CustomerCurrencyCode;
			result.CurrencyRate = order.CurrencyRate;
			result.CustomerTaxDisplayTypeId = order.CustomerTaxDisplayTypeId;
			result.VatNumber = order.VatNumber;
			result.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
			result.OrderSubtotalExclTax = order.OrderSubtotalExclTax;
			result.OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountInclTax;
			result.OrderSubTotalDiscountExclTax = order.OrderSubTotalDiscountExclTax;
			result.OrderShippingInclTax = order.OrderShippingInclTax;
			result.OrderShippingExclTax = order.OrderShippingExclTax;
			result.OrderShippingTaxRate = order.OrderShippingTaxRate;
			result.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
			result.PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax;
			result.PaymentMethodAdditionalFeeTaxRate = order.PaymentMethodAdditionalFeeTaxRate;
			result.TaxRates = order.TaxRates;
			result.OrderTax = order.OrderTax;
			result.OrderDiscount = order.OrderDiscount;
			result.OrderTotal = order.OrderTotal;
			result.RefundedAmount = order.RefundedAmount;
			result.RewardPointsWereAdded = order.RewardPointsWereAdded;
			result.CheckoutAttributeDescription = order.CheckoutAttributeDescription;
			result.CheckoutAttributesXml = order.CheckoutAttributesXml;
			result.CustomerLanguageId = order.CustomerLanguageId;
			result.AffiliateId = order.AffiliateId;
			result.CustomerIp = order.CustomerIp;
			result.AllowStoringCreditCardNumber = order.AllowStoringCreditCardNumber;
			result.CardType = order.CardType;
			result.CardName = order.CardName;
			result.CardNumber = order.CardNumber;
			result.MaskedCreditCardNumber = order.MaskedCreditCardNumber;
			result.CardCvv2 = order.CardCvv2;
			result.CardExpirationMonth = order.CardExpirationMonth;
			result.CardExpirationYear = order.CardExpirationYear;
			result.AllowStoringDirectDebit = order.AllowStoringDirectDebit;
			result.DirectDebitAccountHolder = order.DirectDebitAccountHolder;
			result.DirectDebitAccountNumber = order.DirectDebitAccountNumber;
			result.DirectDebitBankCode = order.DirectDebitBankCode;
			result.DirectDebitBankName = order.DirectDebitBankName;
			result.DirectDebitBIC = order.DirectDebitBIC;
			result.DirectDebitCountry = order.DirectDebitCountry;
			result.DirectDebitIban = order.DirectDebitIban;
			result.CustomerOrderComment = order.CustomerOrderComment;
			result.AuthorizationTransactionId = order.AuthorizationTransactionId;
			result.AuthorizationTransactionCode = order.AuthorizationTransactionCode;
			result.AuthorizationTransactionResult = order.AuthorizationTransactionResult;
			result.CaptureTransactionId = order.CaptureTransactionId;
			result.CaptureTransactionResult = order.CaptureTransactionResult;
			result.SubscriptionTransactionId = order.SubscriptionTransactionId;
			result.PurchaseOrderNumber = order.PurchaseOrderNumber;
			result.PaidDateUtc = order.PaidDateUtc;
			result.ShippingMethod = order.ShippingMethod;
			result.ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName;
			result.Deleted = order.Deleted;
			result.CreatedOnUtc = order.CreatedOnUtc;
			result.UpdatedOnUtc = order.UpdatedOnUtc;
			result.RewardPointsRemaining = order.RewardPointsRemaining;
			result.HasNewPaymentNotification = order.HasNewPaymentNotification;
			result.OrderStatus = order.OrderStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			result.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			result.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);

			result.Customer = null;
			result.BillingAddress = null;
			result.ShippingAddress = null;
			result.Store = null;
			result.Shipments = null;
			result.RedeemedRewardPointsEntry = ToDynamic(ctx, order.RedeemedRewardPointsEntry);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, OrderItem orderItem)
		{
			if (orderItem == null)
				return null;

			dynamic result = new DynamicEntity(orderItem);

			result.Id = orderItem.Id;
			result.OrderItemGuid = orderItem.OrderItemGuid;
			result.OrderId = orderItem.OrderId;
			result.ProductId = orderItem.ProductId;
			result.Quantity = orderItem.Quantity;
			result.UnitPriceInclTax = orderItem.UnitPriceInclTax;
			result.UnitPriceExclTax = orderItem.UnitPriceExclTax;
			result.PriceInclTax = orderItem.PriceInclTax;
			result.PriceExclTax = orderItem.PriceExclTax;
			result.TaxRate = orderItem.TaxRate;
			result.DiscountAmountInclTax = orderItem.DiscountAmountInclTax;
			result.DiscountAmountExclTax = orderItem.DiscountAmountExclTax;
			result.AttributeDescription = orderItem.AttributeDescription;
			result.AttributesXml = orderItem.AttributesXml;
			result.DownloadCount = orderItem.DownloadCount;
			result.IsDownloadActivated = orderItem.IsDownloadActivated;
			result.LicenseDownloadId = orderItem.LicenseDownloadId;
			result.ItemWeight = orderItem.ItemWeight;
			result.BundleData = orderItem.BundleData;
			result.ProductCost = orderItem.ProductCost;

			result.Product = ToDynamic(ctx, orderItem.Product);

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Shipment shipment)
		{
			if (shipment == null)
				return null;

			dynamic result = new DynamicEntity(shipment);

			result.Id = shipment.Id;
			result.OrderId = shipment.OrderId;
			result.TrackingNumber = shipment.TrackingNumber;
			result.TotalWeight = shipment.TotalWeight;
			result.ShippedDateUtc = shipment.ShippedDateUtc;
			result.DeliveryDateUtc = shipment.DeliveryDateUtc;
			result.CreatedOnUtc = shipment.CreatedOnUtc;

			result.ShipmentItems = shipment.ShipmentItems
				.Select(x =>
				{
					dynamic exp = new DynamicEntity(x);

					exp.Id = x.Id;
					exp.ShipmentId = x.ShipmentId;
					exp.OrderItemId = x.OrderItemId;
					exp.Quantity = x.Quantity;

					return exp;
				})
				.ToList();

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, Discount discount)
		{
			if (discount == null)
				return null;

			dynamic result = new DynamicEntity(discount);

			result.Id = discount.Id;
			result.Name = discount.Name;
			result.DiscountTypeId = discount.DiscountTypeId;
			result.UsePercentage = discount.UsePercentage;
			result.DiscountPercentage = discount.DiscountPercentage;
			result.DiscountAmount = discount.DiscountAmount;
			result.StartDateUtc = discount.StartDateUtc;
			result.EndDateUtc = discount.EndDateUtc;
			result.RequiresCouponCode = discount.RequiresCouponCode;
			result.CouponCode = discount.CouponCode;
			result.DiscountLimitationId = discount.DiscountLimitationId;
			result.LimitationTimes = discount.LimitationTimes;

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, ProductSpecificationAttribute psa)
		{
			if (psa == null)
				return null;

			var option = psa.SpecificationAttributeOption;

			dynamic result = new DynamicEntity(psa);

			result.Id = psa.Id;
			result.ProductId = psa.ProductId;
			result.SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId;
			result.AllowFiltering = psa.AllowFiltering;
			result.ShowOnProductPage = psa.ShowOnProductPage;
			result.DisplayOrder = psa.DisplayOrder;

			dynamic dynAttribute = new DynamicEntity(option.SpecificationAttribute);

			dynAttribute.Id = option.SpecificationAttribute.Id;
			dynAttribute.Name = option.SpecificationAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			dynAttribute.DisplayOrder = option.SpecificationAttribute.DisplayOrder;
			dynAttribute._Localized = GetLocalized(ctx, option.SpecificationAttribute, x => x.Name);

			dynamic dynOption = new DynamicEntity(option);

			dynOption.Id = option.Id;
			dynOption.SpecificationAttributeId = option.SpecificationAttributeId;
			dynOption.Name = option.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			dynOption.DisplayOrder = option.DisplayOrder;
			dynOption._Localized = GetLocalized(ctx, option, x => x.Name);
			dynOption.SpecificationAttribute = dynAttribute;

			result.SpecificationAttributeOption = dynOption;

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, GenericAttribute genericAttribute)
		{
			if (genericAttribute == null)
				return null;

			dynamic result = new DynamicEntity(genericAttribute);

			result.Id = genericAttribute.Id;
			result.EntityId = genericAttribute.EntityId;
			result.KeyGroup = genericAttribute.KeyGroup;
			result.Key = genericAttribute.Key;
			result.Value = genericAttribute.Value;
			result.StoreId = genericAttribute.StoreId;

			return result;
		}

		private dynamic ToDynamic(DataExportTaskContext ctx, NewsLetterSubscription subscription)
		{
			if (subscription == null)
				return null;

			dynamic result = new DynamicEntity(subscription);

			result.Id = subscription.Id;
			result.NewsLetterSubscriptionGuid = subscription.NewsLetterSubscriptionGuid;
			result.Email = subscription.Email;
			result.Active = subscription.Active;
			result.CreatedOnUtc = subscription.CreatedOnUtc;
			result.StoreId = subscription.StoreId;

			return result;
		}

	}
}
