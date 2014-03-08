using System.Web.Mvc;
using System.Web.WebPages;
using Telerik.Web.Mvc.UI.Fluent;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Localization;

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

		public static string LabeledProductName<T>(this HtmlHelper<T> helper, string id, string name, string typeName = "ProductTypeName", string typeLabelHint = "ProductTypeLabelHint")
		{
			string namePart = null;

			if (id.HasValue())
			{
				string url = UrlHelper.GenerateContentUrl("~/Admin/Product/Edit/", helper.ViewContext.RequestContext.HttpContext);

				namePart = "<a href=\"{0}<#= {1} #>\"><#= {2} #></a>".FormatInvariant(url, id, name);
			}
			else
			{
				namePart = "<span><#= {0} #></span>".FormatInvariant(name);
			}

			string result = "<span class='label label-smnet label-<#= {0} #>'><#= {1} #></span>{2}".FormatInvariant(typeLabelHint, typeName, namePart);
			
			return "<# if({0} && {0}.length > 0) {{ #>{1}<# }} #>".FormatInvariant(name, result);
		}
		public static HelperResult LabeledProductName<T>(this HtmlHelper<T> helper, int id, string name, string typeName, string typeLabelHint)
		{
			if (id == 0 && name.IsNullOrEmpty())
				return null;

			string namePart = null;

			if (id != 0)
			{
				string url = UrlHelper.GenerateContentUrl("~/Admin/Product/Edit/", helper.ViewContext.RequestContext.HttpContext);

				namePart = "<a href=\"{0}{1}\">{2}</a>".FormatInvariant(url, id, helper.Encode(name));
			}
			else
			{
				namePart = "<span>{0}</span>".FormatInvariant(helper.Encode(name));
			}

			return new HelperResult(writer => writer.Write("<span class='label label-smnet label-{0}'>{1}</span>{2}".FormatInvariant(typeLabelHint, typeName, namePart)));
		}

        public static string RichEditorFlavor(this HtmlHelper helper)
        {
            return EngineContext.Current.Resolve<AdminAreaSettings>().RichEditorFlavor.NullEmpty() ?? "RichEditor";
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

