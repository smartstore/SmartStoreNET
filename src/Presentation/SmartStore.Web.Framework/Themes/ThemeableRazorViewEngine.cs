using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Themes
{
	public class ThemeableRazorViewEngine : ThemeableVirtualPathProviderViewEngine
	{

		public ThemeableRazorViewEngine()
		{
			var areaBasePathsSetting = CommonHelper.GetAppSetting<string>("sm:AreaBasePaths", "~/Plugins/");
			var areaBasePaths = areaBasePathsSetting.Split(',').Select(x => x.Trim().EnsureEndsWith("/")).ToArray();

			// 0: view, 1: controller, 2: area
			var areaFormats = new string[] { "{2}/Views/{1}/{0}.cshtml", "{2}/Views/Shared/{0}.cshtml" };
			var areaViewLocationFormats = areaBasePaths.SelectMany(x => areaFormats.Select(f => x + f));

			AreaViewLocationFormats = areaViewLocationFormats.ToArray();
			AreaMasterLocationFormats = areaViewLocationFormats.ToArray();
			AreaPartialViewLocationFormats = areaViewLocationFormats.ToArray();

			// 0: view, 1: controller, 2: theme
			ViewLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml",
            };

			// 0: view, 1: controller, 2: theme
			MasterLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                "~/Themes/{2}/Views/Shared/{0}.cshtml", 

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml"
            };

			// 0: view, 1: controller, 2: theme
			PartialViewLocationFormats = new[]
            {
				//themes
				"~/Themes/{2}/Views/{1}/{0}.cshtml", 
				"~/Themes/{2}/Views/Shared/{0}.cshtml", 

				//default
				"~/Views/{1}/{0}.cshtml",  
				"~/Views/Shared/{0}.cshtml", 
            };

			FileExtensions = new[] { "cshtml" };
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
		{
			string layoutPath = null;
			var runViewStartPages = false;
			var fileExtensions = base.FileExtensions;
			//return new RazorView(controllerContext, partialPath, layoutPath, runViewStartPages, fileExtensions);
			return new RazorView(controllerContext, partialPath, layoutPath, runViewStartPages, fileExtensions, base.ViewPageActivator);
		}

		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
		{
			string layoutPath = masterPath;
			var runViewStartPages = true;
			var fileExtensions = base.FileExtensions;
			return new RazorView(controllerContext, viewPath, layoutPath, runViewStartPages, fileExtensions, base.ViewPageActivator);
		}
	}
}
