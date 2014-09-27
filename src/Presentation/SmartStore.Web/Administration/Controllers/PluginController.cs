using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Core;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services.Configuration;
using System.IO;
using SmartStore.Services.Stores;
using System.Collections.Generic;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class PluginController : AdminControllerBase
	{
		#region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
	    private readonly ISettingService _settingService;
		private readonly IStoreService _storeService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly WidgetSettings _widgetSettings;
		private readonly IProviderManager _providerManager;
		private readonly PluginMediator _pluginMediator;

	    #endregion

		#region Constructors

        public PluginController(IPluginFinder pluginFinder,
            ILocalizationService localizationService,
			IWebHelper webHelper,
            IPermissionService permissionService,
			ILanguageService languageService,
            ISettingService settingService,
			IStoreService storeService,
            PaymentSettings paymentSettings,
			ShippingSettings shippingSettings,
            TaxSettings taxSettings, 
			ExternalAuthenticationSettings externalAuthenticationSettings, 
            WidgetSettings widgetSettings,
			IProviderManager providerManager,
			PluginMediator pluginMediator)
		{
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._permissionService = permissionService;
            this._languageService = languageService;
            this._settingService = settingService;
			this._storeService = storeService;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._widgetSettings = widgetSettings;
			this._providerManager = providerManager;
			this._pluginMediator = pluginMediator;
			T = NullLocalizer.Instance;
		}

		#endregion 

        #region Utilities

		public Localizer T { get; set; }

        [NonAction]
        protected PluginModel PreparePluginModel(PluginDescriptor pluginDescriptor, bool forList = true)
        {
            var pluginModel = pluginDescriptor.ToModel();

            pluginModel.Group = _localizationService.GetResource("Admin.Plugins.KnownGroup." + pluginDescriptor.Group);

			if (forList)
			{
				pluginModel.FriendlyName = pluginDescriptor.GetLocalizedValue(_localizationService, "FriendlyName");
				pluginModel.Description = pluginDescriptor.GetLocalizedValue(_localizationService, "Description");
			}

            //locales
            AddLocales(_languageService, pluginModel.Locales, (locale, languageId) =>
            {
				locale.FriendlyName = pluginDescriptor.GetLocalizedValue(_localizationService, "FriendlyName", languageId, false);
				locale.Description = pluginDescriptor.GetLocalizedValue(_localizationService, "Description", languageId, false);
            });
			//stores
			pluginModel.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();
			pluginModel.SelectedStoreIds = _settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArray();
			pluginModel.LimitedToStores = pluginModel.SelectedStoreIds.Count() > 0;
			pluginModel.IconUrl = _pluginMediator.GetIconUrl(pluginDescriptor);
            
            if (pluginDescriptor.Installed)
            {
                //specify configuration URL only when a plugin is already installed

                //plugins do not provide a general URL for configuration
                //because some of them have some custom URLs for configuration
                //for example, discount requirement plugins require additional parameters and attached to a certain discount
                var pluginInstance = pluginDescriptor.Instance();
                string configurationUrl = null;
                bool canChangeEnabled = false;
                bool isEnabled = false;

                if (pluginInstance is IPaymentMethod)
                {
                    //payment plugin
                    configurationUrl = Url.Action("ConfigureMethod", "Payment", new { systemName = pluginDescriptor.SystemName });
                    canChangeEnabled = true;
                    isEnabled = ((IPaymentMethod)pluginInstance).IsPaymentMethodActive(_paymentSettings);
                }
				//else if (pluginInstance is IShippingRateComputationMethod)
				//{
				//	//shipping rate computation method
				//	configurationUrl = Url.Action("ConfigureProvider", "Shipping", new { systemName = pluginDescriptor.SystemName });
				//	canChangeEnabled = true;
				//	isEnabled = ((IShippingRateComputationMethod)pluginInstance).IsShippingRateComputationMethodActive(_shippingSettings);
				//}
                else if (pluginInstance is ITaxProvider)
                {
                    //tax provider
                    configurationUrl = Url.Action("ConfigureProvider", "Tax", new { systemName = pluginDescriptor.SystemName });
                    canChangeEnabled = true;
                    isEnabled = pluginDescriptor.SystemName.Equals(_taxSettings.ActiveTaxProviderSystemName, StringComparison.InvariantCultureIgnoreCase);
                }
                else if (pluginInstance is IExternalAuthenticationMethod)
                {
                    //external auth method
                    configurationUrl = Url.Action("ConfigureMethod", "ExternalAuthentication", new { systemName = pluginDescriptor.SystemName });
                    canChangeEnabled = true;
                    isEnabled = ((IExternalAuthenticationMethod)pluginInstance).IsMethodActive(_externalAuthenticationSettings);
                }
				//else if (pluginInstance is IWidgetPlugin)
				//{
				//	// Widgets plugins
				//	configurationUrl = Url.Action("ConfigureWidget", "Widget", new { systemName = pluginDescriptor.SystemName });
				//	canChangeEnabled = true;
				//	isEnabled = ((IWidgetPlugin)pluginInstance).IsWidgetActive(_widgetSettings);
				//}
                else if (pluginInstance is IMiscPlugin)
                {
                    //Misc plugins
                    configurationUrl = Url.Action("ConfigureMiscPlugin", "Plugin", new { systemName = pluginDescriptor.SystemName });
                }
                pluginModel.ConfigurationUrl = configurationUrl;
                pluginModel.CanChangeEnabled = canChangeEnabled;
                pluginModel.IsEnabled = isEnabled;

            }
            return pluginModel;
        }

        [NonAction]
        protected LocalPluginsModel PrepareLocalPluginsModel()
        {
            var plugins = _pluginFinder.GetPluginDescriptors(false)
                .OrderBy(p => p.Group, PluginFileParser.KnownGroupComparer)
                .ThenBy(p => p.DisplayOrder)
                .Select(x => PreparePluginModel(x));

            var model = new LocalPluginsModel();

			model.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();

            var groupedPlugins = from p in plugins
                                 group p by p.Group into g
                                 select g;

            foreach (var group in groupedPlugins)
            {
                foreach (var plugin in group)
                {
                    model.Groups.Add(group.Key, plugin);
                }
            }

            return model;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = PrepareLocalPluginsModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult ExecuteTasks(IEnumerable<string> pluginsToInstall, IEnumerable<string> pluginsToUninstall)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                int tasksCount = 0;
                IEnumerable<PluginDescriptor> descriptors = null;

                // Uninstall first
                if (pluginsToUninstall != null && pluginsToUninstall.Any())
                {
                    descriptors = _pluginFinder.GetPluginDescriptors(false).Where(x => pluginsToUninstall.Contains(x.SystemName));
                    foreach (var d in descriptors)
                    {
                        if (d.Installed)
                        {
                            d.Instance().Uninstall();
                            tasksCount++;
                        }
                    }
                }

                // now execute installations
                if (pluginsToInstall != null && pluginsToInstall.Any())
                {
                    descriptors = _pluginFinder.GetPluginDescriptors(false).Where(x => pluginsToInstall.Contains(x.SystemName));
                    foreach (var d in descriptors)
                    {
                        if (!d.Installed)
                        {
                            d.Instance().Install();
                            tasksCount++;
                        }
                    }
                }

                // restart application
                if (tasksCount > 0)
                {
                    _webHelper.RestartAppDomain();
                }
            }
            catch (Exception exc)
            {
                NotifyError(exc);
            }

            return RedirectToAction("List");
        }

        public ActionResult ReloadList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //restart application
            _webHelper.RestartAppDomain();
            return RedirectToAction("List");
        }
        
        public ActionResult ConfigureMiscPlugin(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IMiscPlugin>(systemName);
            if (descriptor == null || !descriptor.Installed)
                return Redirect("List");

            var plugin  = descriptor.Instance<IMiscPlugin>();

            string actionName, controllerName;
            RouteValueDictionary routeValues;
            plugin.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            var model = new MiscPluginModel();
			model.FriendlyName = descriptor.GetLocalizedValue(_localizationService, "FriendlyName");
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            
            return View(model);
        }

		public ActionResult ConfigureProvider(string systemName, string listUrl)
		{
			var provider = _providerManager.GetProvider(systemName);
			if (provider == null || !provider.Metadata.IsConfigurable)
			{
				return HttpNotFound();
			}

			PermissionRecord requiredPermission = StandardPermissionProvider.AccessAdminPanel;
			var listUrl2 = Url.Action("List");

			var metadata = provider.Metadata;

			if (metadata.ProviderType == typeof(ITaxProvider))
			{
				requiredPermission = StandardPermissionProvider.ManageTaxSettings;
				listUrl2 = Url.Action("Providers", "Tax");
			}
			else if (metadata.ProviderType == typeof(IShippingRateComputationMethod))
			{
				requiredPermission = StandardPermissionProvider.ManageShippingSettings;
				listUrl2 = Url.Action("Providers", "Shipping");
			}
			else if (metadata.ProviderType == typeof(IWidget))
			{
				requiredPermission = StandardPermissionProvider.ManageWidgets;
				listUrl2 = Url.Action("Providers", "Widget");
			}

			if (!_permissionService.Authorize(requiredPermission))
			{
				return AccessDeniedView();
			}

			var model = _pluginMediator.ToProviderModel(provider);

			ViewBag.ListUrl = listUrl.NullEmpty() ?? listUrl2;

			return View(model);
		}

        public ActionResult EditPopup(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, false);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return RedirectToAction("List");

            var model = PreparePluginModel(pluginDescriptor, false);

            return View(model);
        }

		public ActionResult EditProviderPopup(string systemName)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var provider = _providerManager.GetProvider(systemName);
			if (provider == null)
				return HttpNotFound();

			var model = _pluginMediator.ToProviderModel(provider, true);

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata, languageId, false);
				locale.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata, languageId, false);
			});

			return View(model);
		}

		[HttpPost]
		public ActionResult EditProviderPopup(string btnId, ProviderModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var provider = _providerManager.GetProvider(model.SystemName);
			if (provider == null)
				return HttpNotFound();

			var metadata = provider.Metadata;

			_pluginMediator.SetSetting(metadata, "FriendlyName", model.FriendlyName);
			_pluginMediator.SetSetting(metadata, "Description", model.Description);

			foreach (var localized in model.Locales)
			{
				_pluginMediator.SaveLocalizedValue(metadata, localized.LanguageId, "FriendlyName", localized.FriendlyName);
				_pluginMediator.SaveLocalizedValue(metadata, localized.LanguageId, "Description", localized.Description);
			}

			ViewBag.RefreshPage = true;
			ViewBag.btnId = btnId;
			return View(model);
		}

		[HttpPost]
		public ActionResult EditPopup(string btnId, string formId, PluginModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(model.SystemName, false);
			if (pluginDescriptor == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				pluginDescriptor.FriendlyName = model.FriendlyName;
				pluginDescriptor.Description = model.Description;
				pluginDescriptor.DisplayOrder = model.DisplayOrder;

				PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);

				string settingKey = pluginDescriptor.GetSettingKey("LimitedToStores");
				if (model.LimitedToStores && model.SelectedStoreIds != null && model.SelectedStoreIds.Count() > 0)
					_settingService.SetSetting<string>(settingKey, string.Join(",", model.SelectedStoreIds));
				else
					_settingService.DeleteSetting(settingKey);

				// reset string resources cache
				_localizationService.ClearCache();

				var pluginInstance = pluginDescriptor.Instance();

				foreach (var localized in model.Locales)
				{
					pluginInstance.SaveLocalizedValue(_localizationService, localized.LanguageId, "FriendlyName", localized.FriendlyName);
					pluginInstance.SaveLocalizedValue(_localizationService, localized.LanguageId, "Description", localized.Description);
				}

				//enabled/disabled
				if (pluginDescriptor.Installed)
				{
					if (pluginInstance is IPaymentMethod)
					{
						//payment plugin
						var pm = (IPaymentMethod)pluginInstance;
						if (pm.IsPaymentMethodActive(_paymentSettings))
						{
							if (!model.IsEnabled)
							{
								//mark as disabled
								_paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
								_settingService.SaveSetting(_paymentSettings);
							}
						}
						else
						{
							if (model.IsEnabled)
							{
								//mark as active
								_paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
								_settingService.SaveSetting(_paymentSettings);
							}
						}
					}
					//else if (pluginInstance is IShippingRateComputationMethod)
					//{
					//	//shipping rate computation method
					//	var srcm = (IShippingRateComputationMethod)pluginInstance;
					//	if (srcm.IsShippingRateComputationMethodActive(_shippingSettings))
					//	{
					//		if (!model.IsEnabled)
					//		{
					//			//mark as disabled
					//			_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.PluginDescriptor.SystemName);
					//			_settingService.SaveSetting(_shippingSettings);
					//		}
					//	}
					//	else
					//	{
					//		if (model.IsEnabled)
					//		{
					//			//mark as active
					//			_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.PluginDescriptor.SystemName);
					//			_settingService.SaveSetting(_shippingSettings);
					//		}
					//	}
					//}
					else if (pluginInstance is ITaxProvider)
					{
						//tax provider
						if (model.IsEnabled)
						{
							_taxSettings.ActiveTaxProviderSystemName = model.SystemName;
							_settingService.SaveSetting(_taxSettings);
						}
						else
						{
							_taxSettings.ActiveTaxProviderSystemName = "";
							_settingService.SaveSetting(_taxSettings);
						}
					}
					else if (pluginInstance is IExternalAuthenticationMethod)
					{
						//external auth method
						var eam = (IExternalAuthenticationMethod)pluginInstance;
						if (eam.IsMethodActive(_externalAuthenticationSettings))
						{
							if (!model.IsEnabled)
							{
								//mark as disabled
								_externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(eam.PluginDescriptor.SystemName);
								_settingService.SaveSetting(_externalAuthenticationSettings);
							}
						}
						else
						{
							if (model.IsEnabled)
							{
								//mark as active
								_externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(eam.PluginDescriptor.SystemName);
								_settingService.SaveSetting(_externalAuthenticationSettings);
							}
						}
					}
					//else if (pluginInstance is IWidgetPlugin)
					//{
					//	//Misc plugins
					//	var widget = (IWidgetPlugin)pluginInstance;
					//	if (widget.IsWidgetActive(_widgetSettings))
					//	{
					//		if (!model.IsEnabled)
					//		{
					//			//mark as disabled
					//			_widgetSettings.ActiveWidgetSystemNames.Remove(widget.PluginDescriptor.SystemName);
					//			_settingService.SaveSetting(_widgetSettings);
					//		}
					//	}
					//	else
					//	{
					//		if (model.IsEnabled)
					//		{
					//			//mark as active
					//			_widgetSettings.ActiveWidgetSystemNames.Add(widget.PluginDescriptor.SystemName);
					//			_settingService.SaveSetting(_widgetSettings);
					//		}
					//	}
					//}
				}

				ViewBag.RefreshPage = true;
				ViewBag.btnId = btnId;
				ViewBag.formId = formId;
				return View(model);
			}

			//If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpPost]
		public ActionResult SetSelectedStores(string pk /* SystemName */, string name, FormCollection form)
		{
			// gets called from x-editable 
			try 
			{
				var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(pk, false);
				if (pluginDescriptor == null)
				{
					return HttpNotFound("The plugin does not exist");
				}
				
				string settingKey = pluginDescriptor.GetSettingKey("LimitedToStores");
				var storeIds = (form["value[]"] ?? "0").Split(',').Select(x => x.ToInt()).Where(x => x > 0).ToList();
				if (storeIds.Count > 0)
				{
					_settingService.SetSetting<string>(settingKey, string.Join(",", storeIds));
				}
				else
				{
					_settingService.DeleteSetting(settingKey);
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(501, ex.Message);
			}

			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			return new HttpStatusCodeResult(200);
		}

		[HttpPost]
		public ActionResult SortProviders(string providers)
		{
			try
			{
				var arr = providers.Split(',');
				int ordinal = 5;
				foreach (var systemName in arr)
				{
					var provider = _providerManager.GetProvider(systemName);
					if (provider != null)
					{
						_pluginMediator.SetUserDisplayOrder(provider.Metadata, ordinal);
					}
					ordinal += 5;
				}
			}
			catch (Exception ex)
			{
				NotifyError(ex.Message);
				return new HttpStatusCodeResult(501, ex.Message);
			}


			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			return new HttpStatusCodeResult(200);
		}

		public ActionResult UpdateStringResources(string systemName, string returnUrl = null)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var pluginDescriptor = _pluginFinder.GetPluginDescriptors()
				.Where(x => x.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase))
				.FirstOrDefault();

			if (pluginDescriptor == null)
			{
				NotifyError(_localizationService.GetResource("Admin.Configuration.Plugins.Resources.UpdateFailure"));
			}
			else
			{
				_localizationService.ImportPluginResourcesFromXml(pluginDescriptor, null, false);
				NotifySuccess(_localizationService.GetResource("Admin.Configuration.Plugins.Resources.UpdateSuccess"));
			}

			if (returnUrl.IsEmpty())
			{
				return RedirectToAction("List");
			}

			return Redirect(returnUrl);
		}

		public ActionResult UpdateAllStringResources()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var pluginDescriptors = _pluginFinder.GetPluginDescriptors(false);

			foreach (var plugin in pluginDescriptors)
			{
				if (plugin.Installed)
				{
					_localizationService.ImportPluginResourcesFromXml(plugin, null, false);
				}
				else
				{
					_localizationService.DeleteLocaleStringResources(plugin.ResourceRootKey);
				}
			}

			NotifySuccess(_localizationService.GetResource("Admin.Configuration.Plugins.Resources.UpdateSuccess"));
			return RedirectToAction("List");
		}

        #endregion
    }
}
