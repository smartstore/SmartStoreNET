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
			var areaFormats = new[] 
			{
				"{2}/Views/{1}/{0}",
				"{2}/Views/Shared/{0}"
			};
			var areaLocationFormats = areaBasePaths.SelectMany(x => areaFormats.Select(f => x + f));

			AreaViewLocationFormats = ExpandLocationFormats(areaLocationFormats, ViewType.Layout).ToArray();
			AreaMasterLocationFormats = ExpandLocationFormats(areaLocationFormats, ViewType.Layout).ToArray();
			AreaPartialViewLocationFormats = ExpandLocationFormats(areaLocationFormats, ViewType.Partial).ToArray();

			// 0: view, 1: controller, 2: theme
			var locationFormats = new[]
            {
				"~/Themes/{2}/Views/{1}/{0}",
				"~/Views/{1}/{0}",
				"~/Themes/{2}/Views/Shared/{0}",
				"~/Views/Shared/{0}"
			};

            ViewLocationFormats = ExpandLocationFormats(locationFormats, ViewType.Layout).ToArray();
            MasterLocationFormats = ExpandLocationFormats(locationFormats, ViewType.Layout).ToArray();
			PartialViewLocationFormats = ExpandLocationFormats(locationFormats, ViewType.Partial).ToArray();

			FileExtensions = EnableVbViews
				? new[] { "cshtml", "vbhtml" }
				: new[] { "cshtml" };
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
