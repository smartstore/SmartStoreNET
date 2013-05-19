using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;

using Telerik.Web.Mvc.UI.Fluent;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.UI
{
    /// <summary>
    /// <remarks>codehint: sm-add</remarks>
    /// </summary>
    public static class ScaffoldExtensions
    {

        public static string SymbolForBool<T>(this HtmlHelper<T> helper, string boolFieldName)
        {
            return "<i class='icon-active-<#= {0} #>'></i>".FormatInvariant(boolFieldName);
        }

        public static HelperResult SymbolForBool<T>(this HtmlHelper<T> helper, bool value)
        {
            return new HelperResult(writer => writer.Write("<i class='icon-active-{0}'></i>".FormatInvariant(value.ToString().ToLower())));
        }

        public static GridEditActionCommandBuilder Localize(this GridEditActionCommandBuilder builder, Localizer T)
        {
            return builder.Text(T("Admin.Common.Edit").Text)
                          .UpdateText(T("Admin.Common.Save").Text)
                          .CancelText(T("Admin.Common.Cancel").Text)
                          .InsertText(T("Admin.Telerik.GridLocalization.Insert").Text);
        }

        public static GridDeleteActionCommandBuilder Localize(this GridDeleteActionCommandBuilder builder, Localizer T)
        {
            return builder.Text(T("Admin.Common.Delete").Text);
        }

    }
}

