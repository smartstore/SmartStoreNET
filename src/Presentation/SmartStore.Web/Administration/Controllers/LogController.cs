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
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Services.Logging;
using System.Text;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class LogController : AdminControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;
		private readonly ILogService _logService;

		private static readonly Dictionary<LogLevel, string> s_logLevelHintMap = new Dictionary<LogLevel, string> 
        { 
            { LogLevel.Fatal, "dark" },
            { LogLevel.Error, "danger" },
            { LogLevel.Warning, "warning" },
            { LogLevel.Information, "info" },
            { LogLevel.Debug, "default" }
        };

        public LogController(
			IWorkContext workContext,
            ILocalizationService localizationService, 
			IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService,
			ILogService logService)
        {
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._permissionService = permissionService;
			this._logService = logService;
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

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult LogList(GridCommand command, LogListModel model)
        {
			var gridModel = new GridModel<LogModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
			{
				DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
					: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

				DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
					: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

				LogLevel? logLevel = model.LogLevelId > 0 ? (LogLevel?)(model.LogLevelId) : null;

				var logItems = _logService.GetAllLogs(
					createdOnFromValue, 
					createdToFromValue,
					model.Logger, 
					model.Message,
					logLevel, 
					command.Page - 1, 
					command.PageSize);

				gridModel.Data = logItems.Select(x =>
				{
					var logModel = new LogModel
					{
						Id = x.Id,
						LogLevelHint = s_logLevelHintMap[x.LogLevel],
						LogLevel = x.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
						ShortMessage = x.ShortMessage,
						FullMessage = x.FullMessage,
						IpAddress = x.IpAddress,
						CustomerId = x.CustomerId,
						CustomerEmail = x.Customer?.Email,
						PageUrl = x.PageUrl,
						ReferrerUrl = x.ReferrerUrl,
						CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
						Logger = x.Logger,
						LoggerShort = TruncateLoggerName(x.Logger),
						HttpMethod = x.HttpMethod,
						UserName = x.UserName
					};

					return logModel;
				});

				gridModel.Total = logItems.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<LogModel>();

				NotifyAccessDenied();
			}

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

			_logService.ClearLog();

            NotifySuccess(_localizationService.GetResource("Admin.System.Log.Cleared"));
            return RedirectToAction("List");
        }

        public ActionResult View(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

			var log = _logService.GetLogById(id);
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
				CustomerEmail = log.Customer?.Email,
				PageUrl = log.PageUrl,
				ReferrerUrl = log.ReferrerUrl,
				CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc),
				Logger = log.Logger,
				LoggerShort = TruncateLoggerName(log.Logger),
				HttpMethod = log.HttpMethod,
				UserName = log.UserName
			};

            return View(model);
        }

		private string TruncateLoggerName(string loggerName)
		{
			if (loggerName.IndexOf('.') < 0)
				return loggerName;

			var sb = new StringBuilder();
			var tokens = loggerName.Split('.');
			for (int i = 0; i < tokens.Length; i++)
			{
				var token = tokens[i];
				sb.Append(i == tokens.Length - 1 ? token : token.Substring(0, 1) + "...");
			}

			return sb.ToString();
		}

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

			var log = _logService.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

			_logService.DeleteLog(log);

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
				var logItems = _logService.GetLogByIds(selectedIds.ToArray());
                foreach (var logItem in logItems)
					_logService.DeleteLog(logItem);
            }

            return Json(new { Result = true});
        }

    }
}
