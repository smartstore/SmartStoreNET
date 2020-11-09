using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.AmazonPay.Controllers;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Cms;
using SmartStore.Services.Customers;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Tasks;

namespace SmartStore.AmazonPay
{
    [DependentWidgets("Widgets.AmazonPay")]
    [FriendlyName("Amazon Pay")]
    [DisplayOrder(-1)]
    public class AmazonPayPlugin : PaymentPluginBase, IExternalAuthenticationMethod, IConfigurable, ICookiePublisher
    {
        private readonly IAmazonPayService _apiService;
        private readonly ICommonServices _services;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWidgetService _widgetService;
        private readonly WidgetSettings _widgetSettings;

        public AmazonPayPlugin(
            IAmazonPayService apiService,
            ICommonServices services,
            IOrderTotalCalculationService orderTotalCalculationService,
            IScheduleTaskService scheduleTaskService,
            IWidgetService widgetService,
            WidgetSettings widgetSettings)
        {
            _apiService = apiService;
            _services = services;
            _orderTotalCalculationService = orderTotalCalculationService;
            _scheduleTaskService = scheduleTaskService;
            _widgetService = widgetService;
            _widgetSettings = widgetSettings;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public static string SystemName => "SmartStore.AmazonPay";

        /// <summary>
        /// Gets CookieInfos for display in CookieManager dialog.
        /// </summary>
        /// <returns>CookieInfo containing plugin name, cookie purpose description & cookie type</returns>
        public IEnumerable<CookieInfo> GetCookieInfo()
        {
            var widget = _widgetService.LoadWidgetBySystemName("Widgets.AmazonPay");
            if (!widget.IsWidgetActive(_widgetSettings))
                return null;

            var cookieInfo = new CookieInfo
            {
                Name = _services.Localization.GetResource("Plugins.FriendlyName.Widgets.AmazonPay"),
                Description = _services.Localization.GetResource("Plugins.Payments.AmazonPay.CookieInfo"),
                CookieType = CookieType.Required
            };

            return new List<CookieInfo> { cookieInfo };
        }

        public override void Install()
        {
            _services.Settings.SaveSetting(new AmazonPaySettings());
            _services.Localization.ImportPluginResourcesFromXml(PluginDescriptor);

            // Polling task every 30 minutes.
            _scheduleTaskService.GetOrAddTask<DataPollingTask>(x =>
            {
                x.Name = _services.Localization.GetResource("Plugins.Payments.AmazonPay.TaskName");
                x.CronExpression = "*/30 * * * *";
            });

            base.Install();
        }

        public override void Uninstall()
        {
            _scheduleTaskService.TryDeleteTask<DataPollingTask>();
            _services.Settings.DeleteSetting<AmazonPaySettings>();
            _services.Localization.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = _apiService.ProcessPayment(processPaymentRequest);
            return result;
        }

        public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            _apiService.PostProcessPayment(postProcessPaymentRequest);
        }

        public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            var result = decimal.Zero;

            try
            {
                var settings = _services.Settings.LoadSetting<AmazonPaySettings>(_services.StoreContext.CurrentStore.Id);

                result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return result;
        }

        public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = _apiService.Capture(capturePaymentRequest);
            return result;
        }

        public override RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = _apiService.Refund(refundPaymentRequest);
            return result;
        }

        public override VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = _apiService.Void(voidPaymentRequest);
            return result;
        }


        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "AmazonPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "SmartStore.AmazonPay.Controllers" }, { "area", SystemName } };
        }

        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "AuthenticationPublicInfo";
            controllerName = "AmazonPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "SmartStore.AmazonPay.Controllers" }, { "area", SystemName } };
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            try
            {
                var settings = _services.Settings.LoadSetting<AmazonPaySettings>(_services.StoreContext.CurrentStore.Id);
                if (settings.ShowPayButtonForAdminOnly && !_services.WorkContext.CurrentCustomer.IsAdmin())
                {
                    actionName = controllerName = null;
                    routeValues = null;
                    return;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            actionName = "ShoppingCart";
            controllerName = "AmazonPayShoppingCart";
            routeValues = new RouteValueDictionary { { "Namespaces", "SmartStore.AmazonPay.Controllers" }, { "area", SystemName } };
        }

        public override Type GetControllerType()
        {
            return typeof(AmazonPayController);
        }

        public override bool SupportCapture => true;

        public override bool SupportPartiallyRefund => true;

        public override bool SupportRefund => true;

        public override bool SupportVoid => true;

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.StandardAndButton;
    }
}
