using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class SecurityController : AdminControllerBase
    {
        private readonly IWorkContext _workContext;

        public SecurityController(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        // AJAX.
        public ActionResult AllAccessPermissions(string selected)
        {
            var systemNames = Services.Permissions.GetAllSystemNames();
            var selectedArr = selected.SplitSafe(",");

            var data = systemNames
                .Select(x => new ChoiceListItem
                {
                    Id = x.Key,
                    Text = x.Value,
                    Selected = selectedArr.Contains(x.Key)
                })
                .ToList();

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult AccessDenied(string pageUrl)
        {
            var customer = _workContext.CurrentCustomer;

            if (customer == null || customer.IsGuest())
            {
                Logger.Info(T("Admin.System.Warnings.AccessDeniedToAnonymousRequest", pageUrl.NaIfEmpty()));
                return View();
            }

            Logger.Info(T("Admin.System.Warnings.AccessDeniedToUser",
                customer.Email.NaIfEmpty(), customer.Email.NaIfEmpty(), pageUrl.NaIfEmpty()));

            return View();
        }
    }
}
