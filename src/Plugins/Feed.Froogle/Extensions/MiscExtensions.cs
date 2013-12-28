using System.Web.Mvc;

namespace SmartStore.Plugin.Feed.Froogle
{
	public static class MiscExtensions
	{
		public static string XEditableLink(this HtmlHelper hlp, string fieldName, string type)
		{
			string displayText = null;

			if (fieldName == "Gender" || fieldName == "AgeGroup")
				displayText = "<#= {0}Localize #>".FormatWith(fieldName);
			else
				displayText = "<#= {0} #>".FormatWith(fieldName);

			string skeleton =
				"<a href=\"#\" title=\"<#= {0} #>\" class=\"edit-link-{1}\"" +
				" data-pk=\"<#= ProductId #>\" data-name=\"{0}\" data-value=\"<#= {0} #>\" data-inputclass=\"edit-{1}\" data-type=\"{2}\">" +
				"{3}</a>";

			return skeleton.FormatWith(fieldName, fieldName.ToLower(), type, displayText);
		}
	}
}
