using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Web.Framework.Security
{
    public class AdminValidateIpAddressAttribute : FilterAttribute, IAuthorizationFilter
    {
        public Lazy<IWebHelper> WebHelper { get; set; }
        public Lazy<SecuritySettings> SecuritySettings { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            // don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            // Prevent lockout
            if (filterContext.HttpContext.Request.IsLocal)
                return;

            bool ok = false;
            var ipAddresses = SecuritySettings.Value.AdminAreaAllowedIpAddresses;
            if (ipAddresses != null && ipAddresses.Count > 0)
            {
                var webHelper = WebHelper.Value;
                var curIpAddress = webHelper.GetCurrentIpAddress();
                foreach (string ip in ipAddresses)
                    if (ip.Equals(curIpAddress, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ok = true;
                        break;
                    }
            }
            else
            {
                //no restrictions
                ok = true;
            }

            if (!ok)
            {
                //ensure that it's not 'Access denied' page
                var webHelper = WebHelper.Value;
                var thisPageUrl = webHelper.GetThisPageUrl(false);
                if (!thisPageUrl.StartsWith(string.Format("{0}admin/security/accessdenied", webHelper.GetStoreLocation()), StringComparison.InvariantCultureIgnoreCase))
                {
                    //redirect to 'Access denied' page
                    filterContext.Result = new RedirectResult(webHelper.GetStoreLocation() + "admin/security/accessdenied");
                }
            }
        }
    }
}
