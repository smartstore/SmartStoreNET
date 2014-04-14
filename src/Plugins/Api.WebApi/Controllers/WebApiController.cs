using SmartStore.Core.Domain.Common;
using SmartStore.Plugin.Api.WebApi.Models;
using SmartStore.Plugin.Api.WebApi.Security;
using SmartStore.Plugin.Api.WebApi.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.WebApi;
using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Api.WebApi.Controllers
{
	public class WebApiController : PluginControllerBase
	{
		private readonly IPermissionService _permissionService;
		private readonly WebApiSettings _webApiSettings;
		private readonly ISettingService _settingService;
		private readonly IWebApiPluginService _webApiPluginService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly ILocalizationService _localizationService;

		public WebApiController(
			IPermissionService permissionService,
			WebApiSettings settings,
			ISettingService settingService,
			IWebApiPluginService webApiPluginService,
			AdminAreaSettings adminAreaSettings,
			ILocalizationService localizationService)
		{
			_permissionService = permissionService;
			_webApiSettings = settings;
			_settingService = settingService;
			_webApiPluginService = webApiPluginService;
			_adminAreaSettings = adminAreaSettings;
			_localizationService = localizationService;
		}

		private bool HasPermission(bool notify = true)
		{
			bool hasPermission = _permissionService.Authorize(WebApiPermissionProvider.ManageWebApi);

			if (notify && !hasPermission)
				NotifyError(_localizationService.GetResource("Admin.AccessDenied.Description"));

			return hasPermission;
		}
		private void AddButtonText()
		{
			ViewData["ButtonTextEnable"] = _localizationService.GetResource("Plugins.Api.WebApi.Activate");
            ViewData["ButtonTextDisable"] = _localizationService.GetResource("Plugins.Api.WebApi.Deactivate");
            ViewData["ButtonTextRemoveKeys"] = _localizationService.GetResource("Plugins.Api.WebApi.RemoveKeys");
            ViewData["ButtonTextCreateKeys"] = _localizationService.GetResource("Plugins.Api.WebApi.CreateKeys");
		}

		public ActionResult Configure()
		{
			if (!HasPermission(false))
				return AccessDeniedPartialView();

			var model = new WebApiConfigModel();
			model.Copy(_webApiSettings, true);
			
			var odataUri = new Uri(Request.Url,
				WebApiGlobal.MostRecentOdataPath.StartsWith("/") ? WebApiGlobal.MostRecentOdataPath : "/" + WebApiGlobal.MostRecentOdataPath
			);

			model.ApiOdataUrl = odataUri.AbsoluteUri.EnsureEndsWith("/");
			model.ApiOdataMetadataUrl = model.ApiOdataUrl + "$metadata";

			model.GridPageSize = _adminAreaSettings.GridPageSize;

			AddButtonText();

			return View("SmartStore.Plugin.Api.WebApi.Views.WebApi.Configure", model);
		}

		[HttpPost, ActionName("Configure")]
		[FormValueRequired("savegeneralsettings")]
		public ActionResult SaveGeneralSettings(WebApiConfigModel model)
		{
			if (!HasPermission(false) || !ModelState.IsValid)
				return Configure();

			model.Copy(_webApiSettings, false);
			_settingService.SaveSetting(_webApiSettings);

			WebApiCaching.Remove(WebApiControllingCacheData.Key);

			return Configure();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult GridUserData(GridCommand command)
		{
			var model = _webApiPluginService.GetGridModel(command.Page - 1, command.PageSize);

			AddButtonText();

			return new JsonResult { Data = model };
		}

		[HttpPost]
		public void ApiButtonCreateKeys(int customerId)
		{
			_webApiPluginService.CreateKeys(customerId);
		}

		[HttpPost]
		public void ApiButtonRemoveKeys(int customerId)
		{
			_webApiPluginService.RemoveKeys(customerId);
		}

		[HttpPost]
		public void ApiButtonEnable(int customerId)
		{
			_webApiPluginService.EnableOrDisableUser(customerId, true);
		}

		[HttpPost]
		public void ApiButtonDisable(int customerId)
		{
			_webApiPluginService.EnableOrDisableUser(customerId, false);
		}

	}
}
