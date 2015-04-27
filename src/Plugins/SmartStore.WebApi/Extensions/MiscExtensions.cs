using System.Text;
using System.Web.Mvc;

namespace SmartStore.WebApi
{
	public static class PublicMiscExtensions
	{
		public static string ApiGridButtons<T>(this HtmlHelper<T> helper)
		{
			var sb = new StringBuilder();

			sb.Append("<div data-id=\"<#= Id #>\">");

			sb.AppendFormat("<button name=\"ApiButtonRemoveKeys\" class=\"btn btn-danger api-grid-button\" style=\"display:<#= ButtonDisplayRemoveKeys #>;\"><i class=\"fa fa-times\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextRemoveKeys"]);

			sb.AppendFormat("<button name=\"ApiButtonCreateKeys\" class=\"btn btn-primary api-grid-button\" style=\"display:<#= ButtonDisplayCreateKeys #>;\"><i class=\"fa fa-check\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextCreateKeys"]);

			sb.AppendFormat("<button name=\"ApiButtonEnable\" class=\"btn api-grid-button\" style=\"display:<#= ButtonDisplayEnable #>;\"><i class=\"fa fa-plus icon-active-true\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextEnable"]);

			sb.AppendFormat("<button name=\"ApiButtonDisable\" class=\"btn api-grid-button\" style=\"display:<#= ButtonDisplayDisable #>;\"><i class=\"fa fa-minus icon-active-false\"></i>&nbsp;{0}</button>",
				helper.ViewData["ButtonTextDisable"]);

			sb.Append("</div>");

			return sb.ToString();
		}
	}

	internal static class MiscExtensions
	{
		public static string ToUnquoted(this string value)
		{
			if (value.HasValue() && value.Length > 1)
			{
				if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
					return value.Substring(1, value.Length - 2);
			}
			return value;
		}
	}
}
