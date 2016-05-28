using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Core;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Extensions
{
	public static class HtmlHelperExtensions
	{
		
		public static string VariantAttributeValueName<T>(this HtmlHelper<T> helper)
		{
			string result =
				"<i class='<#= TypeNameClass #>' title='<#= TypeName #>'></i>" +
				"<# if(ColorSquaresRgb && ColorSquaresRgb.length > 0) {#>" +
				"<span class=\"color-container\"><span class=\"color\" style=\"background:<#= ColorSquaresRgb #>\">&nbsp;</span></span>" +
				"<span><#= Name #><#= QuantityInfo #></span>" +
				"<# } else { #>" +
				"<span><#= Name #><#= QuantityInfo #></span>" +
				"<# } #>";

			return result;
		}

		public static HelperResult VariantAttributeValueName<T>(this HtmlHelper<T> helper, ProductModel.ProductVariantAttributeValueModel model)
		{
			string colorSquares = "";

			if (model.ColorSquaresRgb.HasValue())
			{
				colorSquares = "<span class=\"color-container\"><span class=\"color\" style=\"background:{0}\">&nbsp;</span></span>".FormatInvariant(model.ColorSquaresRgb);
			}

			string result = "<i class='{0}' title='{1}'></i>{2}<span>{3}{4}</span>".FormatInvariant(
				model.TypeNameClass, model.TypeName, colorSquares, helper.Encode(model.Name), model.QuantityInfo);

			return new HelperResult(writer => writer.Write(result));
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