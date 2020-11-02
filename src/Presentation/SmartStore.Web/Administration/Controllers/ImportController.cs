using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.IO;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog.Importer;
using SmartStore.Services.Customers.Importer;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages.Importer;
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
        private readonly IImportProfileService _importProfileService;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly AdminModelHelper _adminModelHelper;

        public ImportController(
            IImportProfileService importService,
            ITaskScheduler taskScheduler,
            IScheduleTaskService scheduleTaskService,
            AdminModelHelper adminModelHelper)
        {
            _importProfileService = importService;
            _taskScheduler = taskScheduler;
            _scheduleTaskService = scheduleTaskService;
            _adminModelHelper = adminModelHelper;
        }

        #region Utilities

        private bool IsValidImportFile(string path, out string error)
        {
            error = null;

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var unused = LightweightDataTable.FromFile(path, stream, stream.Length, CsvConfiguration.ExcelFriendlyConfiguration, 0, 1);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToAllMessages();
                FileSystemHelper.DeleteFile(path);
                return false;
            }
        }

        private bool IsDefaultValueDisabled(string column, string property, string[] disabledFieldNames)
        {
            if (disabledFieldNames.Contains(property))
                return true;

            string columnWithoutIndex, columnIndex;
            if (ColumnMap.ParseSourceName(property, out columnWithoutIndex, out columnIndex))
                return disabledFieldNames.Contains(columnWithoutIndex);

            return false;
        }

        private string[] GetDisabledDefaultFieldNames(ImportProfile profile)
        {
            switch (profile.EntityType)
            {
                case ImportEntityType.Product:
                    return new string[] { "Name", "Sku", "ManufacturerPartNumber", "Gtin", "SeName" };
                case ImportEntityType.Category:
                    return new string[] { "Name", "SeName" };
                case ImportEntityType.Customer:
                    return new string[] { "CustomerGuid", "Email" };
                case ImportEntityType.NewsLetterSubscription:
                    return new string[] { "Email" };
                default:
                    return new string[0];
            }
        }

        private string GetPropertyDescription(Dictionary<string, string> allProperties, string property)
        {
            if (property.HasValue() && allProperties.ContainsKey(property))
            {
                var result = allProperties[property];
                if (result.HasValue())
                    return result;
            }
            return property;
        }

        private void PrepareProfileModel(
            ImportProfileModel model,
            ImportProfile profile,
            ScheduleTaskHistory lastHistoryEntry,
            bool forEdit,
            ColumnMap invalidMap = null)
        {
            model.Id = profile.Id;
            model.Name = profile.Name;
            model.EntityType = profile.EntityType;
            model.Enabled = profile.Enabled;
            model.ImportRelatedData = profile.ImportRelatedData;
            model.Skip = profile.Skip == 0 ? (int?)null : profile.Skip;
            model.Take = profile.Take == 0 ? (int?)null : profile.Take;
            model.UpdateOnly = profile.UpdateOnly;
            model.KeyFieldNames = profile.KeyFieldNames.SplitSafe(",").Distinct().ToArray();
            model.ScheduleTaskId = profile.SchedulingTaskId;
            model.ScheduleTaskName = profile.ScheduleTask.Name.NaIfEmpty();
            model.IsTaskRunning = lastHistoryEntry?.IsRunning ?? false;
            model.IsTaskEnabled = profile.ScheduleTask.Enabled;
            model.LogFileExists = System.IO.File.Exists(profile.GetImportLogPath());
            model.EntityTypeName = profile.EntityType.GetLocalizedEnum(Services.Localization, Services.WorkContext);
            model.ExistingFiles = profile.GetImportFiles();

            foreach (var file in model.ExistingFiles)
            {
                if (file.RelatedType.HasValue)
                {
                    file.Label = string.Concat(T("Admin.Common.Data"), " ", file.RelatedType.Value.GetLocalizedEnum(Services.Localization, Services.WorkContext));
                }
            }

            if (profile.ResultInfo.HasValue())
            {
                model.ImportResult = XmlHelper.Deserialize<SerializableImportResult>(profile.ResultInfo);
            }

            if (!forEdit)
            {
                return;
            }

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

            // Common configuration.
            var extraData = XmlHelper.Deserialize<ImportExtraData>(profile.ExtraData);
            model.ExtraData.NumberOfPictures = extraData.NumberOfPictures;

            // Column mapping.
            model.AvailableSourceColumns = new List<ColumnMappingItemModel>();
            model.AvailableEntityProperties = new List<ColumnMappingItemModel>();
            model.AvailableKeyFieldNames = new List<SelectListItem>();
            model.ColumnMappings = new List<ColumnMappingItemModel>();

            model.FolderName = profile.GetImportFolder(absolutePath: false);

            try
            {
                string[] availableKeyFieldNames = null;
                string[] disabledDefaultFieldNames = GetDisabledDefaultFieldNames(profile);
                var mapConverter = new ColumnMapConverter();
                var storedMap = mapConverter.ConvertFrom<ColumnMap>(profile.ColumnMapping);
                var map = (invalidMap ?? storedMap) ?? new ColumnMap();

                // Property name to localized property name.
                var allProperties = _importProfileService.GetImportableEntityProperties(profile.EntityType) ?? new Dictionary<string, string>();

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
                    .Select(x =>
                    {
                        var mapping = new ColumnMappingItemModel
                        {
                            Property = x.Key,
                            PropertyDescription = x.Value,
                            IsDefaultDisabled = IsDefaultValueDisabled(x.Key, x.Key, disabledDefaultFieldNames)
                        };

                        return mapping;
                    })
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
                            Column = x.Value.MappedName,
                            Property = x.Key,
                            Default = x.Value.Default
                        };

                        if (x.Value.IgnoreProperty)
                        {
                            // Explicitly ignore the property.
                            mapping.Column = null;
                            mapping.Default = null;
                        }

                        mapping.PropertyDescription = GetPropertyDescription(allProperties, mapping.Property);
                        mapping.IsDefaultDisabled = IsDefaultValueDisabled(mapping.Column, mapping.Property, disabledDefaultFieldNames);

                        return mapping;
                    })
                    .ToList();

                var file = model.ExistingFiles.FirstOrDefault(x => !x.RelatedType.HasValue);
                if (file == null)
                {
                    return;
                }

                using (var stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var dataTable = LightweightDataTable.FromFile(Path.GetFileName(file.Path), stream, stream.Length, csvConfiguration, 0, 1);

                    foreach (var column in dataTable.Columns.Where(x => x.Name.HasValue()))
                    {
                        string columnWithoutIndex, columnIndex;
                        ColumnMap.ParseSourceName(column.Name, out columnWithoutIndex, out columnIndex);

                        model.AvailableSourceColumns.Add(new ColumnMappingItemModel
                        {
                            Index = dataTable.Columns.IndexOf(column),
                            Column = column.Name,
                            ColumnWithoutIndex = columnWithoutIndex,
                            ColumnIndex = columnIndex,
                            PropertyDescription = GetPropertyDescription(allProperties, column.Name)
                        });

                        // Auto map where field equals property name.
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
                                    PropertyDescription = kvp.Value,
                                    IsDefaultDisabled = IsDefaultValueDisabled(column.Name, kvp.Key, disabledDefaultFieldNames)
                                });
                            }
                        }
                    }

                    // Sorting.
                    model.AvailableSourceColumns = model.AvailableSourceColumns
                        .OrderBy(x => x.PropertyDescription)
                        .ToList();

                    model.AvailableEntityProperties = model.AvailableEntityProperties
                        .OrderBy(x => x.PropertyDescription)
                        .ToList();

                    model.ColumnMappings = model.ColumnMappings
                        .OrderBy(x => x.PropertyDescription)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex, true, false);
            }
        }

        #endregion

        #region List / Create

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Import.Read)]
        public ActionResult List()
        {
            var model = new ImportProfileListModel
            {
                Profiles = new List<ImportProfileModel>(),
                AvailableEntityTypes = ImportEntityType.Product.ToSelectList(false).ToList()
            };

            var lastHistoryEntries = _scheduleTaskService.GetHistoryEntries(0, int.MaxValue, 0, true, true).ToDictionarySafe(x => x.ScheduleTaskId);
            var profiles = _importProfileService.GetImportProfiles().ToList();

            foreach (var profile in profiles)
            {
                var profileModel = new ImportProfileModel();
                lastHistoryEntries.TryGetValue(profile.SchedulingTaskId, out var lastHistoryEntry);

                PrepareProfileModel(profileModel, profile, lastHistoryEntry, false);

                profileModel.TaskModel = _adminModelHelper.CreateScheduleTaskModel(profile.ScheduleTask, lastHistoryEntry) ?? new ScheduleTaskModel();

                model.Profiles.Add(profileModel);
            }

            return View(model);
        }

        [Permission(Permissions.Configuration.Import.Read)]
        public ActionResult ProfileListDetails(int profileId)
        {
            var profile = _importProfileService.GetImportProfileById(profileId);
            if (profile != null)
            {
                var importResult = XmlHelper.Deserialize<SerializableImportResult>(profile.ResultInfo);

                return Json(new
                {
                    importResult = this.RenderPartialViewToString("~/Administration/Views/Import/ProfileImportResult.cshtml", importResult)
                },
                JsonRequestBehavior.AllowGet);
            }

            return new EmptyResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Import.Create)]
        public ActionResult Create(ImportProfileModel model)
        {
            if (PathHelper.HasInvalidFileNameChars(model.TempFileName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file name.");
            }

            var importFile = Path.Combine(FileSystemHelper.TempDirTenant(), model.TempFileName.EmptyNull());

            if (System.IO.File.Exists(importFile))
            {
                var profile = _importProfileService.InsertImportProfile(model.TempFileName, model.Name, model.EntityType);

                if (profile != null && profile.Id != 0)
                {
                    var importFileDestination = Path.Combine(profile.GetImportFolder(true, true), model.TempFileName);

                    FileSystemHelper.CopyFile(importFile, importFileDestination, true, true);

                    return RedirectToAction("Edit", new { id = profile.Id });
                }
            }
            else
            {
                NotifyError(T("Admin.DataExchange.Import.MissingImportFile"));
            }

            return RedirectToAction("List");
        }

        #endregion

        #region Edit

        [Permission(Permissions.Configuration.Import.Read)]
        public ActionResult Edit(int id)
        {
            var profile = _importProfileService.GetImportProfileById(id);
            if (profile == null)
                return RedirectToAction("List");

            var model = new ImportProfileModel();
            PrepareProfileModel(model, profile, _scheduleTaskService.GetLastHistoryEntryByTaskId(profile.SchedulingTaskId, true), true);

            return View(model);
        }

        [Permission(Permissions.Configuration.Import.Update)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ImportProfileModel model, bool continueEditing, FormCollection form)
        {
            var profile = _importProfileService.GetImportProfileById(model.Id);
            if (profile == null)
                return RedirectToAction("List");

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

                        if (column.IsEmpty())
                        {
                            // Tell mapper to explicitly ignore the property.
                            map.AddMapping(property, null, property, "[IGNOREPROPERTY]");
                        }
                        else if (!column.IsCaseInsensitiveEqual(property) || defaultValue.HasValue())
                        {
                            if (defaultValue.HasValue() && GetDisabledDefaultFieldNames(profile).Contains(property))
                                defaultValue = null;

                            map.AddMapping(property, null, column, defaultValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                hasErrors = true;
                NotifyError(ex, true, false);
            }

            if (!ModelState.IsValid || hasErrors)
            {
                PrepareProfileModel(model, profile, _scheduleTaskService.GetLastHistoryEntryByTaskId(profile.SchedulingTaskId, true), true, map);
                return View(model);
            }

            profile.Name = model.Name;
            profile.EntityType = model.EntityType;
            profile.Enabled = model.Enabled;
            profile.ImportRelatedData = model.ImportRelatedData;
            profile.Skip = model.Skip ?? 0;
            profile.Take = model.Take ?? 0;
            profile.UpdateOnly = model.UpdateOnly;
            profile.KeyFieldNames = model.KeyFieldNames == null ? null : string.Join(",", model.KeyFieldNames);

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

                if (model.ExtraData != null)
                {
                    var extraData = new ImportExtraData
                    {
                        NumberOfPictures = model.ExtraData.NumberOfPictures
                    };

                    profile.ExtraData = XmlHelper.Serialize(extraData);
                }
            }
            catch (Exception ex)
            {
                hasErrors = true;
                NotifyError(ex, true, false);
            }

            if (!hasErrors)
            {
                _importProfileService.UpdateImportProfile(profile);

                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

                if (resetMappings)
                {
                    NotifyWarning(T("Admin.DataExchange.ColumnMapping.Validate.MappingsReset"));
                }
            }

            return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Import.Update)]
        public ActionResult ResetColumnMappings(int id)
        {
            var profile = _importProfileService.GetImportProfileById(id);
            if (profile == null)
                return RedirectToAction("List");

            profile.ColumnMapping = null;
            _importProfileService.UpdateImportProfile(profile);

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            return RedirectToAction("Edit", new { id = profile.Id });
        }

        #endregion

        #region Execute / Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Import.Execute)]
        public ActionResult Execute(int id)
        {
            // Permissions checked internally by DataImporter.
            var profile = _importProfileService.GetImportProfileById(id);
            if (profile == null)
            {
                return RedirectToAction("List");
            }

            var taskParams = new Dictionary<string, string>
            {
                { TaskExecutor.CurrentCustomerIdParamName, Services.WorkContext.CurrentCustomer.Id.ToString() },
                { TaskExecutor.CurrentStoreIdParamName, Services.StoreContext.CurrentStore.Id.ToString() }
            };

            _taskScheduler.RunSingleTask(profile.SchedulingTaskId, taskParams);

            NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress.DataImportTask"));

            return RedirectToReferrer(null, () => RedirectToAction("List"));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Import.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var profile = _importProfileService.GetImportProfileById(id);
            if (profile == null)
                return RedirectToAction("List");

            try
            {
                _importProfileService.DeleteImportProfile(profile);

                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Edit", new { id = profile.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Import.Delete)]
        public ActionResult DeleteImportFile(int id, string name)
        {
            var profile = _importProfileService.GetImportProfileById(id);
            if (profile != null)
            {
                var path = Path.Combine(profile.GetImportFolder(true), name);
                FileSystemHelper.DeleteFile(path);
            }

            return RedirectToAction("Edit", new { id });
        }

        #endregion

        #region Upload / Download

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Import.Update)]
        public JsonResult FileUpload(int id)
        {
            var success = false;
            string error = null;
            string tempFile = "";

            var postedFile = Request.ToPostedFileResult();
            if (postedFile != null)
            {
                if (id == 0)
                {
                    var path = Path.Combine(FileSystemHelper.TempDirTenant(), postedFile.FileName);
                    FileSystemHelper.DeleteFile(path);

                    success = postedFile.Stream.ToFile(path);
                    if (success)
                    {
                        success = IsValidImportFile(path, out error);
                        if (success)
                            tempFile = postedFile.FileName;
                    }
                }
                else
                {
                    var profile = _importProfileService.GetImportProfileById(id);
                    if (profile != null)
                    {
                        var files = profile.GetImportFiles(false);
                        var file = files.FirstOrDefault();
                        if (file != null && !postedFile.FileExtension.IsCaseInsensitiveEqual(file.Extension))
                        {
                            error = T("Admin.Common.FileTypeMustEqual", file.Extension.Substring(1).ToUpper());
                        }

                        if (!error.HasValue())
                        {
                            var folder = profile.GetImportFolder(true, true);
                            var fileName = Path.GetFileName(postedFile.FileName);
                            var path = Path.Combine(folder, fileName);

                            success = postedFile.Stream.ToFile(path);
                            if (success)
                            {
                                success = IsValidImportFile(path, out error);
                                if (success)
                                {
                                    var fileType = Path.GetExtension(fileName).IsCaseInsensitiveEqual(".xlsx") ? ImportFileType.XLSX : ImportFileType.CSV;
                                    if (fileType != profile.FileType)
                                    {
                                        var tmp = new ImportFile(path);
                                        if (!tmp.RelatedType.HasValue)
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
            }

            if (!success && error.IsEmpty())
            {
                error = T("Admin.Common.UploadFileFailed");
            }
            if (error.HasValue())
            {
                NotifyError(error);
            }

            return Json(new { success, tempFile, error, name = postedFile.FileName, ext = postedFile.FileExtension });
        }

        [Permission(Permissions.Configuration.Import.Read)]
        public ActionResult DownloadLogFile(int id)
        {
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

        [HttpGet]
        public ActionResult DownloadImportFile(int id, string name)
        {
            if (PathHelper.HasInvalidFileNameChars(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file name.");
            }

            string message = null;

            if (Services.Permissions.Authorize(Permissions.Configuration.Import.Read))
            {
                var profile = _importProfileService.GetImportProfileById(id);
                if (profile != null)
                {
                    var path = Path.Combine(profile.GetImportFolder(true), name);

                    if (!System.IO.File.Exists(path))
                    {
                        path = Path.Combine(profile.GetImportFolder(false), name);
                    }

                    if (System.IO.File.Exists(path))
                    {
                        try
                        {
                            var stream = new FileStream(path, FileMode.Open);

                            var result = new FileStreamResult(stream, MimeTypes.MapNameToMimeType(path))
                            {
                                FileDownloadName = Path.GetFileName(path)
                            };

                            return result;
                        }
                        catch (IOException)
                        {
                            message = T("Admin.Common.FileInUse");
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

        #endregion
    }
}