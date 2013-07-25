using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Themes;

namespace SmartStore.Web.Framework.Themes
{
    
    internal class ThemeVarsRepository
    {
        private static readonly Regex s_keyWhitelist = new Regex(@"^[a-zA-Z0-9_-]+$");
        private static readonly Regex s_valueWhitelist = new Regex(@"^[#@]?[a-zA-Z0-9""' _\.,-]*$");
        
        private readonly ICacheManager _cacheManager;

        public ThemeVarsRepository()
        {
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("sm_cache_static");
        }

        public ExpandoObject GetRawVariables(string themeName, int storeId)
        {
			Guard.ArgumentNotZero(storeId, "storeId");

            //var cacheKey = String.Format(FrameworkCacheConsumer.THEMEVARS_RAW_KEY, themeName, storeId);

            //return _cacheManager.Get(cacheKey, () =>
            //{
                var themeVarService = EngineContext.Current.Resolve<IThemeVariablesService>();
				var result = themeVarService.GetThemeVariables(themeName, storeId);

                if (result == null)
                {
                    return new ExpandoObject();
                }

                return result;
            //});
        }

        public IDictionary<string, string> GetLessCssVariables(string themeName, int storeId)
        {
			Guard.ArgumentNotZero(storeId, "storeId");

			var cacheKey = String.Format(FrameworkCacheConsumer.THEMEVARS_LESSCSS_KEY, themeName, storeId);

            return _cacheManager.Get(cacheKey, () =>
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
            });
        }

    }

}
