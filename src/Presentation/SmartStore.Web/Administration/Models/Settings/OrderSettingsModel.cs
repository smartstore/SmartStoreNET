using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(OrderSettingsValidator))]
	public class OrderSettingsModel : ModelBase, ILocalizedModel<OrderSettingsLocalizedModel>
    {
        public OrderSettingsModel()
        {
            GiftCards_Activated_OrderStatuses = new List<SelectListItem>();
            GiftCards_Deactivated_OrderStatuses = new List<SelectListItem>();
			Locales = new List<OrderSettingsLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.IsReOrderAllowed")]
        public bool IsReOrderAllowed { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount")]
        public decimal MinOrderSubtotalAmount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.MinOrderTotalAmount")]
        public decimal MinOrderTotalAmount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.AnonymousCheckoutAllowed")]
        public bool AnonymousCheckoutAllowed { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.TermsOfServiceEnabled")]
        public bool TermsOfServiceEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Order.DisableOrderCompletedPage")]
		public bool DisableOrderCompletedPage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestsEnabled")]
        public bool ReturnRequestsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestReasons")]
        public string ReturnRequestReasons { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestActions")]
        public string ReturnRequestActions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.NumberOfDaysReturnRequestAvailable")]
        public int NumberOfDaysReturnRequestAvailable { get; set; }
        
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.GiftCards_Activated")]
        public int? GiftCards_Activated_OrderStatusId { get; set; }
        public IList<SelectListItem> GiftCards_Activated_OrderStatuses { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.GiftCards_Deactivated")]
        public int? GiftCards_Deactivated_OrderStatusId { get; set; }
        public IList<SelectListItem> GiftCards_Deactivated_OrderStatuses { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
		public int StoreCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.OrderIdent")]
        public int? OrderIdent { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Order.DisplayOrdersOfAllStores")]
		public bool DisplayOrdersOfAllStores { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Order.OrderListPageSize")]
		public int OrderListPageSize { get; set; }

		public IList<OrderSettingsLocalizedModel> Locales { get; set; }
    }


	public class OrderSettingsLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestReasons")]
		public string ReturnRequestReasons { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestActions")]
		public string ReturnRequestActions { get; set; }
	}
}