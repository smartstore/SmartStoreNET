using System.Web.Mvc;
using SmartStore.GoogleMerchantCenter.Domain;

namespace SmartStore.GoogleMerchantCenter
{
	public static class MiscExtensions
	{
		public static string XEditableLink(this HtmlHelper hlp, string fieldName, string type)
		{
			string displayText = null;

			if (fieldName == "Gender" || fieldName == "AgeGroup" || fieldName == "Export2" || fieldName == "IsBundle" || fieldName == "IsAdult")
				displayText = "<#= {0}Localize #>".FormatInvariant(fieldName);
			else
				displayText = "<#= {0} #>".FormatInvariant(fieldName);

			string skeleton =
				"<a href=\"#\" title=\"<#= {0} #>\" class=\"edit-link-{1}\"" +
				" data-pk=\"<#= ProductId #>\" data-name=\"{0}\" data-value=\"<#= {0} #>\" data-inputclass=\"edit-{1}\" data-type=\"{2}\">" +
				"{3}</a>";

			return skeleton.FormatInvariant(fieldName, fieldName.ToLower(), type, displayText);
		}

		public static bool IsTouched(this GoogleProductRecord p)
		{
			if (p != null)
			{
				return
					p.Taxonomy.HasValue() || p.Gender.HasValue() || p.AgeGroup.HasValue() || p.Color.HasValue() ||
					p.Size.HasValue() || p.Material.HasValue() || p.Pattern.HasValue() || p.ItemGroupId.HasValue() ||
					!p.Export || p.Multipack != 0 || p.IsBundle.HasValue || p.IsAdult.HasValue || p.EnergyEfficiencyClass.HasValue() ||
					p.CustomLabel0.HasValue() || p.CustomLabel1.HasValue() || p.CustomLabel2.HasValue() || p.CustomLabel3.HasValue() || p.CustomLabel4.HasValue();
			}
			return false;
		}
	}
}
