using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Extensions;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Services;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class ImportController : AdminControllerBase
	{
		private readonly ICommonServices _services;
		private readonly IImportProfileService _importService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly ITaskScheduler _taskScheduler;

		public ImportController(
			ICommonServices services,
			IImportProfileService importService,
			IDateTimeHelper dateTimeHelper,
			ITaskScheduler taskScheduler)
		{
			_services = services;
			_importService = importService;
			_dateTimeHelper = dateTimeHelper;
			_taskScheduler = taskScheduler;
		}

		#region Utilities

		private void PrepareProfileModel(ImportProfileModel model, ImportProfile profile, bool forEdit)
		{
			if (profile != null)
			{
				model.Id = profile.Id;
				model.Name = profile.Name;
				model.FileType = profile.FileType;
				model.EntityType = profile.EntityType;
				model.Enabled = profile.Enabled;
				model.Skip = profile.Skip;
				model.Take = profile.Take;
				model.ScheduleTaskId = profile.SchedulingTaskId;
				model.ScheduleTaskName = profile.ScheduleTask.Name.NaIfEmpty();
				model.IsTaskRunning = profile.ScheduleTask.IsRunning;
				model.IsTaskEnabled = profile.ScheduleTask.Enabled;
				model.LogFileExists = System.IO.File.Exists(profile.GetImportLogPath());
				model.EntityTypeName = profile.EntityType.GetLocalizedEnum(_services.Localization, _services.WorkContext);

				model.ExistingFileNames = profile.GetImportFiles()
					.Select(x => Path.GetFileName(x))
					.ToList();

				if (profile.FileType == ImportFileType.CSV)
				{
					var converter = new CsvConfigurationConverter();
					var config = converter.ConvertFrom<CsvConfiguration>(profile.FileTypeConfiguration);

					model.CsvConfiguration = new CsvConfigurationModel(config ?? CsvConfiguration.ExcelFriendlyConfiguration);
				}
			}
			else
			{
				model.Name = model.EntityType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
				model.ExistingFileNames = new List<string>();
			}

			if (forEdit)
			{
				model.AvailableEntityTypes = ImportEntityType.Product.ToSelectList(false).ToList();
				model.AvailableFileTypes = ImportFileType.CSV.ToSelectList(false).ToList();
			}
		}

		#endregion

		public ActionResult Index()
		{
			return RedirectToAction("List");
		}

		public ActionResult List()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profiles = _importService.GetImportProfiles().ToList();
			var model = new List<ImportProfileModel>();

			foreach (var profile in profiles)
			{
				var profileModel = new ImportProfileModel();

				PrepareProfileModel(profileModel, profile, false);

				profileModel.TaskModel = profile.ScheduleTask.ToScheduleTaskModel(_services.Localization, _dateTimeHelper, Url);

				model.Add(profileModel);
			}

			return View(model);
		}

		public ActionResult Create()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var model = new ImportProfileModel();
			PrepareProfileModel(model, null, true);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing"), FormValueRequired("save", "save-continue")]
		public ActionResult Create(ImportProfileModel model)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var importFile = Path.Combine(FileSystemHelper.TempDir(), model.TempFileName.EmptyNull());

			if (!System.IO.File.Exists(importFile))
			{
				ModelState.AddModelError("", T("Admin.DataExchange.Import.MissingImportFile"));
			}
			else if (ModelState.IsValid)
			{
				var profile = _importService.InsertImportProfile(model.TempFileName, model.Name, model.EntityType);

				if (profile != null && profile.Id != 0)
				{
					var importFileDestination = Path.Combine(profile.GetImportFolder(true, true), model.TempFileName);

					FileSystemHelper.Copy(importFile, importFileDestination, true, true);

					return RedirectToAction("Edit", new { id = profile.Id });
				}
			}

			PrepareProfileModel(model, null, true);
			return View(model);
		}

		public ActionResult Edit(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var model = new ImportProfileModel();
			PrepareProfileModel(model, profile, true);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult Edit(ImportProfileModel model, bool continueEditing, FormCollection form)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importService.GetImportProfileById(model.Id);
			if (profile == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				profile.Name = model.Name;
				profile.EntityType = model.EntityType;
				profile.Enabled = model.Enabled;
				profile.Skip = model.Skip;
				profile.Take = model.Take;

				if (profile.FileType == ImportFileType.CSV && model.CsvConfiguration != null)
				{
					CsvConfiguration config = model.CsvConfiguration.Clone();

					var converter = new CsvConfigurationConverter();
					profile.FileTypeConfiguration = converter.ConvertTo(config);
				}

				_importService.UpdateImportProfile(profile);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

				return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
			}

			PrepareProfileModel(model, profile, true);

			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			try
			{
				_importService.DeleteImportProfile(profile);

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

				return RedirectToAction("List");
			}
			catch (Exception exception)
			{
				NotifyError(exception);
			}

			return RedirectToAction("Edit", new { id = profile.Id });
		}

		[HttpPost]
		public JsonResult FileUpload(int id)
		{
			var success = false;
			string error = null;
			string tempFile = "";
			string fileList = "";

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var postedFile = Request.ToPostedFileResult();
				if (postedFile != null)
				{
					if (id == 0)
					{
						var path = Path.Combine(FileSystemHelper.TempDir(), postedFile.FileName);
						FileSystemHelper.Delete(path);

						success = postedFile.Stream.ToFile(path);

						if (success)
							tempFile = postedFile.FileName;
					}
					else
					{
						var profile = _importService.GetImportProfileById(id);
						if (profile != null)
						{
							var files = profile.GetImportFiles();
							if (files.Any())
							{
								var extension = Path.GetExtension(files.First());

								if (!postedFile.FileExtension.IsCaseInsensitiveEqual(extension))
									error = T("Admin.Common.FileTypeMustEqual", extension.Substring(1).ToUpper());
							}

							if (!error.HasValue())
							{
								var folder = profile.GetImportFolder(true, true);
								var destFile = Path.Combine(folder, Path.GetFileName(postedFile.FileName));

								success = postedFile.Stream.ToFile(destFile);

								if (success)
								{
									var model = new ImportProfileModel();
									PrepareProfileModel(model, profile, false);

									fileList = this.RenderPartialViewToString("_ImportFileList", model);
								}
							}
						}
					}
				}
			}
			else
			{
				error = T("Admin.AccessDenied.Description");
			}

			if (!success && error.IsEmpty())
				error = T("Admin.Common.UploadFileFailed");

			if (error.HasValue())
				NotifyError(error);

			return Json(new { success = success, tempFile = tempFile, fileList = fileList });
		}

		[HttpPost]
		public ActionResult Execute(int id)
		{
			// permissions checked internally by DataImporter

			var profile = _importService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var taskParams = new Dictionary<string, string>();
			taskParams.Add("CurrentCustomerId", _services.WorkContext.CurrentCustomer.Id.ToString());

			_taskScheduler.RunSingleTask(profile.SchedulingTaskId, taskParams);

			NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress"));

			return RedirectToAction("List");
		}

		public ActionResult DownloadLogFile(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var path = profile.GetImportLogPath();
			var stream = new FileStream(path, FileMode.Open);

			var result = new FileStreamResult(stream, "text/plain; charset=utf-8");
			result.FileDownloadName = profile.Name.ToValidFileName() + "-log.txt";

			return result;
		}

		public ActionResult DownloadImportFile(int id, string name)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var path = Path.Combine(profile.GetImportFolder(true), name);

			if (!System.IO.File.Exists(path))
				path = Path.Combine(profile.GetImportFolder(false), name);

			var stream = new FileStream(path, FileMode.Open);

			var result = new FileStreamResult(stream, MimeTypes.MapNameToMimeType(path));
			result.FileDownloadName = Path.GetFileName(path);

			return result;
		}

		public ActionResult DeleteImportFile(int id, string name)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var profile = _importService.GetImportProfileById(id);
				if (profile != null)
				{
					var path = Path.Combine(profile.GetImportFolder(true), name);
					FileSystemHelper.Delete(path);

					var model = new ImportProfileModel();
					PrepareProfileModel(model, profile, false);

					return PartialView("_ImportFileList", model);
				}
			}
			return new EmptyResult();
		}
	}
}