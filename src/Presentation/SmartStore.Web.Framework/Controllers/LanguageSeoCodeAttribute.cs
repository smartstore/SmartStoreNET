using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Controllers
{
    /// <summary>
    /// Attribute which ensures that store URL contains a language SEO code if "SEO friendly URLs with multiple languages" setting is enabled
    /// </summary>
    public class LanguageSeoCodeAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
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

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var route = filterContext.RouteData.Route as LocalizedRoute;
            
            if (!route.SeoFriendlyUrlsEnabled)
                return;

            //string requestedCultureCode;
            if (filterContext.RouteData.IsCultureCodeSpecified()) 
            {
                // a lang specific url is already requested
                return;
            }

            //// add culture code of working language to route values
            //var workContext = EngineContext.Current.Resolve<IWorkContext>();
            //filterContext.RouteData.Values.SetCultureCode(workContext.WorkingLanguage.UniqueSeoCode);

            //filterContext.Result = new RedirectToRouteResult(filterContext.RouteData.Values);

            //process current URL
            var pageUrl = filterContext.HttpContext.Request.RawUrl;
            string applicationPath = filterContext.HttpContext.Request.ApplicationPath;
            if (pageUrl.IsLocalizedUrl(applicationPath, true))
                //already localized URL
                return;
            //add language code to URL
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            pageUrl = pageUrl.AddLanguageSeoCodeToRawUrl(applicationPath, workContext.WorkingLanguage);
            filterContext.Result = new RedirectResult(pageUrl);
        }

    }
}
