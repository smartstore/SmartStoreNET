using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
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

		public static string Picture(this UrlHelper urlHelper, 
			int? pictureId,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			return pictureService.GetPictureUrl(pictureId.GetValueOrDefault(), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
		}

		public static string Picture(this UrlHelper urlHelper,
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			return pictureService.GetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
		}
	}
}
