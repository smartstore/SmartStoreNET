using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Configuration;
using SmartStore.Core.Themes;
using SmartStore.Services.Security;
using SmartStore.Services.Themes;

namespace SmartStore.Web.Controllers
{
	[AdminAuthorize]
    public partial class ThemeController : SmartController
	{
		#region Fields

        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;

	    #endregion

		#region Constructors

        public ThemeController(IThemeRegistry themeRegistry, IThemeVariablesService themeVarService)
		{
            //this._permissionService = permissionService;
            this._themeRegistry = themeRegistry;
            this._themeVarService = themeVarService;
		}

		#endregion 

        #region Methods

        [ChildActionOnly]
        public ActionResult ConfigureTheme(string theme, string selectedTab)
        {
            if (theme.HasValue())
            {
                this.ControllerContext.RouteData.DataTokens["ThemeOverride"] = theme;
            }

            var model = _themeVarService.GetThemeVariables(theme);

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }

        #endregion
    }
}
