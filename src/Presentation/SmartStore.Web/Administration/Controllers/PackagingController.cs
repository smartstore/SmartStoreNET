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
        [ValidateAntiForgeryToken]
        public ActionResult UploadPackage(string returnUrl = "")
        {
            var isTheme = false;
            var success = false;
            string message = null;
            string tempFile = "";

            try
            {
                var file = Request.ToPostedFileResult();
                if (file != null)
                {
                    var requiredPermission = (isTheme = PackagingUtils.IsTheme(file.FileName))
                        ? Permissions.Configuration.Theme.Upload
                        : Permissions.Configuration.Plugin.Upload;

                    if (!Services.Permissions.Authorize(requiredPermission))
                    {
                        message = T("Admin.AccessDenied.Description");
                        return Json(new { success, file.FileName, message });
                    }

                    if (!file.FileExtension.IsCaseInsensitiveEqual(".nupkg"))
                    {
                        return Json(new { success, file.FileName, T("Admin.Packaging.NotAPackage").Text, returnUrl });
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
                    return Json(new { success, file.FileName, T("Admin.Common.UploadFile").Text, returnUrl });
                }

                if (!isTheme)
                {
                    message = T("Admin.Packaging.InstallSuccess").Text;
                    Services.WebHelper.RestartAppDomain();
                }
                else
                {
                    message = T("Admin.Packaging.InstallSuccess.Theme").Text;
                }

                success = true;

            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logger.Error(ex);
            }

            return Json(new { success, tempFile, message, returnUrl });
        }
    }
}