using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Data.Utilities;
using SmartStore.Rules;
using SmartStore.Services.Customers;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomerRoleController : AdminControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IRuleStorage _ruleStorage;
        private readonly Lazy<ITaskScheduler> _taskScheduler;
        private readonly Lazy<IScheduleTaskService> _scheduleTaskService;
        private readonly CustomerSettings _customerSettings;
        private readonly AdminAreaSettings _adminAreaSettings;

        public CustomerRoleController(
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IRuleStorage ruleStorage,
            Lazy<ITaskScheduler> taskScheduler,
            Lazy<IScheduleTaskService> scheduleTaskService,
            CustomerSettings customerSettings,
            AdminAreaSettings adminAreaSettings)
        {
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _ruleStorage = ruleStorage;
            _taskScheduler = taskScheduler;
            _scheduleTaskService = scheduleTaskService;
            _customerSettings = customerSettings;
            _adminAreaSettings = adminAreaSettings;
        }

        // AJAX.
        public ActionResult AllCustomerRoles(string label, string selectedIds, bool? includeSystemRoles)
        {
            var rolesQuery = _customerService.GetAllCustomerRoles(true).SourceQuery;

            if (!(includeSystemRoles ?? true))
            {
                rolesQuery = rolesQuery.Where(x => !x.IsSystemRole);
            }

            var rolesPager = new FastPager<CustomerRole>(rolesQuery, 500);
            var customerRoles = new List<CustomerRole>();
            var ids = selectedIds.ToIntArray();

            while (rolesPager.ReadNextPage(out var roles))
            {
                customerRoles.AddRange(roles);
            }

            var list = customerRoles
                .OrderBy(x => x.Name)
                .Select(x => new ChoiceListItem
                {
                    Id = x.Id.ToString(),
                    Text = x.Name,
                    Selected = ids.Contains(x.Id)
                })
                .ToList();

            if (label.HasValue())
            {
                list.Insert(0, new ChoiceListItem
                {
                    Id = "0",
                    Text = label,
                    Selected = false
                });
            }

            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #region List / Create / Edit / Delete

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Customer.Role.Read)]
        public ActionResult List()
        {
            ViewData["GridPageSize"] = _adminAreaSettings.GridPageSize;

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Role.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<CustomerRoleModel>();
            var customerRoles = _customerService.GetAllCustomerRoles(true, command.Page - 1, command.PageSize);

            model.Data = customerRoles.Select(x => x.ToModel());
            model.Total = customerRoles.TotalCount;

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

            PrepareModel(model, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Customer.Role.Create)]
        public ActionResult Create(CustomerRoleModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerRole = model.ToEntity();
                _customerService.InsertCustomerRole(customerRole);

                if (model.SelectedRuleSetIds?.Any() ?? false)
                {
                    _ruleStorage.ApplyRuleSetMappings(customerRole, model.SelectedRuleSetIds);

                    _customerService.UpdateCustomerRole(customerRole);
                }

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
            PrepareModel(model, customerRole);

            model.PermissionTree = Services.Permissions.GetPermissionTree(customerRole, true);
            model.PrimaryStoreCurrencyCode = Services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
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

                    // Add\remove assigned rule sets.
                    _ruleStorage.ApplyRuleSetMappings(customerRole, model.SelectedRuleSetIds);

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
                    return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
                }

                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Role.Read)]
        public ActionResult CustomerRoleMappings(GridCommand command, int id)
        {
            var model = new GridModel<CustomerRoleMappingModel>();
            var mappings = _customerService.GetCustomerRoleMappings(null, new int[] { id }, null, command.Page - 1, command.PageSize);
            var role = _customerService.GetCustomerRoleById(id);
            var isGuestRole = role.SystemName.IsCaseInsensitiveEqual(SystemCustomerRoleNames.Guests);
            var emailFallbackStr = isGuestRole ? T("Admin.Customers.Guest").Text : string.Empty;

            model.Data = mappings.Select(x =>
            {
                var mappingModel = new CustomerRoleMappingModel
                {
                    Id = x.Id,
                    Active = x.Customer.Active,
                    CustomerId = x.CustomerId,
                    Email = x.Customer.Email.NullEmpty() ?? emailFallbackStr,
                    Username = x.Customer.Username,
                    FullName = x.Customer.GetFullName(),
                    CreatedOn = Services.DateTimeHelper.ConvertToUserTime(x.Customer.CreatedOnUtc, DateTimeKind.Utc),
                    LastActivityDate = Services.DateTimeHelper.ConvertToUserTime(x.Customer.LastActivityDateUtc, DateTimeKind.Utc),
                    IsSystemMapping = x.IsSystemMapping
                };

                return mappingModel;
            })
            .ToList();

            model.Total = mappings.TotalCount;

            return new JsonResult { Data = model };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyRules(int id)
        {
            var customerRole = _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
            {
                return RedirectToAction("List");
            }

            var task = _scheduleTaskService.Value.GetTaskByType<TargetGroupEvaluatorTask>();
            if (task != null)
            {
                _taskScheduler.Value.RunSingleTask(task.Id, new Dictionary<string, string>
                {
                    { "CustomerRoleIds", customerRole.Id.ToString() }
                });

                NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress"));
            }
            else
            {
                NotifyError(T("Admin.System.ScheduleTasks.TaskNotFound", nameof(TargetGroupEvaluatorTask)));
            }

            return RedirectToAction("Edit", new { id = customerRole.Id });
        }

        #endregion

        private void PrepareModel(CustomerRoleModel model, CustomerRole role)
        {
            if (role != null)
            {
                model.SelectedRuleSetIds = role.RuleSets.Select(x => x.Id).ToArray();

                model.ShowRuleApplyButton = model.SelectedRuleSetIds.Any();
                if (!model.ShowRuleApplyButton)
                {
                    var customerRoleMappingQuery = _customerService.GetCustomerRoleMappings(null, new[] { role.Id }, true, 0, int.MaxValue, false).SourceQuery;
                    model.ShowRuleApplyButton = customerRoleMappingQuery.Any();
                }
            }

            model.TaxDisplayTypes = model.TaxDisplayType.HasValue
                ? ((TaxDisplayType)model.TaxDisplayType.Value).ToSelectList().ToList()
                : TaxDisplayType.IncludingTax.ToSelectList(false).ToList();

            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;
        }
    }
}
