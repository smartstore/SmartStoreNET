using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Themes;

namespace SmartStore.Web.Framework.Theming
{
    internal class ThemeVarsRepository
    {
        private static readonly Regex s_keyWhitelist = new Regex(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        private static readonly Regex s_valueBlacklist = new Regex(@"[:;]+", RegexOptions.Compiled);
        private static readonly Regex s_valueSassVars = new Regex(@"[$][a-zA-Z0-9_-]+", RegexOptions.Compiled);
        private static readonly Regex s_valueLessVars = new Regex(@"[@][a-zA-Z0-9_-]+", RegexOptions.Compiled);
        //private static readonly Regex s_valueWhitelist = new Regex(@"^[#@]?[a-zA-Z0-9""' _\.,-]*$");

        const string SassVarPrefix = "$";

        public string GetPreprocessorCss(string extension, string themeName, int storeId)
        {
            Guard.NotEmpty(themeName, nameof(themeName));
            Guard.IsPositive(storeId, nameof(storeId));

            var variables = GetVariables(themeName, storeId);
            var css = Transform(variables);

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

                if (s_valueBlacklist.IsMatch(value))
                    continue;

                //if (!s_valueWhitelist.IsMatch(value))
                //    continue;

                result.Add(key, value);
            }

            return result;
        }

        internal virtual ExpandoObject GetRawVariables(string themeName, int storeId)
        {
            // we need the Asp.Net cache here in order to define cacheKey dependencies
            bool validationMode = ThemeHelper.IsStyleValidationRequest();

            if (validationMode)
            {
                // Return uncached fresh data (the variables is not nuked yet)
                return GetRawVariablesCore(themeName, storeId);
            }
            else
            {
                string cacheKey = FrameworkCacheConsumer.BuildThemeVarsCacheKey(themeName, storeId);
                return HttpRuntime.Cache.GetOrAdd(cacheKey, () =>
                {
                    return GetRawVariablesCore(themeName, storeId);
                });
            }
        }

        private ExpandoObject GetRawVariablesCore(string themeName, int storeId)
        {
            var themeVarService = EngineContext.Current.Resolve<IThemeVariablesService>();
            return themeVarService.GetThemeVariables(themeName, storeId) ?? new ExpandoObject();
        }

        private string Transform(IDictionary<string, string> parameters)
        {
            if (parameters.Count == 0)
                return string.Empty;

            var prefix = SassVarPrefix;

            var sb = new StringBuilder();
            foreach (var parameter in parameters.Where(kvp => kvp.Value.HasValue()))
            {
                sb.AppendFormat("{0}{1}: {2};\n", prefix, parameter.Key, parameter.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks whether the passed SASS value is a valid, displayable HTML color,
        /// e.g.: "lighten($red, 20%)" would return <c>false</c>.
        /// </summary>
        /// <param name="value">The SASS value to test</param>
        /// <remarks>
        /// We need this helper during theme configuration: we just can't render
        /// color pickers for color values containing var references or SASS functions.
        /// </remarks>
        internal static bool IsValidColor(string value)
        {
            if (value.IsEmpty())
            {
                return true;
            }

            if (s_valueSassVars.IsMatch(value))
            {
                return false;
            }

            if (value[0] == '#' || value.StartsWith("rgb(") || value.StartsWith("rgba(") || value.StartsWith("hsl(") || value.StartsWith("hsla("))
            {
                return true;
            }

            // Let pass all color names (red, blue etc.), but reject functions, e.g. "lighten(#fff, 10%)"
            return !value.Contains("(");
        }
    }

}
