using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Tasks;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews
{
    /// <summary>
    /// trusted shops customer reviews provider
    /// </summary>
    public class TrustedShopsCustomerReviewsPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly TrustedShopsCustomerReviewsSettings _trustedShopsCustomerReviewsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public TrustedShopsCustomerReviewsPlugin(TrustedShopsCustomerReviewsSettings trustedShopsCustomerReviewsSettings, 
            ILocalizationService localizationService,
            ISettingService settingService)
        {
            _trustedShopsCustomerReviewsSettings = trustedShopsCustomerReviewsSettings;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return !string.IsNullOrWhiteSpace(_trustedShopsCustomerReviewsSettings.WidgetZone)
                       ? new List<string>() { _trustedShopsCustomerReviewsSettings.WidgetZone, "checkout_completed_bottom" }
                       : new List<string>() { "left_side_column_before", "checkout_completed_bottom" };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TrustedShopsCustomerReviews";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "TrustedShopsCustomerReviews";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {            
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            //create task 
            EngineContext.Current.Resolve<IScheduleTaskService>().InsertTask(new ScheduleTask
            {
                Name = "trusted shops review update",
                Seconds = 24 * 60 * 60,
                Type = "trusted-shops-review-update",
                Enabled = true,
                StopOnError = false,
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.TrustedShopsCustomerReviews", false);

            _settingService.DeleteSetting<TrustedShopsCustomerReviewsSettings>();

            //delete task 
            var task = EngineContext.Current.Resolve<IScheduleTaskService>().GetTaskByType("trusted-shops-review-update");
            EngineContext.Current.Resolve<IScheduleTaskService>().DeleteTask(task);

            //TODO: delete MessageToken

            base.Uninstall();
        }
    }
}
