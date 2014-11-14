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
    public partial class ThemeController : PublicControllerBase
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
        public ActionResult ConfigureTheme(string theme, int storeId)
        {
            if (theme.HasValue())
            {
				this.Request.SetThemeOverride(theme);
				this.Request.SetStoreOverride(storeId);
            }

            var model = TempData["OverriddenThemeVars"] ?? _themeVarService.GetThemeVariables(theme, storeId);

            return View(model);
        }

        #endregion
    }
}
