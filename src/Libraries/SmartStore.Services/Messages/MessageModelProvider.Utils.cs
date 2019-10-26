﻿using System;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Topics;
using SmartStore.Services.Media;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Html;
using SmartStore.Utilities;
using System.Collections.Generic;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core;

namespace SmartStore.Services.Messages
{
	public partial class MessageModelProvider
	{
		private void ApplyCustomerContentPart(IDictionary<string, object> model, CustomerContent content, MessageContext ctx)
		{
			model["CustomerId"] = content.CustomerId;
			model["IpAddress"] = content.IpAddress;
			model["CreatedOn"] = ToUserDate(content.CreatedOnUtc, ctx);
			model["UpdatedOn"] = ToUserDate(content.UpdatedOnUtc, ctx);
		}

		private string BuildUrl(string url, MessageContext ctx)
		{
			return ctx.BaseUri.GetLeftPart(UriPartial.Authority) + url.EnsureStartsWith("/");
		}

		private string BuildRouteUrl(object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.GetLeftPart(UriPartial.Authority) + _urlHelper.RouteUrl(routeValues);
		}

		private string BuildRouteUrl(string routeName, object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.GetLeftPart(UriPartial.Authority) + _urlHelper.RouteUrl(routeName, routeValues);
		}

		private string BuildActionUrl(string action, string controller, object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.GetLeftPart(UriPartial.Authority) + _urlHelper.Action(action, controller, routeValues);
		}

		private void PublishModelPartCreatedEvent<T>(T source, dynamic part) where T : class
		{
			_services.EventPublisher.Publish(new MessageModelPartCreatedEvent<T>(source, part));
		}

		private string GetLocalizedValue(MessageContext messageContext, ProviderMetadata metadata, string propertyName, Func<ProviderMetadata, string> fallback)
		{
			// TODO: (mc) this actually belongs to PluginMediator, but we simply cannot add a dependency to framework from here. Refactor later!

			Guard.NotNull(metadata, nameof(metadata));

			string systemName = metadata.SystemName;
			var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			string result = _services.Localization.GetResource(resourceName, messageContext.Language.Id, false, "", true);

			if (result.IsEmpty())
				result = fallback(metadata);

			return result;
		}

		private object GetTopic(string topicSystemName, MessageContext ctx)
		{
			var topicService = _services.Resolve<ITopicService>();

			// Load by store
			var topic = topicService.GetTopicBySystemName(topicSystemName, ctx.StoreId ?? 0, false);

			string body = topic?.GetLocalized(x => x.Body, ctx.Language);
			if (body.HasValue())
			{
				body = HtmlUtils.RelativizeFontSizes(body);
			}

			return new
			{
				Title = topic?.GetLocalized(x => x.Title, ctx.Language).Value.NullEmpty(),
				Body = body.NullEmpty()
			};
		}

		private string GetDisplayNameForCustomer(Customer customer)
		{
			return customer.GetFullName().NullEmpty() ?? customer.Username ?? customer.FindEmail();
		}

		private string GetBoolResource(bool value, MessageContext ctx)
		{
			return _services.Localization.GetResource(value ? "Common.Yes" : "Common.No", ctx.Language.Id);
		}

		private DateTime? ToUserDate(DateTime? utcDate, MessageContext messageContext)
		{
			if (utcDate == null)
				return null;

			return _services.DateTimeHelper.ConvertToUserTime(
				utcDate.Value, 
				TimeZoneInfo.Utc, 
				_services.DateTimeHelper.GetCustomerTimeZone(messageContext.Customer));
		}

		private Money FormatPrice(decimal price, Order order, MessageContext messageContext)
		{
			return FormatPrice(price, order.CustomerCurrencyCode, messageContext, order.CurrencyRate);
		}

		private Money FormatPrice(decimal price, MessageContext messageContext, decimal exchangeRate = 1)
		{
			return FormatPrice(price, (Currency)null, messageContext, exchangeRate);
		}

		private Money FormatPrice(decimal price, string currencyCode, MessageContext messageContext, decimal exchangeRate = 1)
		{
			return FormatPrice(
				price,
				_services.Resolve<ICurrencyService>().GetCurrencyByCode(currencyCode) ?? new Currency { CurrencyCode = currencyCode },
				messageContext,
				exchangeRate);
		}

		private Money FormatPrice(decimal price, Currency currency, MessageContext messageContext, decimal exchangeRate = 1)
		{
			if (exchangeRate != 1)
			{
				price = _services.Resolve<ICurrencyService>().ConvertCurrency(price, exchangeRate);
			}

			if (currency == null)
			{
				currency = _services.Resolve<IWorkContext>().WorkingCurrency;
			}

			return new Money(price, currency);
		}

		private PictureInfo GetPictureFor(Product product, string attributesXml)
		{
			var pictureService = _services.PictureService;
			var attrParser = _services.Resolve<IProductAttributeParser>();

			PictureInfo pictureInfo = null;

			if (attributesXml.HasValue())
			{
				var combination = attrParser.FindProductVariantAttributeCombination(product.Id, attributesXml);

				if (combination != null)
				{
					var picturesIds = combination.GetAssignedPictureIds();
					if (picturesIds != null && picturesIds.Length > 0)
					{
						pictureInfo = pictureService.GetPictureInfo(picturesIds[0]);
					}	
				}
			}

			if (pictureInfo == null)
			{
				pictureInfo = pictureService.GetPictureInfo(product.MainPictureId);
			}

			if (pictureInfo == null && !product.VisibleIndividually && product.ParentGroupedProductId > 0)
			{
				pictureInfo = pictureService.GetPictureInfo(pictureService.GetPicturesByProductId(product.ParentGroupedProductId, 1).FirstOrDefault());
			}

			return pictureInfo;
		}

		private object[] Concat(params object[] values)
		{
			return values.Where(x => CommonHelper.IsTruthy(x)).ToArray();
		}
	}
}
