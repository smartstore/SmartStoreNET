using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Controllers
{
    public class StoreIpAddressAttribute : ActionFilterAttribute
    {

		public Lazy<IWebHelper> WebHelper { get; set; }
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<ICustomerService> CustomerService { get; set; }
		
		public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            var webHelper = this.WebHelper.Value;

            //update IP address
            string currentIpAddress = webHelper.GetCurrentIpAddress();
            if (!String.IsNullOrEmpty(currentIpAddress))
            {
                var workContext = WorkContext.Value;
                var customer = workContext.CurrentCustomer;
				
                if (!currentIpAddress.Equals(customer.LastIpAddress, StringComparison.InvariantCultureIgnoreCase))
                {
                    var customerService = CustomerService.Value;
                    customer.LastIpAddress = currentIpAddress;
                    customerService.UpdateCustomer(customer);
                }
            }
        }
    }
}
