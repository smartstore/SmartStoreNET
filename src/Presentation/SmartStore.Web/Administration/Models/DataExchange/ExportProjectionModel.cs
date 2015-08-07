using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.DataExchange
{
	public class ExportProjectionModel
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.LanguageId")]
		public int? LanguageId { get; set; }
		public List<SelectListItem> AvailableLanguages { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Projection.CurrencyId")]
		public int? CurrencyId { get; set; }
		public List<SelectListItem> AvailableCurrencies { get; set; }
	}
}