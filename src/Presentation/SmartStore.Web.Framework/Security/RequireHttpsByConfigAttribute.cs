using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireHttpsByConfigAttribute : FilterAttribute, IAuthorizationFilter
    {
		public RequireHttpsByConfigAttribute(SslRequirement sslRequirement)
        {
            this.SslRequirement = sslRequirement;
        }

		public Lazy<SecuritySettings> SecuritySettings { get; set; }
		public Lazy<IStoreContext> StoreContext { get; set; }
		public Lazy<IWebHelper> WebHelper { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            // don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;
            
            // only redirect for GET requests, 
            // otherwise the browser might not propagate the verb and request body correctly.
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

            var currentConnectionSecured = filterContext.HttpContext.Request.IsSecureConnection();

			var securitySettings = SecuritySettings.Value;
            if (securitySettings.ForceSslForAllPages)
            {
                // all pages are forced to be SSL no matter of the specified value
                this.SslRequirement = SslRequirement.Yes;
            }

            switch (this.SslRequirement)
            {
                case SslRequirement.Yes:
                    {
                        if (!currentConnectionSecured)
                        {
							var storeContext = StoreContext.Value;
							var currentStore = storeContext.CurrentStore;

							if (currentStore != null && currentStore.GetSecurityMode() > HttpSecurityMode.Unsecured)
                            {
                                // redirect to HTTPS version of page
                                // string url = "https://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
								var webHelper = WebHelper.Value;
                                string url = webHelper.GetThisPageUrl(true, true);
                                filterContext.Result = new RedirectResult(url, true);
                            }
                        }
                    }
                    break;
                case SslRequirement.No:
                    {
                        if (currentConnectionSecured)
                        {
                            var webHelper = WebHelper.Value;

                            // redirect to HTTP version of page
                            // string url = "http://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
                            string url = webHelper.GetThisPageUrl(true, false);
                            filterContext.Result = new RedirectResult(url, true);
                        }
                    }
                    break;
                case SslRequirement.Retain:
                    {
                        //do nothing
                    }
                    break;
                default:
					throw new SmartException("Unsupported SslRequirement parameter");
            }
        }

        public SslRequirement SslRequirement { get; set; }
    }
}
