using System.Web.Mvc;

namespace SmartStore.Web.Framework.Themes
{
	public class ThemeableRazorViewEngine : ThemeableVirtualPathProviderViewEngine
    {

		public ThemeableRazorViewEngine()
        {
            // 0: view, 1: controller, 2: area, 3: theme
            AreaViewLocationFormats = new[]
                                          {
                                              //themes
                                              "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml", 
                                              "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.vbhtml", 
                                              "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml", 
                                              "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.vbhtml",
                                              
                                              //default
                                              "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                                              "~/Areas/{2}/Views/{1}/{0}.vbhtml", 
                                              "~/Areas/{2}/Views/Shared/{0}.cshtml", 
                                              "~/Areas/{2}/Views/Shared/{0}.vbhtml"
                                          };

            // 0: view, 1: controller, 2: area, 3: theme
            AreaMasterLocationFormats = new[]
                                            {
                                                //themes
                                                "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml", 
                                                "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.vbhtml", 
                                                "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml", 
                                                "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.vbhtml",


                                                //default
                                                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                                                "~/Areas/{2}/Views/{1}/{0}.vbhtml", 
                                                "~/Areas/{2}/Views/Shared/{0}.cshtml", 
                                                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
                                            };

            // 0: view, 1: controller, 2: area, 3: theme
            AreaPartialViewLocationFormats = new[]
                                                 {
                                                     //themes
                                                    "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml", 
                                                    "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.vbhtml", 
                                                    "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml", 
                                                    "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.vbhtml",
                                                    
                                                    //default
                                                    "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                                                    "~/Areas/{2}/Views/{1}/{0}.vbhtml", 
                                                    "~/Areas/{2}/Views/Shared/{0}.cshtml", 
                                                    "~/Areas/{2}/Views/Shared/{0}.vbhtml"
                                                 };

            // 0: view, 1: controller, 2: theme
            ViewLocationFormats = new[]
                                      {
                                            //themes
                                            "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                                            "~/Themes/{2}/Views/{1}/{0}.vbhtml", 
                                            "~/Themes/{2}/Views/Shared/{0}.cshtml",
                                            "~/Themes/{2}/Views/Shared/{0}.vbhtml",

                                            //default
                                            "~/Views/{1}/{0}.cshtml", 
                                            "~/Views/{1}/{0}.vbhtml", 
                                            "~/Views/Shared/{0}.cshtml",
                                            "~/Views/Shared/{0}.vbhtml",
                                      };

            // 0: view, 1: controller, 2: theme
            MasterLocationFormats = new[]
                                        {
                                            //themes
                                            "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                                            "~/Themes/{2}/Views/{1}/{0}.vbhtml", 
                                            "~/Themes/{2}/Views/Shared/{0}.cshtml", 
                                            "~/Themes/{2}/Views/Shared/{0}.vbhtml",

                                            //default
                                            "~/Views/{1}/{0}.cshtml", 
                                            "~/Views/{1}/{0}.vbhtml", 
                                            "~/Views/Shared/{0}.cshtml", 
                                            "~/Views/Shared/{0}.vbhtml"
                                        };

            // 0: view, 1: controller, 2: theme
            PartialViewLocationFormats = new[]
                                             {
                                                 //themes
                                                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                                                "~/Themes/{2}/Views/{1}/{0}.vbhtml", 
                                                "~/Themes/{2}/Views/Shared/{0}.cshtml", 
                                                "~/Themes/{2}/Views/Shared/{0}.vbhtml",

                                                //default
                                                "~/Views/{1}/{0}.cshtml", 
                                                "~/Views/{1}/{0}.vbhtml", 
                                                "~/Views/Shared/{0}.cshtml", 
                                                "~/Views/Shared/{0}.vbhtml",
                                             };

            FileExtensions = new[] { "cshtml", "vbhtml" };
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
