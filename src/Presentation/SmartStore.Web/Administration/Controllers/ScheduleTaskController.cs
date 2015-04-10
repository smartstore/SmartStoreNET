using System;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Reflection;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using SmartStore.Core.Async;
using Autofac;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ScheduleTaskController : AdminControllerBase
    {
        #region Fields

		private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors

		public ScheduleTaskController(IScheduleTaskService scheduleTaskService, IPermissionService permissionService, IDateTimeHelper dateTimeHelper)
        {
            this._scheduleTaskService = scheduleTaskService;
            this._permissionService = permissionService;
            this._dateTimeHelper = dateTimeHelper;
			T = NullLocalizer.Instance;
        }

        #endregion

		public Localizer T { get; set; }

        #region Utility

		private bool IsTaskInstalled(ScheduleTask task)
		{
			var type = Type.GetType(task.Type);
			if (type != null)
			{
				return PluginManager.IsActivePluginAssembly(type.Assembly);
			}
			return false;
		}

        [NonAction]
        protected ScheduleTaskModel PrepareScheduleTaskModel(ScheduleTask task)
        {
            var model = new ScheduleTaskModel()
            {
                Id = task.Id,
                Name = task.Name,
                Seconds = task.Seconds,
                Enabled = task.Enabled,
                StopOnError = task.StopOnError,
                LastStartUtc = task.LastStartUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastStartUtc.Value, DateTimeKind.Utc).ToString("G") : "",
                LastEndUtc = task.LastEndUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastEndUtc.Value, DateTimeKind.Utc).ToString("G") : "",
                LastSuccessUtc = task.LastSuccessUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastSuccessUtc.Value, DateTimeKind.Utc).ToString("G") : "",
				LastError = task.LastError.EmptyNull(),
				IsRunning =	task.IsRunning,		//task.LastStartUtc.GetValueOrDefault() > task.LastEndUtc.GetValueOrDefault(),
				Duration = ""
            };

			var span = TimeSpan.Zero;
			if (task.LastStartUtc.HasValue)
			{
				span = model.IsRunning ? DateTime.UtcNow - task.LastStartUtc.Value : task.LastEndUtc.Value - task.LastStartUtc.Value;
				model.Duration = span.ToString("g");
			}

            return model;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();
			
            var models = _scheduleTaskService.GetAllTasks(true)
				.Where(IsTaskInstalled)
                .Select(PrepareScheduleTaskModel)
                .ToList();
            var model = new GridModel<ScheduleTaskModel>
            {
                Data = models,
                Total = models.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TaskUpdate(ScheduleTaskModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var scheduleTask = _scheduleTaskService.GetTaskById(model.Id);
            if (scheduleTask == null)
                return Content("Schedule task cannot be loaded");

            scheduleTask.Name = model.Name;
            scheduleTask.Seconds = model.Seconds;
            scheduleTask.Enabled = model.Enabled;
            scheduleTask.StopOnError = model.StopOnError;

			int max = Int32.MaxValue / 1000;

			scheduleTask.Seconds = (model.Seconds > max ? max : model.Seconds);

            _scheduleTaskService.UpdateTask(scheduleTask);

            return List(command);
        }

		public ActionResult RunJob(int id, string returnUrl = "")
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			returnUrl = returnUrl.NullEmpty() ?? Request.UrlReferrer.ToString();

			var t = AsyncRunner.Run((c, ct) =>
			{
				var svc = c.Resolve<IScheduleTaskService>();

				try
				{
					var scheduleTask = svc.GetTaskById(id);
					if (scheduleTask == null)
						throw new Exception("Schedule task cannot be loaded");

					var job = new Job(scheduleTask);
					job.Enabled = true;
					job.Execute(ct, c, false);
				}
				catch
				{
					try
					{
						svc.EnsureTaskIsNotRunning(id);
					}
					catch { }
				}
			});

			// wait only 100 ms.
			t.Wait(100);

			if (t.IsCompleted)
			{
				if (!t.IsFaulted)
				{
					NotifySuccess(T("Admin.System.ScheduleTasks.RunNow.Completed"));
				}
				else
				{
					NotifyError(t.Exception.Flatten().InnerException);
				}
			}
			else
			{
				NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress"));
			}

			return Redirect(returnUrl);
		}

        #endregion
    }
}
