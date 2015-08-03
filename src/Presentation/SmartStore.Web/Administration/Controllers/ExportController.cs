using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Export;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Export;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Export;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class ExportController : AdminControllerBase
	{
		private readonly ICommonServices _services;
		private readonly IExportService _exportService;
		private readonly PluginMediator _pluginMediator;
		private readonly IPictureService _pictureService;

		public ExportController(
			ICommonServices services,
			IExportService exportService,
			PluginMediator pluginMediator,
			IPictureService pictureService)
		{
			_services = services;
			_exportService = exportService;
			_pluginMediator = pluginMediator;
			_pictureService = pictureService;
		}

		private void PrepareExportProfileModel(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			var partition = XmlHelper.Deserialize<ExportPartition>(profile.Partitioning);

			model.AvailableFileTypes = new List<SelectListItem>();

			model.Id = profile.Id;
			model.Name = profile.Name;
			model.ProviderSystemName = profile.ProviderSystemName;
			model.Enabled = profile.Enabled;
			model.FileType = profile.FileType;
			model.FileTypeName = profile.FileType.ToString().ToUpper();
			model.SchedulingHours = profile.ScheduleTask.Seconds / 3600;

			model.Offset = partition.Offset;
			model.Limit = partition.Limit;
			model.BatchSize = partition.BatchSize;
			model.PerStore = partition.PerStore;

			if (provider != null)
			{
				model.ThumbnailUrl = _pluginMediator.GetIconUrl(provider.Metadata);

				if (model.ThumbnailUrl.IsEmpty())
					model.ThumbnailUrl = _pictureService.GetDefaultPictureUrl(48);
				else
					model.ThumbnailUrl = Url.Content(model.ThumbnailUrl);

				model.ProviderFriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);

				model.EntityType = T("Enums.SmartStore.Core.Domain.ExportEntityType." + provider.Value.EntityType.ToString());

				foreach (var fileType in provider.Value.SupportedFileTypes)
				{
					model.AvailableFileTypes.Add(new SelectListItem
					{
						Text = T("Enums.SmartStore.Core.Domain.ExportFileType." + fileType.ToString()),
						Value = ((int)fileType).ToString()
					});
				}
			}
		}

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			return View();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			var model = new GridModel<ExportProfileModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
			{
				var providers = _exportService.LoadAllExportProviders().ToList();
				var query = _exportService.GetExportProfiles();
				var profiles = query.ToList();

				model.Total = profiles.Count;

				model.Data = profiles.Select(x =>
				{
					var profileModel = new ExportProfileModel();
					PrepareExportProfileModel(profileModel, x, providers.FirstOrDefault(y => y.Metadata.SystemName == x.ProviderSystemName));

					return profileModel;
				});
			}

			return new JsonResult {	Data = model };
		}

		public ActionResult Create()
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
			{
				var model = new ExportProfileModel();

				model.AvailableExportProviders = _exportService.LoadAllExportProviders()
					.Select(x =>
					{
						var item = new SelectListItem
						{
							Text = x.Metadata.FriendlyName,
							Value = x.Metadata.SystemName
						};

						return item;
					}).ToList();

				return PartialView(model);
			}

			return Content(T("Admin.AccessDenied.Description"));
		}

		[HttpPost]
		public ActionResult Create(ExportProfileModel model)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			if (model.ProviderSystemName.HasValue())
			{
				var provider = _exportService.LoadProvider(model.ProviderSystemName);
				if (provider != null)
				{
					var profile = _exportService.InsertExportProfile(provider);

					PrepareExportProfileModel(model, profile, provider);

					return RedirectToAction("Edit", new { id = profile.Id });
				}
			}

			NotifyError(T("Admin.Configuration.Export.ProviderSystemName.Validate", model.ProviderSystemName.NaIfEmpty()));

			return RedirectToAction("List");
		}

		public ActionResult Edit(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			var model = new ExportProfileModel();

			PrepareExportProfileModel(model, profile, provider);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult Edit(ExportProfileModel model, bool continueEditing)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(model.Id);
			if (profile == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				var partition = new ExportPartition
				{
					Offset = model.Offset,
					Limit = model.Limit,
					BatchSize = model.BatchSize,
					PerStore = model.PerStore
				};

				profile.Name = model.Name;
				profile.Enabled = model.Enabled;
				profile.ScheduleTask.Seconds = model.SchedulingHours * 3600;
				profile.FileType = model.FileType;
				profile.Partitioning = XmlHelper.Serialize<ExportPartition>(partition);

				_exportService.UpdateExportProfile(profile);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

				return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
			}

			PrepareExportProfileModel(model, profile, _exportService.LoadProvider(model.ProviderSystemName));

			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			try
			{
				_exportService.DeleteExportProfile(profile);

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

				return RedirectToAction("List");
			}
			catch (Exception exc)
			{
				NotifyError(exc);
			}

			return RedirectToAction("Edit", new { id = profile.Id });
		}
	}
}