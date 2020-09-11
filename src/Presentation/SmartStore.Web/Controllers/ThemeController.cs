using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Themes;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Controllers
{
    [AdminAuthorize]
    public partial class ThemeController : PublicControllerBase
    {
        #region Fields

        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;
        private readonly IThemeContext _themeContext;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Constructors

        public ThemeController(
            IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
            IThemeContext themeContext,
            IStoreContext storeContext)
        {
            this._themeRegistry = themeRegistry;
            this._themeVarService = themeVarService;
            this._themeContext = themeContext;
            this._storeContext = storeContext;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult ConfigureTheme(string theme, int storeId)
        {
            if (theme.HasValue())
            {
                _themeContext.SetRequestTheme(theme);
            }

            if (storeId > 0)
            {
                _storeContext.SetRequestStore(storeId);
            }

            var model = TempData["OverriddenThemeVars"] ?? _themeVarService.GetThemeVariables(theme, storeId);

            return View(model);
        }

        #endregion
    }
}
