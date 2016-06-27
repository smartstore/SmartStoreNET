using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Utilities;
using SmartStore.Core.Packaging;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core.Localization;
using System.IO;
using SmartStore.Services;
using SmartStore.Services.Security;
using System.Dynamic;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{

	[AdminAuthorize]
	public class PackagingController : AdminControllerBase
	{
		private readonly ICommonServices _services;
		private readonly IPackageManager _packageManager;
		private readonly Lazy<IThemeRegistry> _themeRegistry;

		public PackagingController(ICommonServices services, IPackageManager packageManager, Lazy<IThemeRegistry> themeRegistry)
		{
			this._services = services;
			this._packageManager = packageManager;
			this._themeRegistry = themeRegistry;
		}

		[ChildActionOnly]
		public ActionResult UploadPackage(bool isTheme)
		{
			var title = isTheme ? T("Admin.Packaging.UploadTheme").Text : T("Admin.Packaging.UploadPlugin").Text;
			var info = isTheme ? T("Admin.Packaging.Dialog.ThemeInfo").Text : T("Admin.Packaging.Dialog.PluginInfo").Text;

			var model = new { Title = title, Info = info };
			return PartialView(CommonHelper.ToExpando(model));
		}

		[HttpPost]
		public ActionResult UploadPackage(FormCollection form, string returnUrl = "")
		{
			bool isTheme = false;

			try
			{
				var file = Request.Files["packagefile"].ToPostedFileResult();
				if (file != null)
				{
					var requiredPermission = (isTheme = PackagingUtils.IsTheme(file.FileName))
						? StandardPermissionProvider.ManageThemes
						: StandardPermissionProvider.ManagePlugins;

					if (!_services.Permissions.Authorize(requiredPermission))
					{
						return AccessDeniedView();
					}

					if (!file.FileExtension.IsCaseInsensitiveEqual(".nupkg"))
					{
						NotifyError(T("Admin.Packaging.NotAPackage"));
						return RedirectToReferrer(returnUrl);
					}

					var location = CommonHelper.MapPath("~/App_Data");
					var appPath = CommonHelper.MapPath("~/");

					if (isTheme)
					{
						// avoid getting terrorized by IO events
						_themeRegistry.Value.StopMonitoring();
					}

					var packageInfo = _packageManager.Install(file.Stream, location, appPath);

					if (isTheme)
					{
						// create manifest
						if (packageInfo != null)
						{
							var manifest = ThemeManifest.Create(packageInfo.ExtensionDescriptor.Path);
							if (manifest != null)
							{
								_themeRegistry.Value.AddThemeManifest(manifest);
							}
						}

						// SOFT start IO events again
						_themeRegistry.Value.StartMonitoring(false);
					}
				}
				else
				{
					NotifyError(T("Admin.Common.UploadFile"));
					return RedirectToReferrer(returnUrl);
				}

				if (!isTheme)
				{
					_services.WebHelper.RestartAppDomain();
				}
				NotifySuccess(T("Admin.Packaging.InstallSuccess"));
				return RedirectToReferrer(returnUrl);
			}
			catch (Exception exc)
			{
				NotifyError(exc);
				Logger.Error(exc);
				return RedirectToReferrer(returnUrl);
			}
		}

	}

}