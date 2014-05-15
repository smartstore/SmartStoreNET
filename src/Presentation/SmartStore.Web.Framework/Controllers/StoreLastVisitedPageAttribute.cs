using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Common;

namespace SmartStore.Web.Framework.Controllers
{
    public class StoreLastVisitedPageAttribute : ActionFilterAttribute
    {

		public Lazy<IWebHelper> WebHelper { get; set; }
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<CustomerSettings> CustomerSettings { get; set; }
		public Lazy<IGenericAttributeService> GenericAttributeService { get; set; }
		
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

            var customerSettings = CustomerSettings.Value;
            if (!customerSettings.StoreLastVisitedPage)
                return;

            var webHelper = this.WebHelper.Value;
            var pageUrl = webHelper.GetThisPageUrl(true);
            if (!String.IsNullOrEmpty(pageUrl))
            {
                var workContext = WorkContext.Value;
                var genericAttributeService = GenericAttributeService.Value;

                var previousPageUrl = workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
                if (!pageUrl.Equals(previousPageUrl))
                {
                    genericAttributeService.SaveAttribute(workContext.CurrentCustomer, SystemCustomerAttributeNames.LastVisitedPage, pageUrl);
                }
            }
        }
    }
}
