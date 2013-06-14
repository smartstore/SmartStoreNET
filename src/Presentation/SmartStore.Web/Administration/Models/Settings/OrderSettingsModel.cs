using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(OrderSettingsValidator))]
    public class OrderSettingsModel : ModelBase
    {
        public OrderSettingsModel()
        {
            GiftCards_Activated_OrderStatuses = new List<SelectListItem>();
            GiftCards_Deactivated_OrderStatuses = new List<SelectListItem>();
        }

		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.IsReOrderAllowed")]
        public StoreDependingSetting<bool> IsReOrderAllowed { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount")]
        public StoreDependingSetting<decimal> MinOrderSubtotalAmount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.MinOrderTotalAmount")]
        public StoreDependingSetting<decimal> MinOrderTotalAmount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.AnonymousCheckoutAllowed")]
        public StoreDependingSetting<bool> AnonymousCheckoutAllowed { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.TermsOfServiceEnabled")]
        public StoreDependingSetting<bool> TermsOfServiceEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.OnePageCheckoutEnabled")]
        public StoreDependingSetting<bool> OnePageCheckoutEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestsEnabled")]
        public StoreDependingSetting<bool> ReturnRequestsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestReasons")]
        public string ReturnRequestReasonsParsed { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestActions")]
        public string ReturnRequestActionsParsed { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.NumberOfDaysReturnRequestAvailable")]
        public StoreDependingSetting<int> NumberOfDaysReturnRequestAvailable { get; set; }
        
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.GiftCards_Activated")]
        public int? GiftCards_Activated_OrderStatusId { get; set; }
        public IList<SelectListItem> GiftCards_Activated_OrderStatuses { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.GiftCards_Deactivated")]
        public int? GiftCards_Deactivated_OrderStatusId { get; set; }
        public IList<SelectListItem> GiftCards_Deactivated_OrderStatuses { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Order.OrderIdent")]
        public int? OrderIdent { get; set; }
    }
}