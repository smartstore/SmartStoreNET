using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using dotless.Core.configuration;
using dotless.Core.Parameters;
using SmartStore.Core.Caching;
using SmartStore.Core;
using SmartStore.Services.Themes;

// codehint: sm-add (whole file)

namespace SmartStore.Web.Framework.Themes
{

    public class DbParameterSource : IParameterSource
    {
        private readonly IThemeContext _themeContext;

        public DbParameterSource(IThemeContext themeContext)
        {
            this._themeContext = themeContext;
        }

        public IDictionary<string, string> GetParameters()
        {
            string themeName = _themeContext.WorkingDesktopTheme;
            if (themeName.IsEmpty())
            {
                return new Dictionary<string, string>();
            }

            var repo = new ThemeVarsRepository();

            return repo.GetLessCssVariables(themeName);
        }

    }
}