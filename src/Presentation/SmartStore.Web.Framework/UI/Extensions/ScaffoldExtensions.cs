using System;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services.Localization;
using Telerik.Web.Mvc.UI.Fluent;

namespace SmartStore.Web.Framework.UI
{
    public static class ScaffoldExtensions
    {
        public static string SymbolForBool<T>(this HtmlHelper<T> helper, string boolFieldName)
        {
            return "<i class='fa fa-fw icon-active-<#= {0} #>'></i>".FormatInvariant(boolFieldName);
        }

        public static HelperResult SymbolForBool<T>(this HtmlHelper<T> helper, bool value)
        {
            return new HelperResult(writer => writer.Write("<i class='fa fa-fw icon-active-{0}'></i>".FormatInvariant(value.ToString().ToLower())));
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

			string result = "<span class='badge badge-<#= {0} #> mr-1'><#= {1} #></span>{2}".FormatInvariant(typeLabelHint, typeName, namePart);
			
			return "<# if({0} && {0}.length > 0) {{ #>{1}<# }} #>".FormatInvariant(name, result);
		}

		public static HelperResult LabeledProductName<T>(this HtmlHelper<T> helper, int id, string name, string typeName, string typeLabelHint)
		{
			if (id == 0 && name.IsEmpty())
				return null;

			string namePart = null;

			if (id != 0)
			{
				string url = UrlHelper.GenerateContentUrl("~/Admin/Product/Edit/", helper.ViewContext.RequestContext.HttpContext);
				namePart = "<a href=\"{0}{1}\" title='{2}'>{2}</a>".FormatInvariant(url, id, helper.Encode(name));
			}
			else
			{
				namePart = "<span>{0}</span>".FormatInvariant(helper.Encode(name));
			}

			return new HelperResult(writer => writer.Write("<span class='badge badge-{0} mr-1'>{1}</span>{2}".FormatInvariant(typeLabelHint, typeName, namePart)));
		}

		public static string LabeledOrderNumber<T>(this HtmlHelper<T> helper)
		{
			var localize = EngineContext.Current.Resolve<ILocalizationService>();
			string url = UrlHelper.GenerateContentUrl("~/Admin/Order/Edit/", helper.ViewContext.RequestContext.HttpContext);

			string link = "<a href=\"{0}<#= Id #>\"><#= OrderNumber #></a>".FormatInvariant(url);

			string label = "<span class='badge badge-warning mr-1' title='{0}'>{1}</span>".FormatInvariant(
				localize.GetResource("Admin.Orders.Payments.NewIpn.Hint"),
				localize.GetResource("Admin.Orders.Payments.NewIpn"));

			return "<# if(HasNewPaymentNotification) {{ #>{0}<# }} #>{1}".FormatInvariant(label, link);
		}

		public static HelperResult LabeledCurrencyName<T>(this HtmlHelper<T> helper, int id, string name, bool isPrimaryStoreCurrency, bool isPrimaryExchangeRateCurrency)
		{
			var localize = EngineContext.Current.Resolve<ILocalizationService>();
			var sb = new StringBuilder();

			if (isPrimaryStoreCurrency)
			{
				sb.AppendFormat("<span class='badge badge-warning{0}'>{1}</span>",
					isPrimaryExchangeRateCurrency ? String.Empty : " mr-1",
					localize.GetResource("Admin.Configuration.Currencies.Fields.IsPrimaryStoreCurrency"));
			}

			if (isPrimaryExchangeRateCurrency)
			{
				sb.AppendFormat("<span class='badge badge-info mr-1'>{0}</span>", localize.GetResource("Admin.Configuration.Currencies.Fields.IsPrimaryExchangeRateCurrency"));
			}

			string url = UrlHelper.GenerateContentUrl("~/Admin/Currency/Edit/", helper.ViewContext.RequestContext.HttpContext);

			sb.AppendFormat("<a href=\"{0}{1}\" title=\"{2}\">{2}</a>", url, id, helper.Encode(name.NaIfEmpty()));

			return new HelperResult(writer => writer.Write(sb.ToString()));
		}

		[Obsolete]
        public static string RichEditorFlavor(this HtmlHelper helper)
        {
            return "Html";
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

