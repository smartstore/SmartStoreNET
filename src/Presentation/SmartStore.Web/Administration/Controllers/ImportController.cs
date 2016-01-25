using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using SmartStore.Admin.Extensions;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
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
		private readonly ILanguageService _languageService;

		public ImportController(
			ICommonServices services,
			IImportProfileService importService,
			IDateTimeHelper dateTimeHelper,
			ITaskScheduler taskScheduler,
			ILanguageService languageService)
		{
			_services = services;
			_importService = importService;
			_dateTimeHelper = dateTimeHelper;
			_taskScheduler = taskScheduler;
			_languageService = languageService;
		}

		#region Utilities

		private void PrepareColumnMappingModels(ImportProfile profile, ImportProfileModel model, CsvConfiguration csvConfiguration, ColumnMap invalidMap = null)
		{
			model.ColumnMappings = new List<ColumnMappingItemModel>();
			model.AvailableEntityProperties = new List<SelectListItem>();

			try
			{
				var files = profile.GetImportFiles();
				if (!files.Any())
					return;

				var filePath = files.First();
				var mapConverter = new ColumnMapConverter();
				var allLanguages = _languageService.GetAllLanguages(true);

				var map = (invalidMap != null ? invalidMap : mapConverter.ConvertFrom<ColumnMap>(profile.ColumnMapping));
				var hasMappings = (map != null && map.Mappings.Any());

				var destProperties = _importService.GetImportableEntityProperties(profile.EntityType);

				model.AvailableEntityProperties = destProperties
					.Select(x => new SelectListItem { Value = x.Key, Text = x.Value	})
					.OrderBy(x => x.Text)
					.ToList();

				using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var dataTable = LightweightDataTable.FromFile(Path.GetFileName(filePath), stream, stream.Length, csvConfiguration, 0, 1);

					foreach (var column in dataTable.Columns.Where(x => x.Name.HasValue()))
					{
						string columnWithoutIndex;
						string columnIndex;
						ColumnMap.ParseSourceColumn(column.Name, out columnWithoutIndex, out columnIndex);

						var mapModel = new ColumnMappingItemModel
						{
							Index = dataTable.Columns.IndexOf(column),
							Column = column.Name,
							ColumnWithoutIndex = columnWithoutIndex,
							ColumnIndex = columnIndex
						};

						if (destProperties.ContainsKey(column.Name))
						{
							mapModel.ColumnLocalized = destProperties[column.Name];
						}

						if (columnIndex.HasValue())
						{
							var language = allLanguages.FirstOrDefault(x => x.UniqueSeoCode.IsCaseInsensitiveEqual(columnIndex));
							if (language != null)
							{
								mapModel.LanguageDescription = LocalizationHelper.GetLanguageNativeName(language.LanguageCulture);
								mapModel.FlagImageFileName = language.FlagImageFileName;
							}
						}

						if (hasMappings)
						{
							var mapping = map.Mappings.FirstOrDefault(x => x.Key == column.Name);
							if (mapping.Value != null)
							{
								mapModel.Property = mapping.Value.Property;
								mapModel.Default = mapping.Value.Default;
								mapModel.IsNilProperty = mapping.Value.Property.IsEmpty();
							}
						}

						model.ColumnMappings.Add(mapModel);
					}
				}
			}
			catch (Exception exception)
			{
				NotifyError(exception, true, false);
			}
		}

		private void PrepareProfileModel(ImportProfileModel model, ImportProfile profile, bool forEdit, ColumnMap invalidMap = null)
		{
			if (profile != null)
			{
				model.Id = profile.Id;
				model.Name = profile.Name;
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
				model.UnspecifiedString = T("Common.Unspecified");

				model.ExistingFileNames = profile.GetImportFiles()
					.Select(x => Path.GetFileName(x))
					.ToList();

				if (forEdit)
				{
					CsvConfiguration csvConfiguration = null;

					if (profile.FileType == ImportFileType.CSV)
					{
						var csvConverter = new CsvConfigurationConverter();
						csvConfiguration = csvConverter.ConvertFrom<CsvConfiguration>(profile.FileTypeConfiguration) ?? CsvConfiguration.ExcelFriendlyConfiguration;

						model.CsvConfiguration = new CsvConfigurationModel(csvConfiguration);
					}

					PrepareColumnMappingModels(profile, model, csvConfiguration ?? CsvConfiguration.ExcelFriendlyConfiguration, invalidMap);
				}
			}
			else
			{
				if (model.Name.IsEmpty())
				{
					var defaultNames = T("Admin.DataExchange.Import.DefaultProfileNames").Text.SplitSafe(";");

					model.Name = defaultNames.SafeGet((int)model.EntityType);
				}

				model.ExistingFileNames = new List<string>();
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

			var model = new ImportProfileListModel
			{
				Profiles = new List<ImportProfileModel>(),
				AvailableEntityTypes = ImportEntityType.Product.ToSelectList(false).ToList()
			};

			var profiles = _importService.GetImportProfiles().ToList();

			foreach (var profile in profiles)
			{
				var profileModel = new ImportProfileModel();

				PrepareProfileModel(profileModel, profile, false);

				profileModel.TaskModel = profile.ScheduleTask.ToScheduleTaskModel(_services.Localization, _dateTimeHelper, Url);

				if (profile.ResultInfo.HasValue())
				{
					profileModel.ImportResult = XmlHelper.Deserialize<SerializableImportResult>(profile.ResultInfo);
				}

				model.Profiles.Add(profileModel);
			}

			return View(model);
		}

		public ActionResult ProfileListDetails(int profileId)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var profile = _importService.GetImportProfileById(profileId);
				if (profile != null)
				{
					var importResult = XmlHelper.Deserialize<SerializableImportResult>(profile.ResultInfo);

					return Json(new
					{
						importResult = this.RenderPartialViewToString("ProfileImportResult", importResult)
					},
					JsonRequestBehavior.AllowGet);
				}
			}

			return new EmptyResult();
		}

		public ActionResult Create(ImportEntityType entityType)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var model = new ImportProfileModel
			{
				EntityType = entityType
			};

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

			var hasErrors = false;
			var map = new ColumnMap();
			var mapConverter = new ColumnMapConverter();
			CsvConfiguration csvConfig = null;

			if (!ModelState.IsValid)
			{
				PrepareProfileModel(model, profile, true, map);
				return View(model);
			}

			try
			{
				if (model.CsvConfiguration != null)
				{
					csvConfig = model.CsvConfiguration.Clone();
				}

				var filePath = profile.GetImportFiles().First();
				using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var dataTable = LightweightDataTable.FromFile(Path.GetFileName(filePath), stream, stream.Length, csvConfig ?? CsvConfiguration.ExcelFriendlyConfiguration, 0, 1);

					foreach (var column in dataTable.Columns.Where(x => x.Name.HasValue()))
					{
						var index = dataTable.Columns.IndexOf(column);
						var key = "ColumnMapping.Property." + index.ToString();

						if (form.AllKeys.Contains(key))
						{
							// allow to nil an entity property name to explicitly ignore a column
							var entityProperty = form[key];
							var defaultValue = form["ColumnMapping.Default." + index.ToString()];

							map.AddMapping(column.Name, null, entityProperty, defaultValue);
						}
					}

					// add model state error for invalid mappings
					foreach (var invalidMapping in map.GetInvalidMappings())
					{
						// do not mark columns as invalid where column name equals entity property name
						if (invalidMapping.Key != invalidMapping.Value.Property)
						{
							var column = dataTable.Columns.First(x => x.Name == invalidMapping.Key);
							var index = dataTable.Columns.IndexOf(column);
							var key = "ColumnMapping.Property." + index.ToString();

							ModelState.AddModelError(key, T("Admin.DataExchange.ColumnMapping.Validate.EntityMultipleMapped", invalidMapping.Value.Property));
						}
					}
				}
			}
			catch (Exception exception)
			{
				hasErrors = true;
				NotifyError(exception, true, false);
			}

			if (!ModelState.IsValid)
			{
				PrepareProfileModel(model, profile, true, map);
				return View(model);
			}

			profile.Name = model.Name;
			profile.EntityType = model.EntityType;
			profile.Enabled = model.Enabled;
			profile.Skip = model.Skip;
			profile.Take = model.Take;
			profile.FileTypeConfiguration = null;

			try
			{
				profile.ColumnMapping = mapConverter.ConvertTo(map);

				if (profile.FileType == ImportFileType.CSV && csvConfig != null)
				{
					var csvConverter = new CsvConfigurationConverter();
					profile.FileTypeConfiguration = csvConverter.ConvertTo(csvConfig);
				}
			}
			catch (Exception exception)
			{
				hasErrors = true;
				NotifyError(exception, true, false);
			}

			if (!hasErrors)
			{
				_importService.UpdateImportProfile(profile);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			}

			return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
		}

		[HttpPost]
		public ActionResult ResetColumnMappings(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			profile.ColumnMapping = null;
			_importService.UpdateImportProfile(profile);

			NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

			return RedirectToAction("Edit", new { id = profile.Id });
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
								var fileName = Path.GetFileName(postedFile.FileName);

								success = postedFile.Stream.ToFile(Path.Combine(folder, fileName));

								if (success)
								{
									var fileType = (Path.GetExtension(fileName).IsCaseInsensitiveEqual(".xlsx") ? ImportFileType.XLSX : ImportFileType.CSV);
									if (fileType != profile.FileType)
									{
										profile.FileType = fileType;
										_importService.UpdateImportProfile(profile);
									}
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

			return Json(new { success = success, tempFile = tempFile });
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

			NotifyInfo(T("Admin.DataExchange.Import.RunNowNote"));

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

			var result = new FileStreamResult(stream, MediaTypeNames.Text.Plain);
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

		[HttpPost]
		public ActionResult DeleteImportFile(int id, string name)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var profile = _importService.GetImportProfileById(id);
				if (profile != null)
				{
					var importFiles = profile.GetImportFiles();
					var path = Path.Combine(profile.GetImportFolder(true), name);
					FileSystemHelper.Delete(path);
				}
			}
			return RedirectToAction("Edit", new { id = id });
		}
	}
}