using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Logging;
using SmartStore.Core;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class LogController : AdminControllerBase
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILogService _logService;

        private static readonly Dictionary<LogLevel, string> s_logLevelHintMap = new Dictionary<LogLevel, string>
        {
            { LogLevel.Fatal, "dark" },
            { LogLevel.Error, "danger" },
            { LogLevel.Warning, "warning" },
            { LogLevel.Information, "info" },
            { LogLevel.Debug, "secondary" }
        };

        #endregion

        #region Constructors

        public LogController(
            IWorkContext workContext,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ILogService logService)
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _logService = logService;
        }

        #endregion

        #region Log

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.System.Log.Read)]
        public ActionResult List()
        {
            var model = new LogListModel();

            model.AvailableLogLevels = LogLevel.Debug.ToSelectList(false).ToList();

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.System.Log.Read)]
        public ActionResult LogList(GridCommand command, LogListModel model)
        {
            var gridModel = new GridModel<LogModel>();

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

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("clearall")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.Log.Delete)]
        public ActionResult ClearAll()
        {
            _logService.ClearLog();
            NotifySuccess(_localizationService.GetResource("Admin.System.Log.Cleared"));
            return RedirectToAction("List");
        }

        [Permission(Permissions.System.Log.Read)]
        public ActionResult View(int id)
        {
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

            var name = string.Empty;
            var tokens = loggerName.Split('.');
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                name += i == tokens.Length - 1 ? token : token.Substring(0, 1) + "...";
            }

            return name;
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.Log.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var log = _logService.GetLogById(id);
            if (log == null)
                // No log entry found with the specified id.
                return RedirectToAction("List");

            _logService.DeleteLog(log);

            NotifySuccess(_localizationService.GetResource("Admin.System.Log.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.Log.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                var logItems = _logService.GetLogByIds(selectedIds.ToArray());
                foreach (var logItem in logItems)
                    _logService.DeleteLog(logItem);
            }

            return Json(new { Result = true });
        }
        #endregion

    }
}
