using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Filters
{
    public class StoreIpAddressAttribute : FilterAttribute, IActionFilter
    {
		public Lazy<IWebHelper> WebHelper { get; set; }
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<ICustomerService> CustomerService { get; set; }
		public Lazy<PrivacySettings> PrivacySettings { get; set; }

		public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            // Don't apply filter to child methods.
            if (filterContext.IsChildAction)
                return;

            // Only GET requests.
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

			if (!PrivacySettings.Value.StoreLastIpAddress)
				return;

			// Update IP address.
			var webHelper = WebHelper.Value;
			var currentIpAddress = webHelper.GetCurrentIpAddress();
            if (!String.IsNullOrEmpty(currentIpAddress))
            {
                var customer = WorkContext.Value.CurrentCustomer;
                if (customer != null && !currentIpAddress.Equals(customer.LastIpAddress, StringComparison.InvariantCultureIgnoreCase))
                {
                    var customerService = CustomerService.Value;
                    customer.LastIpAddress = currentIpAddress;
                    customerService.UpdateCustomer(customer);
                }
            }
        }

		public virtual void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}
