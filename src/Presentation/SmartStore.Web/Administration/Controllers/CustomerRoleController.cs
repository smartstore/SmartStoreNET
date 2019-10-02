using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
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

        public CustomerRoleController(
            ICustomerService customerService,
            ICustomerActivityService customerActivityService)
        {
            _customerService = customerService;
            _customerActivityService = customerActivityService;
        }

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

        // Ajax.
        public ActionResult AllCustomerRoles(string label, string selectedIds)
        {
            var customerRoles = _customerService.GetAllCustomerRoles(true);
            var ids = selectedIds.ToIntArray();

            if (label.HasValue())
            {
                customerRoles.Insert(0, new CustomerRole { Name = label, Id = 0 });
            }

            var list =
                from c in customerRoles
                select new
                {
                    id = c.Id.ToString(),
                    text = c.Name,
                    selected = ids.Contains(c.Id)
                };

            return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #region List / Create / Edit / Delete

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Customer.Role.Read)]
        public ActionResult List()
        {
            var customerRoles = _customerService.GetAllCustomerRoles(true);
            var gridModel = new GridModel<CustomerRoleModel>
            {
                Data = customerRoles.Select(x => x.ToModel()),
                Total = customerRoles.Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Role.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<CustomerRoleModel>();

            var customerRoles = _customerService.GetAllCustomerRoles(true);

            model.Data = customerRoles.Select(x => x.ToModel());
            model.Total = customerRoles.Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Customer.Role.Create)]
        public ActionResult Create()
        {
            var model = new CustomerRoleModel
            {
                Active = true
            };

            model.TaxDisplayTypes = GetTaxDisplayTypesList(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Customer.Role.Create)]
        public ActionResult Create(CustomerRoleModel model, bool continueEditing)
        {
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

        [Permission(Permissions.Customer.Role.Read)]
        public ActionResult Edit(int id)
        {
            var customerRole = _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
            {
                return RedirectToAction("List");
            }

            var model = customerRole.ToModel();
            model.TaxDisplayTypes = GetTaxDisplayTypesList(model);
            model.PermissionTree = Services.Permissions.GetPermissionTree(customerRole, true);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Customer.Role.Update)]
        public ActionResult Edit(CustomerRoleModel model, bool continueEditing, FormCollection form)
        {
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
                    {
                        throw new SmartException(T("Admin.Customers.CustomerRoles.Fields.Active.CantEditSystem"));
                    }

                    if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(model.SystemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new SmartException(T("Admin.Customers.CustomerRoles.Fields.SystemName.CantEditSystem"));
                    }

                    customerRole = model.ToEntity(customerRole);
                    _customerService.UpdateCustomerRole(customerRole);

                    // Update permissions.
                    var permissionKey = "permission-";
                    var existingMappings = customerRole.PermissionRoleMappings.ToDictionarySafe(x => x.PermissionRecordId, x => x);

                    var mappings = form.AllKeys.Where(x => x.StartsWith(permissionKey))
                        .Select(x =>
                        {
                            var id = x.Substring(permissionKey.Length).ToInt();
                            bool? allow = null;
                            var value = form[x].EmptyNull();
                            if (value.StartsWith("2"))
                            {
                                allow = true;
                            }
                            else if (value.StartsWith("1"))
                            {
                                allow = false;
                            }

                            return new { id, allow };
                        })
                        .ToDictionary(x => x.id, x => x.allow);

                    using (var scope = new DbContextScope(ctx: Services.DbContext, validateOnSave: false, autoDetectChanges: false, autoCommit: false))
                    {
                        foreach (var item in mappings)
                        {
                            if (existingMappings.TryGetValue(item.Key, out var mapping))
                            {
                                if (item.Value.HasValue)
                                {
                                    mapping.Allow = item.Value.Value;

                                    Services.Permissions.UpdatePermissionRoleMapping(mapping);
                                }
                                else
                                {
                                    Services.Permissions.DeletePermissionRoleMapping(mapping);
                                }
                            }
                            else if (item.Value.HasValue)
                            {
                                Services.Permissions.InsertPermissionRoleMapping(new PermissionRoleMapping
                                {
                                    Allow = item.Value.Value,
                                    PermissionRecordId = item.Key,
                                    CustomerRoleId = customerRole.Id
                                });
                            }
                        }

                        scope.Commit();
                    }

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
        [Permission(Permissions.Customer.Role.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
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
