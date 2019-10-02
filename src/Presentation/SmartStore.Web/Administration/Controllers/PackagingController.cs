using System;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Security;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
	public class PackagingController : AdminControllerBase
	{
		private readonly IPackageManager _packageManager;
		private readonly Lazy<IThemeRegistry> _themeRegistry;

		public PackagingController(
            IPackageManager packageManager,
            Lazy<IThemeRegistry> themeRegistry)
		{
			_packageManager = packageManager;
			_themeRegistry = themeRegistry;
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
			var isTheme = false;

			try
			{
				var file = Request.Files["packagefile"].ToPostedFileResult();
				if (file != null)
				{
                    var requiredPermission = (isTheme = PackagingUtils.IsTheme(file.FileName))
                        ? Permissions.Configuration.Theme.Upload
                        : Permissions.Configuration.Plugin.Upload;

					if (!Services.Permissions.Authorize(requiredPermission))
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
						// Avoid getting terrorized by IO events.
						_themeRegistry.Value.StopMonitoring();
					}

					var packageInfo = _packageManager.Install(file.Stream, location, appPath);

					if (isTheme)
					{
						// Create manifest.
						if (packageInfo != null)
						{
							var manifest = ThemeManifest.Create(packageInfo.ExtensionDescriptor.Path);
							if (manifest != null)
							{
								_themeRegistry.Value.AddThemeManifest(manifest);
							}
						}

						// SOFT start IO events again.
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
					Services.WebHelper.RestartAppDomain();
				}

				NotifySuccess(T("Admin.Packaging.InstallSuccess"));
			}
			catch (Exception ex)
			{
				NotifyError(ex);
				Logger.Error(ex);
			}

            return RedirectToReferrer(returnUrl);
        }
	}
}