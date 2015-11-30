using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Logging;
using SmartStore.Core;
using SmartStore.Core.Domain.Logging;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class LogController : AdminControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;

        private static readonly Dictionary<LogLevel, string> s_logLevelHintMap = new Dictionary<LogLevel, string> 
        { 
            { LogLevel.Fatal, "inverse" },
            { LogLevel.Error, "important" },
            { LogLevel.Warning, "warning" },
            { LogLevel.Information, "info" },
            { LogLevel.Debug, "default" }
        };

        public LogController(IWorkContext workContext,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService)
        {
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._permissionService = permissionService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var model = new LogListModel();
            model.AvailableLogLevels = LogLevel.Debug.ToSelectList(false).ToList();
            model.AvailableLogLevels.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult LogList(GridCommand command, LogListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            LogLevel? logLevel = model.LogLevelId > 0 ? (LogLevel?)(model.LogLevelId) : null;


			var logItems = Logger.GetAllLogs(createdOnFromValue, createdToFromValue, model.Message,
                logLevel, command.Page - 1, command.PageSize, model.MinFrequency);

            var gridModel = new GridModel<LogModel>
            {
                Data = logItems.Select(x =>
                {
                    var logModel = new LogModel()
                    {
                        Id = x.Id,
                        LogLevelHint = s_logLevelHintMap[x.LogLevel],
                        LogLevel = x.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                        ShortMessage = x.ShortMessage,
                        FullMessage = x.FullMessage,
                        IpAddress = x.IpAddress,
                        CustomerId = x.CustomerId,
                        CustomerEmail = x.Customer != null ? x.Customer.Email : null,
                        PageUrl = x.PageUrl,
                        ReferrerUrl = x.ReferrerUrl,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
						Frequency = x.Frequency,
						ContentHash = x.ContentHash
                    };

					if (x.UpdatedOnUtc.HasValue)
						logModel.UpdatedOn = _dateTimeHelper.ConvertToUserTime(x.UpdatedOnUtc.Value, DateTimeKind.Utc);

					return logModel;
                }),
                Total = logItems.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        [HttpPost, ActionName("List")]
        [FormValueRequired("clearall")]
        public ActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

			Logger.ClearLog();

            NotifySuccess(_localizationService.GetResource("Admin.System.Log.Cleared"));
            return RedirectToAction("List");
        }

        public ActionResult View(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

			var log = Logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

            var model = new LogModel()
            {
                Id = log.Id,
                LogLevelHint = s_logLevelHintMap[log.LogLevel],
                LogLevel = log.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                ShortMessage = log.ShortMessage,
                FullMessage = log.FullMessage,
                IpAddress = log.IpAddress,
                CustomerId = log.CustomerId,
                CustomerEmail = log.Customer != null ? log.Customer.Email : null,
                PageUrl = log.PageUrl,
                ReferrerUrl = log.ReferrerUrl,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc),
				Frequency = log.Frequency,
				ContentHash = log.ContentHash
            };

			if (log.UpdatedOnUtc.HasValue)
				model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(log.UpdatedOnUtc.Value, DateTimeKind.Utc);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

			var log = Logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

			Logger.DeleteLog(log);


            NotifySuccess(_localizationService.GetResource("Admin.System.Log.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            if (selectedIds != null)
            {
				var logItems = Logger.GetLogByIds(selectedIds.ToArray());
                foreach (var logItem in logItems)
					Logger.DeleteLog(logItem);
            }

            return Json(new { Result = true});
        }

    }
}
