using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Plugin.Widgets.TrustedShopsSeal.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers
{
    
    public class TrustedShopsSealController : Controller
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

		public TrustedShopsSealController(IWorkContext workContext,
			IStoreContext storeContext, IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
			_workContext = workContext;
			_storeContext = storeContext;
			_storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var trustedShopsSealSettings = _settingService.LoadSetting<TrustedShopsSealSettings>(storeScope);

            var model = new ConfigurationModel();
            model.TrustedShopsId = trustedShopsSealSettings.TrustedShopsId;
            model.IsTestMode = trustedShopsSealSettings.IsTestMode;
            model.ShopName = trustedShopsSealSettings.ShopName;
            model.ShopText = trustedShopsSealSettings.ShopText;
            model.WidgetZone = trustedShopsSealSettings.WidgetZone;

            model.AvailableZones.Add(new SelectListItem() { Text = "Before left side column", Value = "left_side_column_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "After left side column", Value = "left_side_column_after" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Before right side column", Value = "right_side_column_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "After right side column", Value = "right_side_column_after" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Homepage bottom", Value = "home_page_bottom" });

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(trustedShopsSealSettings, model, storeScope, _settingService);
            
            return View("SmartStore.Plugin.Widgets.TrustedShopsSeal.Views.TrustedShopsSeal.Configure", model);
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
			var trustedShopsSealSettings = _settingService.LoadSetting<TrustedShopsSealSettings>(storeScope);

            //TODO: test and remove duplicate code
            //var tsProtectionService = new TrustedShopsSeal.com.trustedshops.qa.TSProtectionService();
            //if (!model.IsTestMode)
            //{
            //    tsProtectionService.Url = "https://www.trustedshops.com/ts/services/TsProtection?wsdl";
            //}

            //var certStatus = new TrustedShopsSeal.com.trustedshops.qa.CertificateStatus();
            //certStatus = tsProtectionService.checkCertificate(model.TrustedShopsId);

            //if (certStatus.stateEnum == "TEST" || certStatus.stateEnum == "PRODUCTION")
            //{
            //    // inform user about successfull validation
            //    this.AddNotificationMessage(NotifyType.Success, _localizationService.GetResource("Plugins.Widgets.TrustedShopsSeal.CheckIdSuccess"), true);

            //    //save settings
            //    trustedShopsSealSettings.TrustedShopsId = model.TrustedShopsId;
            //    trustedShopsSealSettings.IsTestMode = model.IsTestMode;
            //    trustedShopsSealSettings.WidgetZone = model.WidgetZone;
            //    trustedShopsSealSettings.ShopName = model.ShopName;
            //    trustedShopsSealSettings.ShopText = model.ShopText;

            //    storeDependingSettingHelper.UpdateSettings(trustedShopsSealSettings, form, storeScope, _settingService);
            //    _settingService.ClearCache();
            //}
            //else
            //{
            //    // inform user about validation error
            //    this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsSeal.CheckIdError"), true);
            //    model.TrustedShopsId = String.Empty;
            //    model.IsTestMode = false;
            //}


            var tsProtectionServiceSandbox = new TrustedShopsSeal.com.trustedshops.qa.TSProtectionService();
            var tsProtectionServiceLive = new TrustedShopsSeal.com.trustedshops.www.TSProtectionService();

            if (model.IsTestMode)
            {
                var certStatus = new TrustedShopsSeal.com.trustedshops.qa.CertificateStatus();
                certStatus = tsProtectionServiceSandbox.checkCertificate(model.TrustedShopsId);

                if (certStatus.stateEnum == "TEST")
                {
                    // inform user about successfull validation
                    this.AddNotificationMessage(NotifyType.Success, _localizationService.GetResource("Plugins.Widgets.TrustedShopsSeal.CheckIdSuccess"), true);

                    //save settings
                    trustedShopsSealSettings.TrustedShopsId = model.TrustedShopsId;
                    trustedShopsSealSettings.IsTestMode = model.IsTestMode;
                    trustedShopsSealSettings.WidgetZone = model.WidgetZone;
                    trustedShopsSealSettings.ShopName = model.ShopName;
                    trustedShopsSealSettings.ShopText = model.ShopText;

                    storeDependingSettingHelper.UpdateSettings(trustedShopsSealSettings, form, storeScope, _settingService);
                    _settingService.ClearCache();
                }
                else
                {
                    // inform user about validation error
                    this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsSeal.CheckIdError"), true);
                    model.TrustedShopsId = String.Empty;
                    model.IsTestMode = false;
                }
            }
            else
            {
                var certStatus = new TrustedShopsSeal.com.trustedshops.www.CertificateStatus();
                certStatus = tsProtectionServiceLive.checkCertificate(model.TrustedShopsId);

                if (certStatus.stateEnum == "PRODUCTION")
                {
                    // inform user about successfull validation
                    this.AddNotificationMessage(NotifyType.Success, _localizationService.GetResource("Plugins.Widgets.TrustedShopsSeal.CheckIdSuccess"), true);

                    //save settings
                    trustedShopsSealSettings.TrustedShopsId = model.TrustedShopsId;
                    trustedShopsSealSettings.IsTestMode = model.IsTestMode;
                    trustedShopsSealSettings.WidgetZone = model.WidgetZone;
                    trustedShopsSealSettings.ShopName = model.ShopName;
                    trustedShopsSealSettings.ShopText = model.ShopText;

                    storeDependingSettingHelper.UpdateSettings(trustedShopsSealSettings, form, storeScope, _settingService);
                    _settingService.ClearCache();
                }
                else
                {
                    // inform user about validation error
                    this.AddNotificationMessage(NotifyType.Error, _localizationService.GetResource("Plugins.Widgets.TrustedShopsSeal.CheckIdError"), true);
                    model.TrustedShopsId = String.Empty;
                    model.IsTestMode = false;

                }
            }

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone)
        {
			var trustedShopsSealSettings = _settingService.LoadSetting<TrustedShopsSealSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();
            model.TrustedShopsId = trustedShopsSealSettings.TrustedShopsId;
            model.IsTestMode = trustedShopsSealSettings.IsTestMode;
            model.ShopName = trustedShopsSealSettings.ShopName;
            model.ShopText = trustedShopsSealSettings.ShopText;

            return View("SmartStore.Plugin.Widgets.TrustedShopsSeal.Views.TrustedShopsSeal.PublicInfo", model);
        }
    }
}