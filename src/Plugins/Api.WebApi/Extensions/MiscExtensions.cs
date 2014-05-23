using System;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Seo;

namespace SmartStore.Plugin.Api.WebApi.Extensions
{
	public static class MiscExtensions
	{
		public static string ApiGridButtons<T>(this HtmlHelper<T> helper)
		{
			var sb = new StringBuilder();

			sb.Append("<div data-id=\"<#= Id #>\">");

			sb.AppendFormat("<button name=\"ApiButtonRemoveKeys\" class=\"btn btn-danger api-grid-button\" style=\"display:<#= ButtonDisplayRemoveKeys #>;\"><i class=\"icon-trash\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextRemoveKeys"]);

			sb.AppendFormat("<button name=\"ApiButtonCreateKeys\" class=\"btn btn-primary api-grid-button\" style=\"display:<#= ButtonDisplayCreateKeys #>;\"><i class=\"icon-ok\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextCreateKeys"]);

			sb.AppendFormat("<button name=\"ApiButtonEnable\" class=\"btn api-grid-button\" style=\"display:<#= ButtonDisplayEnable #>;\"><i class=\"icon-active-true\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextEnable"]);

			sb.AppendFormat("<button name=\"ApiButtonDisable\" class=\"btn api-grid-button\" style=\"display:<#= ButtonDisplayDisable #>;\"><i class=\"icon-active-false\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextDisable"]);

			sb.Append("</div>");

			return sb.ToString();
		}

		// TODO: move that to UrlRecordService
		public static void EnsureUrlRecord<T>(this IUrlRecordService urlRecordService, T entity, Expression<Func<T, string>> nameProperty)
			where T : BaseEntity, ISlugSupported
		{
			string name = nameProperty.Compile().Invoke(entity);

			string existingSeName = entity.GetSeName<T>(0, true, false);
			existingSeName = entity.ValidateSeName(existingSeName, name, true);

			urlRecordService.SaveSlug(entity, existingSeName, 0);
		}
	}
}
