using System;
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
            {
                return urlHelper.Action("Login", "Customer", new { returnUrl, area = "" });
            }

			return urlHelper.Action("Login", "Customer", new { area = "" });
        }

        public static string LogOff(this UrlHelper urlHelper, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return urlHelper.Action("Logout", "Customer", new { returnUrl, area = "" });
            }

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

		public static string Picture(this UrlHelper urlHelper, MediaFile picture, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
		{
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			return pictureService.GetUrl(picture, targetSize, fallbackType, host);
		}

		/// <summary>
		/// Resolves a link to a topic page.
		/// </summary>
		/// <param name="systemName">The system name of the topic.</param>
		/// <returns>Link</returns>
		/// <remarks>
		/// This method returns an empty string in following cases:
		/// - the requested page does not exist.
		/// - the current user has no permission to acces the page.
		/// </remarks>
		public static string Topic(this UrlHelper urlHelper, string systemName, bool popup = false)
		{
			Guard.NotEmpty(systemName, nameof(systemName));

			var expression = "topic:" + systemName;
			if (popup)
			{
				expression += "?popup=true";
			}

			return Entity(urlHelper, expression);
		}

		/// <summary>
		/// Resolves a link label for a topic page.
		/// The label is either the page short title or title.
		/// </summary>
		/// <param name="systemName">The system name of the topic.</param>
		/// <returns>Label</returns>
		/// <remarks>
		/// This method returns an empty string if the requested page does not exist.
		/// </remarks>
		public static string TopicLabel(this UrlHelper urlHelper, string systemName)
		{
			Guard.NotEmpty(systemName, nameof(systemName));

			return EntityLabel(urlHelper, "topic:" + systemName);
		}

		/// <summary>
		/// Resolves a link to a system internal entity like product, topic, category or manufacturer.
		/// </summary>
		/// <param name="expression">A link expression as supported by the <see cref="ILinkResolver"/></param>
		/// <returns>Link</returns>
		/// <remarks>
		/// This method returns an empty string in following cases:
		/// - the requested entity does not exist.
		/// - the current user has no permission to acces the entity.
		/// </remarks>
		public static string Entity(this UrlHelper urlHelper, string expression)
		{
			Guard.NotEmpty(expression, nameof(expression));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var link = linkResolver.Resolve(expression);

			if (link.Status == LinkStatus.Ok)
			{
				return link.Link.EmptyNull();
			}

			return string.Empty;
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
		public static string EntityLabel(this UrlHelper urlHelper, string expression)
		{
			Guard.NotEmpty(expression, nameof(expression));

			var linkResolver = EngineContext.Current.Resolve<ILinkResolver>();
			var link = linkResolver.Resolve(expression);

			if (link.Status == LinkStatus.Ok || link.Status == LinkStatus.Forbidden)
			{
				return link.Label;
			}

			return string.Empty;
		}
	}
}
