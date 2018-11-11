using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Theming
{
	public class ThemeableRazorViewEngine : ThemeableVirtualPathProviderViewEngine
	{
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public ThemeableRazorViewEngine()
		{
			var areaBasePathsSetting = CommonHelper.GetAppSetting<string>("sm:AreaBasePaths", "~/Plugins/");
			var areaBasePaths = areaBasePathsSetting.Split(',').Select(x => x.Trim().EnsureEndsWith("/")).ToArray();

			// 0: view, 1: controller, 2: area
			var areaFormats = new string[] { "{2}/Views/{1}/{0}", "{2}/Views/Shared/{0}" };
			var areaViewLocationFormats = ExpandLocationFormats(areaBasePaths.SelectMany(x => areaFormats.Select(f => x + f)));

			AreaViewLocationFormats = areaViewLocationFormats.ToArray();
			AreaMasterLocationFormats = areaViewLocationFormats.ToArray();
			AreaPartialViewLocationFormats = areaViewLocationFormats.ToArray();

            // 0: view, 1: controller, 2: theme
            var locationFormats = ExpandLocationFormats(new[]
            {
                "~/Themes/{2}/Views/{1}/{0}",
                "~/Views/{1}/{0}",
                "~/Themes/{2}/Views/Shared/{0}",
                "~/Views/Shared/{0}"
            });

            ViewLocationFormats = locationFormats.ToArray();
            MasterLocationFormats = locationFormats.ToArray();
            PartialViewLocationFormats = locationFormats.ToArray();

            if (EnableVbViews)
            {
                FileExtensions = new[] { "cshtml", "vbhtml" };
            }
            else
            {
                FileExtensions = new[] { "cshtml" };
            }
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
		{
			return new RazorView(controllerContext, partialPath, null, false, base.FileExtensions, base.ViewPageActivator);
		}

		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
		{
			return new RazorView(controllerContext, viewPath, masterPath, true, base.FileExtensions, base.ViewPageActivator);
		}
	}
}
