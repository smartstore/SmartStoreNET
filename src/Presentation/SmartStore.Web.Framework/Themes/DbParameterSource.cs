using System.Collections.Generic;
using dotless.Core.Parameters;
using SmartStore.Core;

// codehint: sm-add (whole file)

namespace SmartStore.Web.Framework.Themes
{

    public class DbParameterSource : IParameterSource
    {
        private readonly IThemeContext _themeContext;
		private readonly IStoreContext _storeContext;

		public DbParameterSource(IThemeContext themeContext, IStoreContext storeContext)
        {
            this._themeContext = themeContext;
			this._storeContext = storeContext;
        }

        public IDictionary<string, string> GetParameters()
        {
            string themeName = _themeContext.WorkingDesktopTheme;
            if (themeName.IsEmpty())
            {
                return new Dictionary<string, string>();
            }

            var repo = new ThemeVarsRepository();

            return repo.GetLessCssVariables(themeName, _storeContext.CurrentStore.Id);
        }

    }
}