using SmartStore.Core.Configuration;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Plugin.Feed.ElmarShopinfo
{
	public class ElmarShopinfoSettings : PromotionFeedSettings, ISettings
    {
		public string StaticFileNameXml { get; set; }
		public bool ExportEanAsIsbn { get; set; }
		public bool ExportSpecialPrice { get; set; }
		public string Warranty { get; set; }
		public string CategoryMapping { get; set; }

		public string UpdateDay { get; set; }
		public string UpdateHourFrom { get; set; }
		public string UpdateHourTo { get; set; }

		public bool BranchOffice { get; set; }
		public string AddressName { get; set; }
		public string AddressStreet { get; set; }
		public string AddressCity { get; set; }
		public string AddressPostalCode { get; set; }

		public string PublicMail { get; set; }
		public string PrivateMail { get; set; }
		public string OrderPhone { get; set; }
		public string OrderFax { get; set; }
		public string Hotline { get; set; }
		public bool ExportHotlineCost { get; set; }
		public decimal HotlineCostPerMinute { get; set; }

	}
}