using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Orders;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Controllers
{

    public class TrustedShopsCustomerReviewsController : Controller
    {
        private readonly TrustedShopsCustomerReviewsSettings _trustedShopsCustomerReviewsSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        public TrustedShopsCustomerReviewsController(TrustedShopsCustomerReviewsSettings trustedShopsCustomerReviewsSettings, 
            ISettingService settingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IOrderService orderService,
            ILogger logger,
            IStoreContext storeContext,
            IStoreService storeService)
        {
            _trustedShopsCustomerReviewsSettings = trustedShopsCustomerReviewsSettings;
            _settingService = settingService;
            _localizationService = localizationService;
            _workContext = workContext;
            _orderService = orderService;
            _logger = logger;
            _storeContext = storeContext;
            _storeService = storeService;
        }
        
        

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var trustedShopsCustomerReviewsSettings = _settingService.LoadSetting<TrustedShopsCustomerReviewsSettings>(storeScope);

            var model = new ConfigurationModel();
            model.TrustedShopsId = trustedShopsCustomerReviewsSettings.TrustedShopsId;
            model.TrustedShopsActivation = trustedShopsCustomerReviewsSettings.ActivationState;
            model.IsTestMode = trustedShopsCustomerReviewsSettings.IsTestMode;
            model.ShopName = trustedShopsCustomerReviewsSettings.ShopName;
            model.DisplayWidget = trustedShopsCustomerReviewsSettings.DisplayWidget;
            model.DisplayReviewLinkOnOrderCompleted = trustedShopsCustomerReviewsSettings.DisplayReviewLinkOnOrderCompleted;
            model.DisplayReviewLinkInEmail = trustedShopsCustomerReviewsSettings.DisplayReviewLinkInEmail;

            model.ZoneId = trustedShopsCustomerReviewsSettings.WidgetZone;
            model.AvailableZones.Add(new SelectListItem() { Text = "Before left side column", Value = "left_side_column_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "After left side column", Value = "left_side_column_after" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Before right side column", Value = "right_side_column_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "After right side column", Value = "right_side_column_after" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Homepage bottom", Value = "home_page_bottom" });

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.GetOverrideKeys(trustedShopsCustomerReviewsSettings, model, storeScope, _settingService);
            
            return View("SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Views.TrustedShopsCustomerReviews.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var trustedShopsCustomerReviewsSettings = _settingService.LoadSetting<TrustedShopsCustomerReviewsSettings>(storeScope);

            //save settings
            trustedShopsCustomerReviewsSettings.TrustedShopsId = model.TrustedShopsId;
            trustedShopsCustomerReviewsSettings.IsTestMode = model.IsTestMode;
            trustedShopsCustomerReviewsSettings.ActivationState = model.TrustedShopsActivation;
            trustedShopsCustomerReviewsSettings.WidgetZone = model.ZoneId;
            trustedShopsCustomerReviewsSettings.ShopName = model.ShopName;
            trustedShopsCustomerReviewsSettings.DisplayWidget = model.DisplayWidget;
            trustedShopsCustomerReviewsSettings.DisplayReviewLinkOnOrderCompleted = model.DisplayReviewLinkOnOrderCompleted;
            trustedShopsCustomerReviewsSettings.DisplayReviewLinkInEmail = model.DisplayReviewLinkInEmail;
            _settingService.SaveSetting(trustedShopsCustomerReviewsSettings);

            var updateTask = new UpdateRatingWidgetStateTask(trustedShopsCustomerReviewsSettings, 
                _settingService, 
                _localizationService, 
                _workContext, 
                _logger);

            updateTask.Execute();

            storeDependingSettingHelper.UpdateSettings(trustedShopsCustomerReviewsSettings, form, storeScope, _settingService);
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone)
        {
            var trustedShopsCustomerReviewsSettings = _settingService.LoadSetting<TrustedShopsCustomerReviewsSettings>(_storeContext.CurrentStore.Id);

            if (widgetZone == "checkout_completed_bottom")
            {
                var checkoutModel = new PublicInfoCheckoutModel();
                checkoutModel.TrustedShopsId = trustedShopsCustomerReviewsSettings.TrustedShopsId;
                checkoutModel.BuyerEmail = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_workContext.CurrentCustomer.BillingAddress.Email));

                var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
                    null, null, null, null, null, null, null, 0, 1).FirstOrDefault();

                checkoutModel.ShopOrderId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(order.Id.ToString()));
                checkoutModel.DisplayReviewLinkOnOrderCompleted = trustedShopsCustomerReviewsSettings.DisplayReviewLinkOnOrderCompleted;

                //display PublicInfoCheckout-View
                return View("SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Views.TrustedShopsCustomerReviews.PublicInfoCheckout", checkoutModel);
            }

            var model = new PublicInfoModel();
            model.TrustedShopsId = trustedShopsCustomerReviewsSettings.TrustedShopsId;
            model.ShopName = trustedShopsCustomerReviewsSettings.ShopName;
            model.DisplayWidget = trustedShopsCustomerReviewsSettings.DisplayWidget;

            return View("SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Views.TrustedShopsCustomerReviews.PublicInfo", model);
        }
    }
}