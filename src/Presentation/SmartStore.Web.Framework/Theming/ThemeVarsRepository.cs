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
        private static readonly Regex s_keyWhitelist = new Regex(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
		private static readonly Regex s_valueBlacklist = new Regex(@"[:;]+", RegexOptions.Compiled);
		private static readonly Regex s_valueSassVars = new Regex(@"[$][a-zA-Z0-9_-]+", RegexOptions.Compiled);
		private static readonly Regex s_valueLessVars = new Regex(@"[@][a-zA-Z0-9_-]+", RegexOptions.Compiled);
		//private static readonly Regex s_valueWhitelist = new Regex(@"^[#@]?[a-zA-Z0-9""' _\.,-]*$");

		const string LessVarPrefix = "@var_";
		const string SassVarPrefix = "$";

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

			var prefix = toLess ? LessVarPrefix : SassVarPrefix;

			var sb = new StringBuilder();
			foreach (var parameter in parameters.Where(kvp => kvp.Value.HasValue()))
			{
				var value = parameter.Value;
				if (toLess)
				{
					value = s_valueLessVars.Replace(value, match =>
					{
						// Replaces all occurences of @varname with @var_varname (in case of LESS).
						// The LESS compiler would throw exceptions otherwise, because the main variables file
						// is not loaded yet at this stage.
						var refVar = match.Value;
						if (!refVar.StartsWith(prefix))
						{
							refVar = "{0}{1}".FormatInvariant(prefix, refVar.Substring(1));
						}

						return refVar;
					});
				}

				sb.AppendFormat("{0}{1}: {2};\n", prefix, parameter.Key, value);
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
