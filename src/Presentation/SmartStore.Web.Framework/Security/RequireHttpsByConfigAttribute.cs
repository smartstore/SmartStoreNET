using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Web.Framework.Security
{
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
            // don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

			// only redirect for GET requests, 
			// otherwise the browser might not propagate the verb and request body correctly.
			if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

			var securitySettings = SecuritySettings.Value;
			var isLocalRequest = filterContext.HttpContext.Request.IsLocal;

			if (!securitySettings.UseSslOnLocalhost && isLocalRequest)
				return;

			var webHelper = WebHelper.Value;

			var currentConnectionSecured = webHelper.IsCurrentConnectionSecured();
			var storeContext = StoreContext.Value;
			var currentStore = storeContext.CurrentStore;

			if (currentStore.ForceSslForAllPages)
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
							if (currentStore != null && currentStore.GetSecurityMode() > HttpSecurityMode.Unsecured)
                            {
                                // redirect to HTTPS version of page
                                // string url = "https://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
								
                                string url = webHelper.GetThisPageUrl(true, true);
                                filterContext.Result = new RedirectResult(url, !isLocalRequest);
                            }
                        }
                    }
                    break;
                case SslRequirement.No:
                    {
                        if (currentConnectionSecured)
                        {
                            // redirect to HTTP version of page
                            // string url = "http://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
                            string url = webHelper.GetThisPageUrl(true, false);
                            filterContext.Result = new RedirectResult(url, !isLocalRequest);
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
