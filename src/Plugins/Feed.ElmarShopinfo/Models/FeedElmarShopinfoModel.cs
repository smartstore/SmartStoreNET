using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Plugin.Feed.ElmarShopinfo.Models
{
    public class FeedElmarShopinfoModel
	{
		#region General

		public string GenerateFeedResult { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.StaticFileUrl")]
		public List<GeneratedFeedFile> GeneratedFiles { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.BuildDescription")]
		public string BuildDescription { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.DescriptionToPlainText")]
		public bool DescriptionToPlainText { get; set; }

        [SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.ProductPictureSize")]
        public int ProductPictureSize { get; set; }

        [SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.Currency")]
        public int CurrencyId { get; set; }
		public List<SelectListItem> AvailableCurrencies { get; set; }

        [SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.ShippingCost")]
        public decimal ShippingCost { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.ShippingTime")]
		public string ShippingTime { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.Brand")]
		public string Brand { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.Availability")]
		public string Availability { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.Warranty")]
		public string Warranty { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.ExportEanAsIsbn")]
		public bool ExportEanAsIsbn { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.ExportSpecialPrice")]
		public bool ExportSpecialPrice { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.CategoryMapping")]
		public string CategoryMapping { get; set; }
		public List<SelectListItem> ElmarCategories { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.Store")]
		public int StoreId { get; set; }
		public List<SelectListItem> AvailableStores { get; set; }

		#endregion
		
		#region Automation
		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.TaskEnabled")]
		public bool TaskEnabled { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.GenerateStaticFileEachMinutes")]
		public int GenerateStaticFileEachMinutes { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.UpdateDay")]
		public string UpdateDay { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.UpdateHourFrom")]
		public string UpdateHourFrom { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.UpdateHourTo")]
		public string UpdateHourTo { get; set; }

		public List<SelectListItem> DayList { get; set; }
		public List<SelectListItem> HourList { get; set; }
		#endregion
		
		#region Address
		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.BranchOffice")]
		public bool BranchOffice { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.AddressName")]
		public string AddressName { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.AddressStreet")]
		public string AddressStreet { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.AddressCity")]
		public string AddressCity { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.AddressPostalCode")]
		public string AddressPostalCode { get; set; }
		#endregion
		
		#region Contact
		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.PublicMail")]
		public string PublicMail { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.PrivateMail")]
		public string PrivateMail { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.OrderPhone")]
		public string OrderPhone { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.OrderFax")]
		public string OrderFax { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.Hotline")]
		public string Hotline { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.ExportHotlineCost")]
		public bool ExportHotlineCost { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.ElmarShopinfo.HotlineCostPerMinute")]
		public decimal HotlineCostPerMinute { get; set; }
		#endregion


		public void Copy(ElmarShopinfoSettings settings, bool fromSettings) {
			if (fromSettings) {
				ProductPictureSize = settings.ProductPictureSize;
				CurrencyId = settings.CurrencyId;
				ShippingCost = settings.ShippingCost;
				ShippingTime = settings.ShippingTime;
				Availability = settings.Availability;
				BuildDescription = settings.BuildDescription;
				DescriptionToPlainText = settings.DescriptionToPlainText;
				Brand = settings.Brand;
				ExportEanAsIsbn = settings.ExportEanAsIsbn;
				ExportSpecialPrice = settings.ExportSpecialPrice;
				Warranty = settings.Warranty;
				UpdateDay = settings.UpdateDay;
				UpdateHourFrom = settings.UpdateHourFrom;
				UpdateHourTo = settings.UpdateHourTo;
				BranchOffice = settings.BranchOffice;
				AddressName = settings.AddressName;
				AddressStreet = settings.AddressStreet;
				AddressCity = settings.AddressCity;
				AddressPostalCode = settings.AddressPostalCode;
				PublicMail = settings.PublicMail;
				PrivateMail = settings.PrivateMail;
				OrderPhone = settings.OrderPhone;
				OrderFax = settings.OrderFax;
				Hotline = settings.Hotline;
				ExportHotlineCost = settings.ExportHotlineCost;
				HotlineCostPerMinute = settings.HotlineCostPerMinute;
				CategoryMapping = settings.CategoryMapping;
				StoreId = settings.StoreId;
			}
			else {
				settings.ProductPictureSize = ProductPictureSize;
				settings.CurrencyId = CurrencyId;
				settings.ShippingCost = ShippingCost;
				settings.ShippingTime = ShippingTime;
				settings.Availability = Availability;
				settings.BuildDescription = BuildDescription;
				settings.DescriptionToPlainText = DescriptionToPlainText;
				settings.Brand = Brand;
				settings.ExportEanAsIsbn = ExportEanAsIsbn;
				settings.ExportSpecialPrice = ExportSpecialPrice;
				settings.Warranty = Warranty;
				settings.UpdateDay = UpdateDay;
				settings.UpdateHourFrom = UpdateHourFrom;
				settings.UpdateHourTo = UpdateHourTo;
				settings.BranchOffice = BranchOffice;
				settings.AddressName = AddressName;
				settings.AddressStreet = AddressStreet;
				settings.AddressCity = AddressCity;
				settings.AddressPostalCode = AddressPostalCode;
				settings.PublicMail = PublicMail;
				settings.PrivateMail = PrivateMail;
				settings.OrderPhone = OrderPhone;
				settings.OrderFax = OrderFax;
				settings.Hotline = Hotline;
				settings.ExportHotlineCost = ExportHotlineCost;
				settings.HotlineCostPerMinute = HotlineCostPerMinute;
				settings.CategoryMapping = CategoryMapping;
				settings.StoreId = StoreId;
			}
		}
    }	// class
}
