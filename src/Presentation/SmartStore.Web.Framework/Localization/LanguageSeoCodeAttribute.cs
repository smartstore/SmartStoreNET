using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Localization
{
    /// <summary>
    /// Attribute which ensures that store URL contains a language SEO code if "SEO friendly URLs with multiple languages" setting is enabled
    /// </summary>
    public class LanguageSeoCodeAttribute : FilterAttribute, IActionFilter
    {
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<ILanguageService> LanguageService { get; set; }
		public Lazy<LocalizationSettings> LocalizationSettings { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            // ensure that this route is registered and localizable (LocalizedRoute in RouteProvider.cs)
            if (filterContext.RouteData == null || filterContext.RouteData.Route == null || !(filterContext.RouteData.Route is LocalizedRoute))
                return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

            var localizationSettings = LocalizationSettings.Value;
            if (!localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                return;
            
            // process current URL
			var workContext = WorkContext.Value;
			var languageService = LanguageService.Value;
            var workingLanguage = workContext.WorkingLanguage;
            var helper = new LocalizedUrlHelper(filterContext.HttpContext.Request, true);
			string defaultSeoCode = languageService.GetDefaultLanguageSeoCode();
			
            string seoCode;
            if (helper.IsLocalizedUrl(out seoCode)) 
            {
				if (!languageService.IsPublishedLanguage(seoCode))
                {
					var descriptor = filterContext.ActionDescriptor;
					
					// language is not defined in system or not assigned to store
					if (localizationSettings.InvalidLanguageRedirectBehaviour == InvalidLanguageRedirectBehaviour.ReturnHttp404)
                    {
						filterContext.Result = new ViewResult
						{
							ViewName = "NotFound",
							MasterName = (string)null,
							ViewData = new ViewDataDictionary<HandleErrorInfo>(new HandleErrorInfo(new HttpException(404, "The resource does not exist."), descriptor.ActionName, descriptor.ControllerDescriptor.ControllerName)),
							TempData = filterContext.Controller.TempData
						};
						filterContext.RouteData.Values["StripInvalidSeoCode"] = true;
						filterContext.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
						filterContext.RequestContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    }
                    else if (localizationSettings.InvalidLanguageRedirectBehaviour == InvalidLanguageRedirectBehaviour.FallbackToWorkingLanguage)
                    {
                        helper.StripSeoCode();
                        filterContext.Result = new RedirectResult(helper.GetAbsolutePath(), true);
                    }
                }
                else
                {
                    // redirect default language (if desired)
                    if (seoCode == defaultSeoCode && localizationSettings.DefaultLanguageRedirectBehaviour == DefaultLanguageRedirectBehaviour.StripSeoCode)
                    {
                        helper.StripSeoCode();
                        filterContext.Result = new RedirectResult(helper.GetAbsolutePath(), true);
                    }
                }

                // already localized URL, skip the rest
                return;
            }

            // keep default language prefixless (if desired)
            if (workingLanguage.UniqueSeoCode == defaultSeoCode && (int)(localizationSettings.DefaultLanguageRedirectBehaviour) > 0)
            {
                return;
            }

            // add language code to URL
            helper.PrependSeoCode(workingLanguage.UniqueSeoCode);
            filterContext.Result = new RedirectResult(helper.GetAbsolutePath());
        }

		public virtual void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}
