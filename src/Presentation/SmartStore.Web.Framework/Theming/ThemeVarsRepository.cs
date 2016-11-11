using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Themes;
using System.Web;
using System.IO;

namespace SmartStore.Web.Framework.Theming
{ 
    internal class ThemeVarsRepository
    {
        private static readonly Regex s_keyWhitelist = new Regex(@"^[a-zA-Z0-9_-]+$");
        private static readonly Regex s_valueWhitelist = new Regex(@"^[#@]?[a-zA-Z0-9""' _\.,-]*$");

		public string GetPreprocessorCss(string extension, string themeName, int storeId)
        {
            Guard.NotEmpty(themeName, nameof(themeName));
            Guard.IsPositive(storeId, nameof(storeId));

            var variables = GetVariables(themeName, storeId);

			var isLess = extension.IsCaseInsensitiveEqual(".less");
            var css = Transform(variables, isLess);
            return css;
        }

        private IDictionary<string, string> GetVariables(string themeName, int storeId)
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

        internal virtual ExpandoObject GetRawVariables(string themeName, int storeId)
        {
			// we need the Asp.Net cache here in order to define cacheKey dependencies

			string cacheKey = FrameworkCacheConsumer.BuildThemeVarsCacheKey(themeName, storeId);

			return HttpRuntime.Cache.GetOrAdd(cacheKey, () => 
			{
				var themeVarService = EngineContext.Current.Resolve<IThemeVariablesService>();
				return themeVarService.GetThemeVariables(themeName, storeId) ?? new ExpandoObject();
			});
        }

		private string Transform(IDictionary<string, string> parameters, bool toLess)
		{
			if (parameters.Count == 0)
				return string.Empty;

			var prefix = toLess ? "@" : "$";

			var sb = new StringBuilder();
			foreach (var parameter in parameters.Where(kvp => kvp.Value.HasValue()))
			{
				sb.AppendFormat("{0}{1}: {2};\n", prefix, parameter.Key, parameter.Value);
			}

			return sb.ToString();
		}
	}

}
