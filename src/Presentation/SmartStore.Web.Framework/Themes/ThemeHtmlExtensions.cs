using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Services.Common;
using SmartStore.Utilities;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Themes
{
    
    public static class ThemeHtmlExtensions
    {

        #region ThemeVars

        public static MvcHtmlString ThemeVarLabel(this HtmlHelper html, ThemeVariableInfo info)
        {
            Guard.ArgumentNotNull(info, "info");

            var result = new StringBuilder();
            var resKey = "ThemeVar.{0}.{1}".FormatInvariant(info.Manifest.ThemeName, info.Name);
            var langId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
            var locService = EngineContext.Current.Resolve<ILocalizationService>();

            var displayName = locService.GetResource(resKey, langId, false, Inflector.Titleize(info.Name));

            var hint = locService.GetResource(resKey + ".Hint", langId, false, "", true);
            hint = "@(var_){0}{1}".FormatInvariant(info.Name, hint.HasValue() ? "\n" + hint : "");

            result.Append("<div class='ctl-label'>");
            result.Append(html.Label(html.NameForThemeVar(info), displayName));
            result.Append(html.Hint(hint).ToHtmlString());
            result.Append("</div>");

            return MvcHtmlString.Create(result.ToString());
        }
         
        public static MvcHtmlString ThemeVarEditor(this HtmlHelper html, ThemeVariableInfo info, object value)
        {
            Guard.ArgumentNotNull(info, "info");

            string expression = html.NameForThemeVar(info);

            if (info.Type == ThemeVariableType.Color)
            {
                return html.ColorBox(expression, value.ToString());
            }
            else if (info.Type == ThemeVariableType.Boolean)
            {
                return html.CheckBox(expression, value.ToString().ToBool());
            }
            else if (info.Type == ThemeVariableType.Select)
            {
                return ThemeVarSelectEditor(html, info, expression, value.ToString());
            }

            return html.TextBox(expression, value);
        }

        private static MvcHtmlString ThemeVarSelectEditor(HtmlHelper html, ThemeVariableInfo info, string expression, string value)
        {
            var manifest = info.Manifest; 

            if (!manifest.Selects.ContainsKey(info.SelectRef))
            {
                throw new SmartException("A select list with id '{0}' was not specified. Please specify a 'Select' element with at least one 'Option' child.", info.SelectRef);
            }
            
            var selectList = from x in manifest.Selects[info.SelectRef]
                             select new SelectListItem 
                             { 
                                 Value = x, 
                                 Text = x, // TODO: (MC) Localize
                                 Selected = x.IsCaseInsensitiveEqual(value) 
                             };
            
            return html.DropDownList(expression, selectList);
        }

        public static string NameForThemeVar(this HtmlHelper html, ThemeVariableInfo info)
        {
            return "values[{0}]".FormatInvariant(info.Name);
        }

        public static string IdForThemeVar(this HtmlHelper html, ThemeVariableInfo info)
        {
            return TagBuilder.CreateSanitizedId(html.NameForThemeVar(info));
        }

        #endregion


        #region Href & Path

        public static string ThemeAwareContent(this UrlHelper url, string path)
        {
            var themeContext = EngineContext.Current.Resolve<IThemeContext>();
            var currentTheme = themeContext.CurrentTheme;
            if (currentTheme == null)
            {
                return string.Empty;
            }

            return url.ThemeAwareContent(currentTheme, path);
        }

        public static string ThemeAwareContent(this UrlHelper url, ThemeManifest manifest, string path)
        {
            path = EnsurePath(path);
            
            string fullPath = Path.Combine(manifest.Path, path);
            if (File.Exists(fullPath)) 
            {
                return url.Content(manifest.Location + "/" + Path.Combine(manifest.ThemeName, path));
            }

            fullPath = url.RequestContext.HttpContext.Server.MapPath("~/" + path);
            if (File.Exists(fullPath))
            {
                return url.Content("~/" + path);
            }
            
            return string.Empty;
        }

        public static string ThemePath(this HtmlHelper html, string path)
        {
            var themeContext = EngineContext.Current.Resolve<IThemeContext>();
            var currentTheme = themeContext.CurrentTheme;
            if (currentTheme == null)
            {
                return string.Empty;
            }

            return html.ThemePath(currentTheme, path);
        }

        public static string ThemePath(this HtmlHelper html, ThemeManifest manifest, string path)
        {
            path = EnsurePath(path);
            return "{0}{1}/{2}".FormatCurrent(manifest.Location, manifest.ThemeName, path);
        }

        private static string EnsurePath(string path)
        {
            if (path.StartsWith("/"))
            {
                return path.Substring(1);
            }

            if (path.StartsWith("~/"))
            {
                return path.Substring(2);
            }

            return path;
        }

        #endregion

    }

}
