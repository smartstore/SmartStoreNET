using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Cms;
using SmartStore.Services.Media;

namespace SmartStore.Web.Framework
{
	public static class UrlHelperExtensions
    {
        public static string LogOn(this UrlHelper urlHelper, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("Login", "Customer", new { ReturnUrl = returnUrl, area = "" });

			return urlHelper.Action("Login", "Customer", new { area = "" });
        }

        public static string LogOff(this UrlHelper urlHelper, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("Logout", "Customer", new { ReturnUrl = returnUrl, area = "" });

			return urlHelper.Action("Logout", "Customer", new { area = "" });
        }

		public static string Referrer(this UrlHelper urlHelper, string fallbackUrl = "")
		{
			var request = urlHelper.RequestContext.HttpContext.Request;
			if (request.UrlReferrer != null && request.UrlReferrer.ToString().HasValue())
			{
				return request.UrlReferrer.ToString();
			}

			return fallbackUrl;
		}

		public static string Picture(this UrlHelper urlHelper, int? pictureId, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
		{
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			return pictureService.GetUrl(pictureId.GetValueOrDefault(), targetSize, fallbackType, host);
		}

		public static string Picture(this UrlHelper urlHelper, Picture picture, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
		{
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			return pictureService.GetUrl(picture, targetSize, fallbackType, host);
		}

		/// <summary>
		/// Resolves a link to a system internal entity like product, topic, category or manufacturer.
		/// </summary>
		/// <param name="expression">A link expression as supported by the <see cref="ILinkResolver"/></param>
		/// <param name="q">Optional query string, which should be appended to the resolved link.</param>
		/// <returns>Link</returns>
		/// <remarks>
		/// This method returns an empty string in following cases:
		/// - the requested entity does not exist.
		/// - the current user has no permission to acces the entity.
		/// </remarks>
		public static IHtmlString Entity(this UrlHelper urlHelper, string expression, string q = null)
		{
			Guard.NotEmpty(expression, nameof(expression));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var link = linkResolver.Resolve(expression);

			if (link.Status == LinkStatus.Ok)
			{
				return link;
			}

			return MvcHtmlString.Empty;
		}

		/// <summary>
		/// Resolves a link label for a system internal entity like product, topic, category or manufacturer.
		/// The label is either the entity short title, title or name, whichever is applicable.
		/// </summary>
		/// <param name="expression">A link expression as supported by the <see cref="ILinkResolver"/></param>
		/// <returns>Label</returns>
		/// <remarks>
		/// This method returns an empty string if the requested entity does not exist.
		/// </remarks>
		public static IHtmlString EntityLabel(this UrlHelper urlHelper, string expression)
		{
			Guard.NotEmpty(expression, nameof(expression));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var link = linkResolver.Resolve(expression);

			if (link.Status == LinkStatus.Ok || link.Status == LinkStatus.Forbidden)
			{
				return MvcHtmlString.Create(link.Label);
			}

			return MvcHtmlString.Empty;
		}

		public static string TopicUrl(this UrlHelper urlHelper, string systemName, bool popup = false)
		{
			Guard.NotEmpty(systemName, nameof(systemName));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var expression = "topic:" + systemName;
			if (popup)
			{
				expression += "?popup=true";
			}

			var link = linkResolver.Resolve(expression);

			if (link.Status == LinkStatus.Ok)
			{
				return link.Link;
			}

			return string.Empty;
		}

		public static string TopicSeName(this UrlHelper urlHelper, string systemName)
		{
			Guard.NotEmpty(systemName, nameof(systemName));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var link = linkResolver.Resolve("topic:" + systemName);
			return link.Slug;
		}

		public static string TopicLinkText(this UrlHelper urlHelper, string systemName)
		{
			Guard.NotEmpty(systemName, nameof(systemName));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var link = linkResolver.Resolve("topic:" + systemName);

			if (link.Status == LinkStatus.Ok)
			{
				return link.Label;
			}
			
			return string.Empty;
		}

		//private static TopicLinkData GetTopicLinkData(string systemName)
		//{
		//	var container = EngineContext.Current.ContainerManager;

		//	var workContext = container.Resolve<IWorkContext>();
		//	var storeId = container.Resolve<IStoreContext>().CurrentStore.Id;
		//	var cache = container.Resolve<ICacheManager>();

		//	var cacheKey = string.Format(FrameworkCacheConsumer.TOPIC_SENAME_BY_SYSTEMNAME, systemName.ToLower(), workContext.WorkingLanguage.Id, storeId, workContext.CurrentCustomer.GetRolesIdent());
		//	var data = cache.Get(cacheKey, () =>
		//	{
		//		var topicService = container.Resolve<ITopicService>();
		//		var topic = topicService.GetTopicBySystemName(systemName, storeId, true);

		//		if (topic == null || !topic.IsPublished)
		//			return null;

		//		var seName = topic.GetSeName();
		//		if (seName.IsEmpty())
		//			return null;

		//		return new TopicLinkData
		//		{
		//			SeName = seName,
		//			LinkText = topic.GetLocalized(x => x.ShortTitle).Value.NullEmpty() ?? topic.GetLocalized(x => x.Title).Value.NullEmpty() ?? seName
		//		};
		//	});

		//	return data;
		//}
	}

	[Serializable]
	public class TopicLinkData
	{
		public string SeName { get; set; }
		public string LinkText { get; set; }
	}
}
