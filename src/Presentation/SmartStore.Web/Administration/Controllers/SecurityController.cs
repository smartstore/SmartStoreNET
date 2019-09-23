using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Customers;
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
    public class SecurityController : AdminControllerBase
	{
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;

        public SecurityController(
            IWorkContext workContext,
            IPermissionService permissionService,
            ICustomerService customerService)
		{
            _workContext = workContext;
            _permissionService = permissionService;
            _customerService = customerService;
		}

        // Ajax.
        public ActionResult AllAccessPermissions(string selected)
        {
            var systemNames = Services.Permissions2.GetAllSystemNames();
            var selectedArr = selected.SplitSafe(",");

            var data = systemNames
                .Select(x => new
                {
                    id = x.Key,
                    text = x.Value,
                    selected = selectedArr.Contains(x.Key)
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

        //GP: remove (old permission list).
        public ActionResult Permissions()
        {
            var model = new PermissionMappingModel();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var customerRoles = _customerService.GetAllCustomerRoles(true);

            foreach (var pr in permissionRecords)
            {
                model.AvailablePermissions.Add(new PermissionRecordModel
                {
                    Name = pr.Name,
                    SystemName = pr.SystemName,
					Category = pr.Category
                });
            }

            foreach (var cr in customerRoles)
            {
                model.AvailableCustomerRoles.Add(new CustomerRoleModel
                {
                    Id = cr.Id,
                    Name = cr.Name
                });
            }

			foreach (var pr in permissionRecords)
			{
				foreach (var cr in customerRoles)
				{
					var allowed = pr.CustomerRoles.Any(x => x.Id == cr.Id);

					if (!model.Allowed.ContainsKey(pr.SystemName))
						model.Allowed[pr.SystemName] = new Dictionary<int, bool>();

					model.Allowed[pr.SystemName][cr.Id] = allowed;
				}
			}

            return View(model);
        }

        //GP: remove (old permission list).
        [HttpPost, ActionName("Permissions")]
        public ActionResult PermissionsSave(FormCollection form)
        {
            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var customerRoles = _customerService.GetAllCustomerRoles(true);

            foreach (var cr in customerRoles)
            {
				var restrictedSystemNames = form["allow_" + cr.Id.ToString()].SplitSafe(",").ToList();

				foreach (var permission in permissionRecords)
                {
                    bool allow = restrictedSystemNames.Contains(permission.SystemName);

                    if (allow)
                    {
                        if (!permission.CustomerRoles.Any(x => x.Id == cr.Id))
                        {
                            permission.CustomerRoles.Add(cr);
                            _permissionService.UpdatePermissionRecord(permission);
                        }
                    }
                    else
                    {
                        if (permission.CustomerRoles.Any(x => x.Id == cr.Id))
                        {
                            permission.CustomerRoles.Remove(cr);
                            _permissionService.UpdatePermissionRecord(permission);
                        }
                    }
                }
            }

            NotifySuccess(T("Admin.Configuration.ACL.Updated"));

            return RedirectToAction("Permissions");
        }
    }
}
