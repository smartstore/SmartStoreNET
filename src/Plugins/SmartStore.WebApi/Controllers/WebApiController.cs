using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Caching;
using SmartStore.WebApi.Models;
using SmartStore.WebApi.Security;
using SmartStore.WebApi.Services;
using Telerik.Web.Mvc;

namespace SmartStore.WebApi.Controllers
{
	[AdminAuthorize]
	public class WebApiController : PluginControllerBase
	{
		private readonly WebApiSettings _webApiSettings;
		private readonly IWebApiPluginService _webApiPluginService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly ICommonServices _services;

		public WebApiController(
			WebApiSettings settings,
			IWebApiPluginService webApiPluginService,
			AdminAreaSettings adminAreaSettings,
			ICommonServices services)
		{
			_webApiSettings = settings;
			_webApiPluginService = webApiPluginService;
			_adminAreaSettings = adminAreaSettings;
			_services = services;
		}

		private bool HasPermission(bool notify = true)
		{
			bool hasPermission = _services.Permissions.Authorize(WebApiPermissionProvider.ManageWebApi);

			if (notify && !hasPermission)
				NotifyError(_services.Localization.GetResource("Admin.AccessDenied.Description"));

			return hasPermission;
		}

		private void AddButtonText()
		{
			ViewData["ButtonTextEnable"] = _services.Localization.GetResource("Plugins.Api.WebApi.Activate");
			ViewData["ButtonTextDisable"] = _services.Localization.GetResource("Plugins.Api.WebApi.Deactivate");
			ViewData["ButtonTextRemoveKeys"] = _services.Localization.GetResource("Plugins.Api.WebApi.RemoveKeys");
			ViewData["ButtonTextCreateKeys"] = _services.Localization.GetResource("Plugins.Api.WebApi.CreateKeys");
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

			return View(model);
		}

		[HttpPost, ActionName("Configure")]
		[FormValueRequired("savegeneralsettings")]
		public ActionResult SaveGeneralSettings(WebApiConfigModel model)
		{
			if (!ModelState.IsValid)
				return Configure();

			if (!HasPermission(false))
				return AccessDeniedPartialView();

			model.Copy(_webApiSettings, false);
			_services.Settings.SaveSetting(_webApiSettings);

			WebApiCachingControllingData.Remove();

			return Configure();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult GridUserData(GridCommand command)
		{
			if (!HasPermission())
				return new JsonResult { Data = new GridModel<WebApiUserModel> { Data = new List<WebApiUserModel>() } };

			var model = _webApiPluginService.GetGridModel(command.Page - 1, command.PageSize);

			AddButtonText();

			return new JsonResult { Data = model };
		}

		[HttpPost]
		public void ApiButtonCreateKeys(int customerId)
		{
			if (HasPermission())
				_webApiPluginService.CreateKeys(customerId);
		}

		[HttpPost]
		public void ApiButtonRemoveKeys(int customerId)
		{
			if (HasPermission())
				_webApiPluginService.RemoveKeys(customerId);
		}

		[HttpPost]
		public void ApiButtonEnable(int customerId)
		{
			if (HasPermission())
				_webApiPluginService.EnableOrDisableUser(customerId, true);
		}

		[HttpPost]
		public void ApiButtonDisable(int customerId)
		{
			if (HasPermission())
				_webApiPluginService.EnableOrDisableUser(customerId, false);
		}

	}
}
