using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotLiquid;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Utilities;

namespace SmartStore.Templating.Liquid
{
    public static class AdditionalFilters
    {
        private static LiquidTemplateEngine GetEngine()
        {
            return (LiquidTemplateEngine)Template.FileSystem;
        }

        #region Common Filters

        public static object Default(object input, object value)
        {
            return input ?? value;
        }

        public static string Json(object input)
        {
            if (input == null)
                return null;

            return JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = SmartContractResolver.Instance,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public static string FormatAddress(Context context, object input)
        {
            if (input == null)
                return null;

            var engine = GetEngine();
            var countryService = engine.Services.Resolve<ICountryService>();
            var addressService = engine.Services.Resolve<IAddressService>();

            Country country = null;

            // We know that we converted Address entity to a dictionary.

            if (input is IDictionary<string, object> dict)
            {
                country = countryService.GetCountryById(dict.Get("CountryId").Convert<int?>().GetValueOrDefault());
            }
            else if (input is IIndexable lq)
            {
                country = countryService.GetCountryById(lq["CountryId"].Convert<int?>().GetValueOrDefault());
            }

            return addressService.FormatAddress(input, country?.AddressFormat, context.FormatProvider);
        }

        #endregion

        #region Array Filters

        public static IEnumerable Uniq(IEnumerable input)
        {
            if (input == null)
                return null;

            return input.Cast<object>().Distinct();
        }

        public static IEnumerable Compact(IEnumerable input)
        {
            if (input == null)
                return null;

            return input.Cast<object>().Where(x => x != null);
        }

        public static IEnumerable Reverse(IEnumerable input)
        {
            if (input == null)
                return null;

            return input.Cast<object>().Reverse();
        }

        #endregion

        #region Number Filters

        public static object Ceil(object input)
        {
            if (input == null)
                return null;

            return CommonHelper.TryConvert<double>(input, out var d)
                ? Math.Ceiling(d)
                : 0;
        }

        public static object Floor(object input)
        {
            if (input == null)
                return null;

            return CommonHelper.TryConvert<double>(input, out var d)
                ? Math.Floor(d)
                : 0;
        }

        public static object Abs(object input)
        {
            if (input == null)
                return null;

            return CommonHelper.TryConvert<double>(input, out var d)
                ? Math.Abs(d)
                : 0;
        }

        public static object AtMost(object input, object operand)
        {
            if (input == null || operand == null)
                return null;

            return CommonHelper.TryConvert<double>(input, out var d) && CommonHelper.TryConvert<double>(operand, out var o)
                ? Math.Min(d, o)
                : 0;
        }

        public static object AtLeast(object input, object operand)
        {
            if (input == null || operand == null)
                return null;

            return CommonHelper.TryConvert<double>(input, out var d) && CommonHelper.TryConvert<double>(operand, out var o)
                ? Math.Max(d, o)
                : 0;
        }

        #endregion

        #region String Filters

        public static string Prettify(object input, bool allowSpace)
        {
            if (CommonHelper.TryConvert<long>(input, out var l))
            {
                return Prettifier.BytesToString(l);
            }
            else if (input is string s)
            {
                return s.Slugify(allowSpace);
            }

            return null;
        }

        public static string SanitizeHtmlId(string input)
        {
            return input?.SanitizeHtmlId();
        }

        public static string Md5(string input)
        {
            return input?.Hash(Encoding.UTF8);
        }

        public static string UrlDecode(string input)
        {
            return input?.UrlDecode();
        }

        public static string Handleize(string input)
        {
            return Inflector.Handleize(input.EmptyNull());
        }

        public static string Pluralize(string input)
        {
            return Inflector.Pluralize(input.EmptyNull());
        }

        #endregion

        #region Localization Filters

        public static string T(Context context, string key, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null)
        {
            var engine = GetEngine();

            int languageId = 0;

            if (context["Context.LanguageId"] is int lid)
            {
                languageId = lid;
            }

            var args = (new object[] { arg1, arg2, arg3, arg4 }).ToArray();

            return engine.T(key, languageId, args);
        }

        #endregion

        #region Url Filters

        #endregion

        #region Html Filters

        public static string ScriptTag(string input)
        {
            return String.Format("<script src=\"{0}\"></script>", input);
        }

        public static string StylesheetTag(string input)
        {
            return String.Format("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" media=\"all\" />", input);
        }

        public static string ImgTag(string input, string alt = "", string css = "")
        {
            return input == null ? null : GetImageTag(input, alt, css);
        }

        private static string GetImageTag(string src, string alt, string css)
        {
            return String.Format("<img src=\"{0}\" alt=\"{1}\" class=\"{2}\" />", src, alt, css);
        }

        #endregion
    }
}
