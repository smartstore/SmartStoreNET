using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Common;

namespace SmartStore.Web.Framework.Filters
{
    public class StoreLastVisitedPageAttribute : FilterAttribute, IActionFilter
    {
		public Lazy<IWebHelper> WebHelper { get; set; }
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<CustomerSettings> CustomerSettings { get; set; }
		public Lazy<IGenericAttributeService> GenericAttributeService { get; set; }
		public Lazy<IUserAgent> UserAgent { get; set; }

		public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
			if (!DataSettings.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            // don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            if (!CustomerSettings.Value.StoreLastVisitedPage)
                return;

			var customer = WorkContext.Value.CurrentCustomer;

			if (customer == null || customer.IsSystemAccount)
				return;

			var pageUrl = WebHelper.Value.GetThisPageUrl(true);
			var userAgent = UserAgent.Value.RawValue;

			var genericAttributeService = GenericAttributeService.Value;

			if (pageUrl.HasValue())
            {
				var previousPageUrl = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
				if (!pageUrl.IsCaseInsensitiveEqual(previousPageUrl))
				{
					genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastVisitedPage, pageUrl);
				}
            }

			if (userAgent.HasValue())
			{
				var previousUserAgent = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastUserAgent);
				if (!userAgent.IsCaseInsensitiveEqual(previousUserAgent))
				{
					genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastUserAgent, userAgent);
				}
			}
		}

		public virtual void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}
