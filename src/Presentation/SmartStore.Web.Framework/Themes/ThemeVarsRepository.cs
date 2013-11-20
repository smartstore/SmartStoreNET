using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using SmartStore.Core;
//using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Themes;

namespace SmartStore.Web.Framework.Themes
{
    
    internal class ThemeVarsRepository
    {
        private static readonly Regex s_keyWhitelist = new Regex(@"^[a-zA-Z0-9_-]+$");
        private static readonly Regex s_valueWhitelist = new Regex(@"^[#@]?[a-zA-Z0-9""' _\.,-]*$");

        public ThemeVarsRepository()
        {
        }

        public IDictionary<string, string> GetParameters(int storeId)
        {
            Guard.ArgumentIsPositive(storeId, "storeId");

            string themeName = EngineContext.Current.Resolve<IThemeContext>().WorkingDesktopTheme;
            if (themeName.IsEmpty())
            {
                return new Dictionary<string, string>();
            }

            return this.GetLessCssVariables(themeName, storeId);
        }

        private IDictionary<string, string> GetLessCssVariables(string themeName, int storeId)
        {
            var result = new Dictionary<string, string>();

			var rawVars = this.GetRawVariables(themeName, storeId);

            foreach (var v in rawVars)
            {
                string key = v.Key;

                if (v.Value == null || !s_keyWhitelist.IsMatch(key))
                    continue;

                string value = v.Value.ToString();

                if (!s_valueWhitelist.IsMatch(value))
                    continue;

                result.Add("var_" + key, value);
            }

            return result;
        }

        protected internal virtual ExpandoObject GetRawVariables(string themeName, int storeId)
        {
            var themeVarService = EngineContext.Current.Resolve<IThemeVariablesService>();
            var result = themeVarService.GetThemeVariables(themeName, storeId);

            if (result == null)
            {
                return new ExpandoObject();
            }

            return result;
        }

    }

}
