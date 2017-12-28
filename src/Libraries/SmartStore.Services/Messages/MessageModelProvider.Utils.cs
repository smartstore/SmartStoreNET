using System;
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
using System.Text;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Html;
using SmartStore.Utilities;

namespace SmartStore.Services.Messages
{
	public partial class MessageModelProvider
	{
		private string BuildUrl(string url, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + url;
		}

		private string BuildRouteUrl(object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + _urlHelper.RouteUrl(routeValues);
		}

		private string BuildRouteUrl(string routeName, object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + _urlHelper.RouteUrl(routeName, routeValues);
		}

		private string BuildActionUrl(string action, string controller, object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + _urlHelper.Action(action, controller, routeValues);
		}

		private void PublishModelPartCreatedEvent<T>(T source, dynamic part) where T : class
		{
			_services.EventPublisher.Publish(new MessageModelPartCreatedEvent<T>(source, part));
		}

		private string GetLocalizedValue(MessageContext messageContext, ProviderMetadata metadata, string propertyName, Expression<Func<ProviderMetadata, string>> fallback)
		{
			// TODO: (mc) this actually belongs to PluginMediator, but we simply cannot add a dependency to framework from here. Refactor later!

			Guard.NotNull(metadata, nameof(metadata));

			string systemName = metadata.SystemName;
			var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			string result = _services.Localization.GetResource(resourceName, messageContext.Language.Id, false, "", true);

			if (result.IsEmpty())
				result = fallback.Compile()(metadata);

			return result;
		}

		private object GetTopic(string topicSystemName, MessageContext ctx)
		{
			var topicService = _services.Resolve<ITopicService>();

			// Load by store
			var topic = topicService.GetTopicBySystemName(topicSystemName, ctx.Store.Id);
			if (topic == null)
			{
				// Not found. Let's find topic assigned to all stores
				topic = topicService.GetTopicBySystemName(topicSystemName, 0);
			}

			var body = topic?.GetLocalized(x => x.Body, ctx.Language.Id);
			if (body.HasValue())
			{
				body = HtmlUtils.RelativizeFontSizes(body);
			}

			return new
			{
				Title = topic?.GetLocalized(x => x.Title, ctx.Language.Id).NullEmpty(),
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

		private string FormatPrice(decimal price, Order order, MessageContext messageContext)
		{
			return FormatPrice(price, order.CurrencyRate, order.CustomerCurrencyCode, messageContext);
		}

		private string FormatPrice(decimal price, decimal currencyRate, string customerCurrencyCode, MessageContext messageContext)
		{
			var language = messageContext.Language;
			var currencyService = _services.Resolve<ICurrencyService>();
			var priceFormatter = _services.Resolve<IPriceFormatter>();

			return priceFormatter.FormatPrice(currencyService.ConvertCurrency(price, currencyRate), true, customerCurrencyCode, false, language);
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
