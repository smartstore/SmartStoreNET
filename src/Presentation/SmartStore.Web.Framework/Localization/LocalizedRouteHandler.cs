using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc; 

namespace SmartStore.Web.Framework.Localization
{
    
    public class LocalizedRouteHandler : MvcRouteHandler
    {
        //public const string CULTURECODE_REQUESTED_CACHEKEY = "RouteValue_cultureCode";
        //public const string CULTURECODE_DEFAULT_CACHEKEY = "RouteDefault_cultureCode";
        
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            //////string cultureCode;
            //////if (requestContext.RouteData.Values.TryGetCultureCode(out cultureCode)) 
            //////{
            //////    // save it for the request, so 'WorkingLanguage' can read it (instead of parsing the raw url).
            //////    requestContext.HttpContext.Items[CULTURECODE_REQUESTED_CACHEKEY] = cultureCode;
            //////}

            //////requestContext.HttpContext.SetRouteData(requestContext.RouteData);
            
            ////var route = requestContext.RouteData.Route as LocalizedRoute;
            ////if (route != null)
            ////{
            ////    requestContext.HttpContext.Items[CULTURECODE_DEFAULT_CACHEKEY] = route.DefaultCultureCode;
            ////}

            //var httpContext = requestContext.HttpContext;
            //string virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            //string cultureCodeFromRouteValues = requestContext.RouteData.Values.GetCultureCode();

            //bool cultureCodeSpecified = cultureCodeFromRouteValues != null && virtualPath.IsPathPrecededByCultureCode(cultureCodeFromRouteValues);

            //requestContext.RouteData.SetCultureCodeSpecified(cultureCodeSpecified);

            return base.GetHttpHandler(requestContext);
        }
    }

}
