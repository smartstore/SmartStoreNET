using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Filters
{
    public class StoreLastVisitedPageAttribute : FilterAttribute, IActionFilter
    {
        public Lazy<IWebHelper> WebHelper { get; set; }
        public Lazy<IWorkContext> WorkContext { get; set; }
        public Lazy<CustomerSettings> CustomerSettings { get; set; }
        public Lazy<IGenericAttributeService> GenericAttributeService { get; set; }
        public Lazy<IUserAgent> UserAgent { get; set; }
        public Lazy<ICustomerService> CustomerService { get; set; }

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

            if (!CustomerSettings.Value.StoreLastVisitedPage)
                return;

            var customer = WorkContext.Value.CurrentCustomer;

            if (customer == null || customer.Deleted || customer.IsSystemAccount)
                return;

            var pageUrl = WebHelper.Value.GetThisPageUrl(true);
            var userAgent = UserAgent.Value.RawValue;

            if (pageUrl.HasValue())
            {
                var previousPageUrl = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
                if (!pageUrl.IsCaseInsensitiveEqual(previousPageUrl))
                {
                    GenericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.LastVisitedPage, pageUrl);
                }
            }

            if (userAgent.HasValue())
            {
                var previousUserAgent = customer.LastUserAgent;
                if (!userAgent.IsCaseInsensitiveEqual(previousUserAgent))
                {
                    try
                    {
                        customer.LastUserAgent = userAgent;
                        CustomerService.Value.UpdateCustomer(customer);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        // The exception may occur on the first call after a migration.
                        if (!ioe.IsAlreadyAttachedEntityException())
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}
