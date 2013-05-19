using System;
using System.Web.Mvc;
using SmartStore.Plugin.Widgets.TrustedShopsSeal.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers
{
    
    public class TrustedShopsSealController : Controller
    {
        private readonly TrustedShopsSealSettings _trustedShopsSealSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public TrustedShopsSealController(TrustedShopsSealSettings trustedShopsSealSettings, 
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            _trustedShopsSealSettings = trustedShopsSealSettings;
            _settingService = settingService;
            _localizationService = localizationService;
        }
        
        

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.TrustedShopsId = _trustedShopsSealSettings.TrustedShopsId;
            model.IsTestMode = _trustedShopsSealSettings.IsTestMode;
            model.ShopName = _trustedShopsSealSettings.ShopName;
            model.ShopText = _trustedShopsSealSettings.ShopText;

            model.ZoneId = _trustedShopsSealSettings.WidgetZone;
            model.AvailableZones.Add(new SelectListItem() { Text = "Before left side column", Value = "left_side_column_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "After left side column", Value = "left_side_column_after" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Before right side column", Value = "right_side_column_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "After right side column", Value = "right_side_column_after" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Homepage bottom", Value = "home_page_bottom" });
            
            return View("SmartStore.Plugin.Widgets.TrustedShopsSeal.Views.TrustedShopsSeal.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

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
                    _trustedShopsSealSettings.TrustedShopsId = model.TrustedShopsId;
                    _trustedShopsSealSettings.IsTestMode = model.IsTestMode;
                    _trustedShopsSealSettings.WidgetZone = model.ZoneId;
                    _trustedShopsSealSettings.ShopName = model.ShopName;
                    _trustedShopsSealSettings.ShopText = model.ShopText;

                    _settingService.SaveSetting(_trustedShopsSealSettings);
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
                    _trustedShopsSealSettings.TrustedShopsId = model.TrustedShopsId;
                    _trustedShopsSealSettings.IsTestMode = model.IsTestMode;
                    _trustedShopsSealSettings.WidgetZone = model.ZoneId;
                    _trustedShopsSealSettings.ShopName = model.ShopName;
                    _trustedShopsSealSettings.ShopText = model.ShopText;

                    _settingService.SaveSetting(_trustedShopsSealSettings);
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
            var model = new PublicInfoModel();
            model.TrustedShopsId = _trustedShopsSealSettings.TrustedShopsId;
            model.IsTestMode = _trustedShopsSealSettings.IsTestMode;
            model.ShopName = _trustedShopsSealSettings.ShopName;
            model.ShopText = _trustedShopsSealSettings.ShopText;

            return View("SmartStore.Plugin.Widgets.TrustedShopsSeal.Views.TrustedShopsSeal.PublicInfo", model);
        }
    }
}