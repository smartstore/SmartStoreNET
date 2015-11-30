using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;

namespace SmartStore
{
    
    public static class HttpExtensions
    {
        private const string HTTP_CLUSTER_VAR = "HTTP_CLUSTER_HTTPS";
        
        /// <summary>
        /// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol). 
        /// Works with Cloud's load balancers.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsSecureConnection(this HttpRequestBase request)
        {
            return (request.IsSecureConnection || (request.ServerVariables[HTTP_CLUSTER_VAR] != null || request.ServerVariables[HTTP_CLUSTER_VAR] == "on"));
        }

        /// <summary>
        /// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol). 
        /// Works with Cloud's load balancers.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsSecureConnection(this HttpRequest request)
        {
            return (request.IsSecureConnection || (request.ServerVariables[HTTP_CLUSTER_VAR] != null || request.ServerVariables[HTTP_CLUSTER_VAR] == "on"));
        }

		public static void SetFormsAuthenticationCookie(this HttpWebRequest webRequest, HttpRequestBase httpRequest)
		{
			Guard.ArgumentNotNull(() => webRequest);
			Guard.ArgumentNotNull(() => httpRequest);

			var authCookie = httpRequest.Cookies[FormsAuthentication.FormsCookieName];
			var sendCookie = new Cookie(authCookie.Name, authCookie.Value, authCookie.Path, httpRequest.Url.Host);

			if (webRequest.CookieContainer == null)
			{
				webRequest.CookieContainer = new CookieContainer();
			}
			webRequest.CookieContainer.Add(sendCookie);
		}
    }

}
