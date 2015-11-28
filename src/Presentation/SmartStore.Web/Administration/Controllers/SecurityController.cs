using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Security;
using SmartStore.Core;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class SecurityController : AdminControllerBase
	{
		#region Fields

        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

        public SecurityController(IWorkContext workContext,
            IPermissionService permissionService,
            ICustomerService customerService, ILocalizationService localizationService)
		{
            this._workContext = workContext;
            this._permissionService = permissionService;
            this._customerService = customerService;
            this._localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public ActionResult AccessDenied(string pageUrl)
        {
            var currentCustomer = _workContext.CurrentCustomer;
            if (currentCustomer == null || currentCustomer.IsGuest())
            {
				Logger.Information(string.Format("Access denied to anonymous request on {0}", pageUrl));
                return View();
            }

			Logger.Information(string.Format("Access denied to user #{0} '{1}' on {2}", currentCustomer.Email, currentCustomer.Email, pageUrl));


            return View();
        }

        public ActionResult Permissions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var model = new PermissionMappingModel();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var customerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var pr in permissionRecords)
            {
                model.AvailablePermissions.Add(new PermissionRecordModel()
                {
                    Name = pr.Name,
                    SystemName = pr.SystemName
                });
            }
            foreach (var cr in customerRoles)
            {
                model.AvailableCustomerRoles.Add(new CustomerRoleModel()
                {
                    Id = cr.Id,
                    Name = cr.Name
                });
            }
            foreach (var pr in permissionRecords)
                foreach (var cr in customerRoles)
                {
                    bool allowed = pr.CustomerRoles.Where(x => x.Id == cr.Id).ToList().Count() > 0;
                    if (!model.Allowed.ContainsKey(pr.SystemName))
                        model.Allowed[pr.SystemName] = new Dictionary<int, bool>();
                    model.Allowed[pr.SystemName][cr.Id] = allowed;
                }

            return View(model);
        }

        [HttpPost, ActionName("Permissions")]
        public ActionResult PermissionsSave(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var customerRoles = _customerService.GetAllCustomerRoles(true);


            foreach (var cr in customerRoles)
            {
                string formKey = "allow_" + cr.Id;
                var permissionRecordSystemNamesToRestrict = form[formKey] != null ? form[formKey].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();

                foreach (var pr in permissionRecords)
                {

                    bool allow = permissionRecordSystemNamesToRestrict.Contains(pr.SystemName);
                    if (allow)
                    {
                        if (pr.CustomerRoles.Where(x => x.Id == cr.Id).FirstOrDefault() == null)
                        {
                            pr.CustomerRoles.Add(cr);
                            _permissionService.UpdatePermissionRecord(pr);
                        }
                    }
                    else
                    {
                        if (pr.CustomerRoles.Where(x => x.Id == cr.Id).FirstOrDefault() != null)
                        {
                            pr.CustomerRoles.Remove(cr);
                            _permissionService.UpdatePermissionRecord(pr);
                        }
                    }
                }
            }

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.ACL.Updated"));
            return RedirectToAction("Permissions");
        }
        #endregion
    }
}
