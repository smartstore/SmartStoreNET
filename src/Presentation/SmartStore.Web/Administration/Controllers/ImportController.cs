using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using SmartStore.Admin.Extensions;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Services;
using SmartStore.Services.Catalog.Importer;
using SmartStore.Services.Customers.Importer;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages.Importer;
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
		private readonly IImportProfileService _importProfileService;
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
			_importProfileService = importService;
			_dateTimeHelper = dateTimeHelper;
			_taskScheduler = taskScheduler;
			_languageService = languageService;
		}

		#region Utilities

		private void PrepareProfileModel(ImportProfileModel model, ImportProfile profile, bool forEdit, ColumnMap invalidMap = null)
		{
			model.Id = profile.Id;
			model.Name = profile.Name;
			model.EntityType = profile.EntityType;
			model.Enabled = profile.Enabled;
			model.Skip = (profile.Skip == 0 ? (int?)null : profile.Skip);
			model.Take = (profile.Take == 0 ? (int?)null : profile.Take);
			model.UpdateOnly = profile.UpdateOnly;
			model.KeyFieldNames = profile.KeyFieldNames.SplitSafe(",").Distinct().ToArray();
			model.ScheduleTaskId = profile.SchedulingTaskId;
			model.ScheduleTaskName = profile.ScheduleTask.Name.NaIfEmpty();
			model.IsTaskRunning = profile.ScheduleTask.IsRunning;
			model.IsTaskEnabled = profile.ScheduleTask.Enabled;
			model.LogFileExists = System.IO.File.Exists(profile.GetImportLogPath());
			model.EntityTypeName = profile.EntityType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
			model.UnspecifiedString = T("Common.Unspecified");
			model.AddNewString = T("Common.AddNew");
			model.DeleteString = T("Common.Delete");
			model.IgnoreString = T("Admin.Common.Ignore");

			model.ExistingFileNames = profile.GetImportFiles()
				.Select(x => Path.GetFileName(x))
				.ToList();

			if (profile.ResultInfo.HasValue())
				model.ImportResult = XmlHelper.Deserialize<SerializableImportResult>(profile.ResultInfo);

			if (!forEdit)
				return;

			CsvConfiguration csvConfiguration = null;

			if (profile.FileType == ImportFileType.CSV)
			{
				var csvConverter = new CsvConfigurationConverter();
				csvConfiguration = csvConverter.ConvertFrom<CsvConfiguration>(profile.FileTypeConfiguration) ?? CsvConfiguration.ExcelFriendlyConfiguration;

				model.CsvConfiguration = new CsvConfigurationModel(csvConfiguration);
			}
			else
			{
				csvConfiguration = CsvConfiguration.ExcelFriendlyConfiguration;
			}

			// column mapping
			model.AvailableSourceColumns = new List<ColumnMappingItemModel>();
			model.AvailableEntityProperties = new List<SelectListItem>();
			model.AvailableKeyFieldNames = new List<SelectListItem>();
			model.ColumnMappings = new List<ColumnMappingItemModel>();

			try
			{
				string[] availableKeyFieldNames = null;
				var mapConverter = new ColumnMapConverter();
				var storedMap = mapConverter.ConvertFrom<ColumnMap>(profile.ColumnMapping);
				//var hasStoredMappings = (storedMap != null && storedMap.Mappings.Any());
				var map = (invalidMap ?? storedMap) ?? new ColumnMap();

				// property name to localized property name
				var allProperties = _importProfileService.GetImportableEntityProperties(profile.EntityType);

				switch (profile.EntityType)
				{
					case ImportEntityType.Product:
						availableKeyFieldNames = ProductImporter.SupportedKeyFields;
						break;
					case ImportEntityType.Category:
						availableKeyFieldNames = CategoryImporter.SupportedKeyFields;
						break;
					case ImportEntityType.Customer:
						availableKeyFieldNames = CustomerImporter.SupportedKeyFields;
						break;
					case ImportEntityType.NewsLetterSubscription:
						availableKeyFieldNames = NewsLetterSubscriptionImporter.SupportedKeyFields;
						break;
				}

				model.AvailableEntityProperties = allProperties
					.Select(x => new SelectListItem { Value = x.Key, Text = x.Value })
					.OrderBy(x => x.Text)
					.ToList();

				model.AvailableKeyFieldNames = availableKeyFieldNames
					.Select(x =>
					{
						var item = new SelectListItem { Value = x, Text = x };

						if (x == "Id")
							item.Text = T("Admin.Common.Entity.Fields.Id");
						else if (allProperties.ContainsKey(x))
							item.Text = allProperties[x];

						return item;
					})
					.ToList();

				model.ColumnMappings = map.Mappings
					.Select(x =>
					{
						var mapping = new ColumnMappingItemModel
						{
							Column = x.Key,
							Property = x.Value.Property,
							Default = x.Value.Default
						};

						if (mapping.Default != null && mapping.Default == "[NULL]")
						{
							// explicitly ignore this column
							mapping.Column = null;
							mapping.Default = null;
						}

						// add localized to make mappings sortable
						if (allProperties.ContainsKey(mapping.Property))
						{
							mapping.ColumnLocalized = allProperties[mapping.Property];
						}

						return mapping;
					})
					.ToList();

				var files = profile.GetImportFiles();
				if (!files.Any())
					return;

				var filePath = files.First();

				using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var dataTable = LightweightDataTable.FromFile(Path.GetFileName(filePath), stream, stream.Length, csvConfiguration, 0, 1);

					foreach (var column in dataTable.Columns.Where(x => x.Name.HasValue()))
					{
						string columnWithoutIndex, columnIndex;
						ColumnMap.ParseSourceColumn(column.Name, out columnWithoutIndex, out columnIndex);

						var mapModel = new ColumnMappingItemModel
						{
							Index = dataTable.Columns.IndexOf(column),
							Column = column.Name,
							ColumnWithoutIndex = columnWithoutIndex,
							ColumnIndex = columnIndex,
							ColumnLocalized = (allProperties.ContainsKey(column.Name) ? allProperties[column.Name] : column.Name)
						};

						model.AvailableSourceColumns.Add(mapModel);

						// auto map where field equals property name
						if (!model.ColumnMappings.Any(x => x.Column == column.Name))
						{
							var kvp = allProperties.FirstOrDefault(x => x.Key.IsCaseInsensitiveEqual(column.Name));
							if (kvp.Key.IsEmpty())
							{
								var alternativeName = LightweightDataTable.GetAlternativeColumnNameFor(column.Name);
								kvp = allProperties.FirstOrDefault(x => x.Key.IsCaseInsensitiveEqual(alternativeName));
							}

							if (kvp.Key.HasValue() && !model.ColumnMappings.Any(x => x.Property == kvp.Key))
							{
								model.ColumnMappings.Add(new ColumnMappingItemModel
								{
									Column = column.Name,
									Property = kvp.Key,
									ColumnLocalized = kvp.Value
								});
							}
						}
					}

					// sorting
					model.AvailableSourceColumns = model.AvailableSourceColumns
						.OrderBy(x => x.ColumnLocalized)
						.ToList();

					model.ColumnMappings = model.ColumnMappings
						.OrderBy(x => x.ColumnLocalized)
						.ToList();
				}
			}
			catch (Exception exception)
			{
				NotifyError(exception, true, false);
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

			var profiles = _importProfileService.GetImportProfiles().ToList();

			foreach (var profile in profiles)
			{
				var profileModel = new ImportProfileModel();

				PrepareProfileModel(profileModel, profile, false);

				profileModel.TaskModel = profile.ScheduleTask.ToScheduleTaskModel(_services.Localization, _dateTimeHelper, Url);

				model.Profiles.Add(profileModel);
			}

			return View(model);
		}

		public ActionResult ProfileListDetails(int profileId)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var profile = _importProfileService.GetImportProfileById(profileId);
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

		[HttpPost]
		public ActionResult Create(ImportProfileModel model)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var importFile = Path.Combine(FileSystemHelper.TempDir(), model.TempFileName.EmptyNull());

			if (System.IO.File.Exists(importFile))
			{
				var profile = _importProfileService.InsertImportProfile(model.TempFileName, model.Name, model.EntityType);

				if (profile != null && profile.Id != 0)
				{
					var importFileDestination = Path.Combine(profile.GetImportFolder(true, true), model.TempFileName);

					FileSystemHelper.Copy(importFile, importFileDestination, true, true);

					return RedirectToAction("Edit", new { id = profile.Id });
				}
			}
			else
			{
				NotifyError(T("Admin.DataExchange.Import.MissingImportFile"));
			}

			return RedirectToAction("List");
		}

		public ActionResult Edit(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importProfileService.GetImportProfileById(id);
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

			var profile = _importProfileService.GetImportProfileById(model.Id);
			if (profile == null)
				return RedirectToAction("List");

			var multipleMapped = new List<string>();
			var map = new ColumnMap();
			var hasErrors = false;
			var resetMappings = false;

			try
			{
				var propertyKeyPrefix = "ColumnMapping.Property.";
				var allPropertyKeys = form.AllKeys.Where(x => x.HasValue() && x.StartsWith(propertyKeyPrefix));

				if (allPropertyKeys.Any())
				{
					var entityProperties = _importProfileService.GetImportableEntityProperties(profile.EntityType);

					foreach (var key in allPropertyKeys)
					{
						var index = key.Substring(propertyKeyPrefix.Length);
						var property = form[key];
						var column = form["ColumnMapping.Column." + index];
						var defaultValue = form["ColumnMapping.Default." + index];
						var result = true;

						if (column.IsEmpty())
						{
							// tell mapper to explicitly ignore this column
							result = map.AddMapping(property, null, property, "[NULL]");
						}
						else if (!column.IsCaseInsensitiveEqual(property) || defaultValue.HasValue())
						{
							result = map.AddMapping(column, null, property, defaultValue);
						}

						if (!result)
						{
							// add warning for ignored, multiple mapped properties
							multipleMapped.Add("{0} ({1})".FormatInvariant(entityProperties.ContainsKey(property) ? entityProperties[property] : "".NaIfEmpty(), property));
						}
					}
				}
			}
			catch (Exception exception)
			{
				hasErrors = true;
				NotifyError(exception, true, false);
			}

			if (!ModelState.IsValid || hasErrors)
			{
				PrepareProfileModel(model, profile, true, map);
				return View(model);
			}

			profile.Name = model.Name;
			profile.EntityType = model.EntityType;
			profile.Enabled = model.Enabled;
			profile.Skip = model.Skip ?? 0;
			profile.Take = model.Take ?? 0;
			profile.UpdateOnly = model.UpdateOnly;
			profile.KeyFieldNames = (model.KeyFieldNames == null ? null : string.Join(",", model.KeyFieldNames));

			try
			{
				if (profile.FileType == ImportFileType.CSV && model.CsvConfiguration != null)
				{
					var csvConverter = new CsvConfigurationConverter();

					var oldCsvConfig = csvConverter.ConvertFrom<CsvConfiguration>(profile.FileTypeConfiguration);
					var oldDelimiter = (oldCsvConfig != null ? oldCsvConfig.Delimiter.ToString() : null);

					// auto reset mappings cause they are invalid. note: delimiter can be whitespaced, so no oldDelimter.HasValue() etc.
					resetMappings = (oldDelimiter != model.CsvConfiguration.Delimiter && profile.ColumnMapping.HasValue());

					profile.FileTypeConfiguration = csvConverter.ConvertTo(model.CsvConfiguration.Clone());
				}
				else
				{
					profile.FileTypeConfiguration = null;
				}

				if (resetMappings)
				{
					profile.ColumnMapping = null;
				}
				else
				{
					var mapConverter = new ColumnMapConverter();
					profile.ColumnMapping = mapConverter.ConvertTo(map);
				}
			}
			catch (Exception exception)
			{
				hasErrors = true;
				NotifyError(exception, true, false);
			}

			if (!hasErrors)
			{
				_importProfileService.UpdateImportProfile(profile);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

				if (resetMappings)
				{
					NotifyWarning(T("Admin.DataExchange.ColumnMapping.Validate.MappingsReset"));
				}
				else if (multipleMapped.Any())
				{
					NotifyWarning(T("Admin.DataExchange.ColumnMapping.Validate.MultipleMappedIgnored", "<p>" + string.Join("<br />", multipleMapped) + "</p>"));
				}
			}

			return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
		}

		[HttpPost]
		public ActionResult ResetColumnMappings(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importProfileService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			profile.ColumnMapping = null;
			_importProfileService.UpdateImportProfile(profile);

			NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

			return RedirectToAction("Edit", new { id = profile.Id });
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importProfileService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			try
			{
				_importProfileService.DeleteImportProfile(profile);

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
						var profile = _importProfileService.GetImportProfileById(id);
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
										_importProfileService.UpdateImportProfile(profile);
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

			return Json(new { success = success, tempFile = tempFile, error = error });
		}

		[HttpPost]
		public ActionResult Execute(int id)
		{
			// permissions checked internally by DataImporter

			var profile = _importProfileService.GetImportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var taskParams = new Dictionary<string, string>();
			taskParams.Add(TaskExecutor.CurrentCustomerIdParamName, _services.WorkContext.CurrentCustomer.Id.ToString());

			_taskScheduler.RunSingleTask(profile.SchedulingTaskId, taskParams);

			NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress.DataImportTask"));

			var referrer = Services.WebHelper.GetUrlReferrer();
			if (referrer.HasValue())
				return Redirect(referrer);

			return RedirectToAction("List");
		}

		public ActionResult DownloadLogFile(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
				return AccessDeniedView();

			var profile = _importProfileService.GetImportProfileById(id);
			if (profile != null)
			{
				var path = profile.GetImportLogPath();
				if (System.IO.File.Exists(path))
				{
					try
					{
						var stream = new FileStream(path, FileMode.Open);
						var result = new FileStreamResult(stream, MediaTypeNames.Text.Plain);

						return result;
					}
					catch (IOException)
					{
						NotifyWarning(T("Admin.Common.FileInUse"));
					}
				}
			}

			return RedirectToAction("List");
		}

		public ActionResult DownloadImportFile(int id, string name)
		{
			string message = null;

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var profile = _importProfileService.GetImportProfileById(id);
				if (profile != null)
				{
					var path = Path.Combine(profile.GetImportFolder(true), name);

					if (!System.IO.File.Exists(path))
						path = Path.Combine(profile.GetImportFolder(false), name);

					if (System.IO.File.Exists(path))
					{
						try
						{
							var stream = new FileStream(path, FileMode.Open);

							var result = new FileStreamResult(stream, MimeTypes.MapNameToMimeType(path));
							result.FileDownloadName = Path.GetFileName(path);

							return result;
						}
						catch (IOException)
						{
							NotifyWarning(T("Admin.Common.FileInUse"));
						}
					}
				}
			}
			else
			{
				message = T("Admin.AccessDenied.Description");
			}

			if (message.IsEmpty())
			{
				message = T("Admin.Common.ResourceNotFound");
			}

			return File(Encoding.UTF8.GetBytes(message), MediaTypeNames.Text.Plain, "DownloadImportFile.txt");
		}

		[HttpPost]
		public ActionResult DeleteImportFile(int id, string name)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageImports))
			{
				var profile = _importProfileService.GetImportProfileById(id);
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