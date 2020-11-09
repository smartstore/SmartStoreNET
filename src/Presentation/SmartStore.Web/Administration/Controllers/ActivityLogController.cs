using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Logging;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ActivityLogController : AdminControllerBase
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AdminAreaSettings _adminAreaSettings;

        #endregion Fields

        #region Constructors

        public ActivityLogController(
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings)
        {
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _adminAreaSettings = adminAreaSettings;
        }

        #endregion

        #region Activity log types

        [Permission(Permissions.Configuration.ActivityLog.Read)]
        public ActionResult ListTypes()
        {
            var activityLogTypeModel = _customerActivityService.GetAllActivityTypes().Select(x => x.ToModel()).OrderBy(x => x.Name);
            var gridModel = new GridModel<ActivityLogTypeModel>
            {
                Data = activityLogTypeModel,
                Total = activityLogTypeModel.Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.ActivityLog.Read)]
        public ActionResult ListTypes(GridCommand command)
        {
            var activityLogTypeModel = _customerActivityService.GetAllActivityTypes().Select(x => x.ToModel()).ToList();
            var gridModel = new GridModel<ActivityLogTypeModel>
            {
                Data = activityLogTypeModel,
                Total = activityLogTypeModel.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.ActivityLog.Update)]
        public ActionResult SaveTypes(FormCollection formCollection)
        {
            var keys = formCollection.AllKeys.Where(c => c.StartsWith("checkBox_")).Select(c => c.Substring(9));
            foreach (var key in keys)
            {
                if (Int32.TryParse(key, out int id))
                {
                    var activityType = _customerActivityService.GetActivityTypeById(id);
                    activityType.Enabled = !formCollection["checkBox_" + key].Equals("false");
                    _customerActivityService.UpdateActivityType(activityType);
                }
            }

            NotifySuccess(T("Admin.Configuration.ActivityLog.ActivityLogType.Updated"));

            return RedirectToAction("ListTypes");
        }

        #endregion

        #region Activity log

        [Permission(Permissions.Configuration.ActivityLog.Read)]
        public ActionResult ListLogs()
        {
            var model = new ActivityLogSearchModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            model.ActivityLogType = _customerActivityService.GetAllActivityTypes()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.ActivityLog.Read)]
        public JsonResult ListActivityLogs(GridCommand command, ActivityLogSearchModel model)
        {
            DateTime? startDateValue = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.CreatedOnTo == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var activityLog = _customerActivityService.GetAllActivities(startDateValue, endDateValue, null, model.ActivityLogTypeId,
                command.Page - 1, command.PageSize, model.CustomerEmail, model.CustomerSystemAccount);

            var systemAccountCustomers = _customerService.GetSystemAccountCustomers();

            var gridModel = new GridModel<ActivityLogModel>
            {
                Data = activityLog.Select(x =>
                {
                    var m = x.ToModel();
                    var systemCustomer = systemAccountCustomers.FirstOrDefault(y => y.Id == x.CustomerId);

                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.IsSystemAccount = (systemCustomer != null);

                    if (systemCustomer != null)
                    {
                        if (systemCustomer.IsSearchEngineAccount())
                            m.SystemAccountName = T("Admin.System.SystemCustomerNames.SearchEngine");
                        else if (systemCustomer.IsBackgroundTaskAccount())
                            m.SystemAccountName = T("Admin.System.SystemCustomerNames.BackgroundTask");
                        else if (systemCustomer.IsPdfConverter())
                            m.SystemAccountName = T("Admin.System.SystemCustomerNames.PdfConverter");
                        else
                            m.SystemAccountName = "";
                    }

                    return m;
                }),
                Total = activityLog.TotalCount
            };
            return new JsonResult { Data = gridModel };
        }

        [HttpPost, ActionName("ListLogs")]
        [FormValueRequired("clearall")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.ActivityLog.Delete)]
        public ActionResult ClearAll()
        {
            _customerActivityService.ClearAllActivities();

            return RedirectToAction("ListLogs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.ActivityLog.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                var activityLogs = _customerActivityService.GetActivityByIds(selectedIds.ToArray());

                foreach (var activityLog in activityLogs)
                {
                    _customerActivityService.DeleteActivity(activityLog);
                }
            }

            return Json(new { success = true });
        }

        #endregion
    }
}
