using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string VariantAttributeValueName<T>(this HtmlHelper<T> helper)
        {
            string result =
                "<i class='<#= TypeNameClass #>' title='<#= TypeName #>'></i>" +
                "<# if(Color && Color.length > 0) {#>" +
                "<span class=\"color-container\"><span class=\"color\" style=\"background:<#= Color #>\">&nbsp;</span></span>" +
                "<span><#= NameString #><#= QuantityInfo #></span>" +
                "<# } else { #>" +
                "<span><#= NameString #><#= QuantityInfo #></span>" +
                "<# } #>";

            return result;
        }

        public static string AttributeOptionName<T>(this HtmlHelper<T> helper)
        {
            var result =
                "<# if(Color && Color.length > 0) {#>" +
                "<span class=\"color-container\"><span class=\"color\" style=\"background:<#= Color #>\">&nbsp;</span></span>" +
                "<span><#= NameString #></span>" +
                "<# } else { #>" +
                "<span><#= NameString #></span>" +
                "<# } #>";

            return result;
        }

        public static MvcHtmlString ProviderList<TModel>(this HtmlHelper<IEnumerable<TModel>> html,
            IEnumerable<TModel> model,
            params Func<TModel, object>[] extraColumns) where TModel : ProviderModel
        {
            var list = new ProviderModelList<TModel>();
            list.SetData(model);
            list.SetColumns(extraColumns);

            return html.Partial("_Providers", list);
        }
    }
}