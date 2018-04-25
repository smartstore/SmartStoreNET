using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Media;
using SmartStore.Services.Topics;
using SmartStore.Services.Seo;
using System.Web.Routing;

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

		public static string TopicUrl(this UrlHelper urlHelper, string systemName, bool popup = false)
		{
			var workContext = EngineContext.Current.Resolve<IWorkContext>();
			var cache = EngineContext.Current.Resolve<ICacheManager>();

			var cacheKey = "Hallo";
			var seName = cache.Get(cacheKey, () => 
			{
				var topicService = EngineContext.Current.Resolve<ITopicService>();
				var topic = topicService.GetTopicBySystemName(systemName);

				return topic?.GetSeName() ?? string.Empty;
			});

			var routeValues = new RouteValueDictionary { ["SeName"] = seName };
			if (popup)
				routeValues["popup"] = true;

			return urlHelper.RouteUrl("Topic", routeValues);
		}
	}
}
