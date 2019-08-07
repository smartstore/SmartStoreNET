using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomerRoleController : AdminControllerBase
	{
		private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IPermissionService2 _permissionService2;

        public CustomerRoleController(
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IPermissionService2 permissionService2)
		{
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _permissionService = permissionService;
            _permissionService2 = permissionService2;
		}

        #region Utilities

        [NonAction]
        protected List<SelectListItem> GetTaxDisplayTypesList(CustomerRoleModel model)
        {
            var list = new List<SelectListItem>();

            if (model.TaxDisplayType.HasValue)
            {
                list.Insert(0, new SelectListItem
                {
                    Text = T("Enums.Smartstore.Core.Domain.Tax.TaxDisplayType.IncludingTax"),
                    Value = "0",
                    Selected = (TaxDisplayType)model.TaxDisplayType.Value == TaxDisplayType.IncludingTax
                });
                list.Insert(1, new SelectListItem
                {
                    Text = T("Enums.Smartstore.Core.Domain.Tax.TaxDisplayType.ExcludingTax"),
                    Value = "10",
                    Selected = (TaxDisplayType)model.TaxDisplayType.Value == TaxDisplayType.ExcludingTax
                });
            }
            else
            {
                list.Insert(0, new SelectListItem { Text = T("Enums.Smartstore.Core.Domain.Tax.TaxDisplayType.IncludingTax"), Value = "0" });
                list.Insert(1, new SelectListItem { Text = T("Enums.Smartstore.Core.Domain.Tax.TaxDisplayType.ExcludingTax"), Value = "10" });
            }

            return list;
        }

        #endregion

        #region Customer roles

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                return AccessDeniedView();
            
			var customerRoles = _customerService.GetAllCustomerRoles(true);
			var gridModel = new GridModel<CustomerRoleModel>
			{
                Data = customerRoles.Select(x => x.ToModel()),
                Total = customerRoles.Count()
			};
			return View(gridModel);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
        {
			var model = new GridModel<CustomerRoleModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
			{
				var customerRoles = _customerService.GetAllCustomerRoles(true);

				model.Data = customerRoles.Select(x => x.ToModel());
				model.Total = customerRoles.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<CustomerRoleModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = model
			};
		}

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                return AccessDeniedView();

            var model = new CustomerRoleModel
            {
                Active = true
            };

            model.TaxDisplayTypes = GetTaxDisplayTypesList(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CustomerRoleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                return AccessDeniedView();
            
            if (ModelState.IsValid)
            {
                var customerRole = model.ToEntity();
                _customerService.InsertCustomerRole(customerRole);

                _customerActivityService.InsertActivity("AddNewCustomerRole", T("ActivityLog.AddNewCustomerRole"), customerRole.Name);

                NotifySuccess(T("Admin.Customers.CustomerRoles.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                return AccessDeniedView();
            
            var customerRole = _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
            {
                return RedirectToAction("List");
            }

            var model = customerRole.ToModel();
            model.TaxDisplayTypes = GetTaxDisplayTypesList(model);
            model.PermissionTree = _permissionService2.GetPermissionTree(customerRole);

            return View(model);
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CustomerRoleModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                return AccessDeniedView();
            
            var customerRole = _customerService.GetCustomerRoleById(model.Id);
            if (customerRole == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (customerRole.IsSystemRole && !model.Active)
                        throw new SmartException(T("Admin.Customers.CustomerRoles.Fields.Active.CantEditSystem"));

                    if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(model.SystemName, StringComparison.InvariantCultureIgnoreCase))
                        throw new SmartException(T("Admin.Customers.CustomerRoles.Fields.SystemName.CantEditSystem"));

                    customerRole = model.ToEntity(customerRole);
                    _customerService.UpdateCustomerRole(customerRole);

                    // TODO: update permissions.

                    _customerActivityService.InsertActivity("EditCustomerRole", T("ActivityLog.EditCustomerRole"), customerRole.Name);

                    NotifySuccess(T("Admin.Customers.CustomerRoles.Updated"));
                    return continueEditing ? RedirectToAction("Edit", customerRole.Id) : RedirectToAction("List");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                return AccessDeniedView();
            
            var customerRole = _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                _customerService.DeleteCustomerRole(customerRole);

                _customerActivityService.InsertActivity("DeleteCustomerRole", T("ActivityLog.DeleteCustomerRole"), customerRole.Name);

                NotifySuccess(T("Admin.Customers.CustomerRoles.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
				NotifyError(ex.Message);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }

		}

        #endregion
    }
}
