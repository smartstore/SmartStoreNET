using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Filters
{
    public class CheckAffiliateAttribute : FilterAttribute, IActionFilter
    {
        public Lazy<IAffiliateService> AffiliateService { get; set; }
        public Lazy<IWorkContext> WorkContext { get; set; }
        public Lazy<ICustomerService> CustomerService { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext?.Request == null)
                return;

            var request = filterContext.HttpContext.Request;

            if (filterContext.IsChildAction)
                return;

            if (request.QueryString != null && request.QueryString["AffiliateId"] != null)
            {
                var affiliateId = Convert.ToInt32(request.QueryString["AffiliateId"]);

                if (affiliateId > 0)
                {
                    var affiliate = AffiliateService.Value.GetAffiliateById(affiliateId);
                    if (affiliate != null && !affiliate.Deleted && affiliate.Active)
                    {
                        var customer = WorkContext.Value.CurrentCustomer;
                        if (customer != null && !customer.IsSystemAccount && customer.AffiliateId != affiliate.Id)
                        {
                            customer.AffiliateId = affiliate.Id;
                            CustomerService.Value.UpdateCustomer(customer);
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
