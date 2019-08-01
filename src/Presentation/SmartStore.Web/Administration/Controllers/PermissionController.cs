using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Security;
using SmartStore.Core;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class PermissionController : AdminControllerBase
    {
        private readonly IPermissionService2 _permissionService;

        public PermissionController(IPermissionService2 permissionService)
        {
            _permissionService = permissionService;
        }

        // Ajax.
        public ActionResult AllAccessPermissions(string selected)
        {
            var permissions = _permissionService.GetAllPermissionRecords();
            var selectedArr = selected.SplitSafe(",");

            var data = permissions
                .Select(x => new
                {
                    id = x.SystemName,
                    text = x.SystemName,  // TODO: localization.
                    selected = selectedArr.Contains(x.SystemName)
                })
                .ToList();

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult AccessDenied(string pageUrl)
        {
            var customer = Services.WorkContext.CurrentCustomer;

            if (customer == null || customer.IsGuest())
            {
                Logger.Info(T("Admin.System.Warnings.AccessDeniedToAnonymousRequest", pageUrl.NaIfEmpty()));
            }
            else
            {
                Logger.Info(T("Admin.System.Warnings.AccessDeniedToUser",
                    customer.Email.NaIfEmpty(), customer.Email.NaIfEmpty(), pageUrl.NaIfEmpty()));
            }

            return View();
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Acl.Read)]
        public ActionResult List()
        {
            var model = new PermissionListModel();

            return View(model);
        }

        [HttpPost, Permission(Permissions.Configuration.Acl.Update)]
        public ActionResult Edit(FormCollection form)
        {

            NotifySuccess(T("Admin.Configuration.ACL.Updated"));

            return RedirectToAction("Permissions");
        }
    }
}