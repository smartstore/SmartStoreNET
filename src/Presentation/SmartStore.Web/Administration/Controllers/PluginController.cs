using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Core;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Plugins;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Plugins;

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
		private readonly ILicenseService _licenseService;

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
			PluginMediator pluginMediator,
			ILicenseService licenseService)
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
			this._licenseService = licenseService;

			T = NullLocalizer.Instance;
		}

		#endregion 

        #region Utilities

		public Localizer T { get; set; }

        [NonAction]
        protected PluginModel PreparePluginModel(PluginDescriptor pluginDescriptor, bool forList = true)
        {
            var model = pluginDescriptor.ToModel();

            model.Group = _localizationService.GetResource("Admin.Plugins.KnownGroup." + pluginDescriptor.Group);

			if (forList)
			{
				model.FriendlyName = pluginDescriptor.GetLocalizedValue(_localizationService, "FriendlyName");
				model.Description = pluginDescriptor.GetLocalizedValue(_localizationService, "Description");
			}

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
				locale.FriendlyName = pluginDescriptor.GetLocalizedValue(_localizationService, "FriendlyName", languageId, false);
				locale.Description = pluginDescriptor.GetLocalizedValue(_localizationService, "Description", languageId, false);
            });

			// Stores
			model.SelectedStoreIds = _settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArray();

			// Icon
			model.IconUrl = _pluginMediator.GetIconUrl(pluginDescriptor);
            
            if (pluginDescriptor.Installed)
            {
                // specify configuration URL only when a plugin is already installed
				if (pluginDescriptor.IsConfigurable)
				{
					model.ConfigurationUrl = Url.Action("ConfigurePlugin", new { systemName = pluginDescriptor.SystemName });

					if (!forList)
					{
						var configurable = pluginDescriptor.Instance() as IConfigurable;

						string actionName;
						string controllerName;
						RouteValueDictionary routeValues;
						configurable.GetConfigurationRoute(out actionName, out controllerName, out routeValues);

						if (actionName.HasValue() && controllerName.HasValue())
						{
							model.ConfigurationRoute = new RouteInfo(actionName, controllerName, routeValues);
						}
					}
				}

				if (pluginDescriptor.IsLicensable)
				{
					// we always show license button to serve ability to delete a license
					model.LicenseUrl = Url.Action("LicensePlugin", new { systemName = pluginDescriptor.SystemName });
					model.IsLicensed = (_licenseService.GetAllLicenses().FirstOrDefault(x => x.SystemName == pluginDescriptor.SystemName) != null);
				}
            }
            return model;
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
        
        public ActionResult ConfigurePlugin(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            if (descriptor == null || !descriptor.Installed || !descriptor.IsConfigurable)
				return HttpNotFound();

			var model = PreparePluginModel(descriptor, false);
			model.FriendlyName = descriptor.GetLocalizedValue(_localizationService, "FriendlyName");
            
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

			if (metadata.ProviderType == typeof(IPaymentMethod))
			{
				requiredPermission = StandardPermissionProvider.ManagePaymentMethods;
				listUrl2 = Url.Action("Providers", "Payment");
			}
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
			else if (metadata.ProviderType == typeof(IExternalAuthenticationMethod))
			{
				requiredPermission = StandardPermissionProvider.ManageExternalAuthenticationMethods;
				listUrl2 = Url.Action("Providers", "ExternalAuthentication");
			}

			if (!_permissionService.Authorize(requiredPermission))
			{
				return AccessDeniedView();
			}

			var model = _pluginMediator.ToProviderModel(provider);

			ViewBag.ListUrl = listUrl.NullEmpty() ?? listUrl2;

			return View(model);
		}

		public ActionResult LicensePlugin(string systemName, string licenseKey)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedPartialView();

			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
			if (descriptor == null || !descriptor.Installed || !descriptor.IsLicensable)
				return Content(T("Admin.Common.ResourceNotFound"));

			var licensable = descriptor.Instance() as ILicensable;
			var stores = _storeService.GetAllStores();
			var licenses = _licenseService.GetAllLicenses();

			var model = new LicensePluginModel
			{
				SystemName = systemName,
				Licenses = new List<LicensePluginModel.LicenseModel>()
			};

			if (licensable.HasSingleLicenseForAllStores)
			{
				model.Licenses.Add(new LicensePluginModel.LicenseModel
				{
					LicenseKey = licenses.Where(x => x.SystemName == systemName && x.StoreId == 0).Select(x => x.LicenseKey).FirstOrDefault(),
					StoreId = 0
				});
			}
			else
			{
				foreach (var store in stores)
				{
					var key = licenses.Where(x => x.SystemName == systemName && x.StoreId == store.Id).Select(x => x.LicenseKey).FirstOrDefault();
					model.Licenses.Add(new LicensePluginModel.LicenseModel()
					{
						LicenseKey = key,
						OldLicenseKey = key,
						StoreId = store.Id,
						StoreName = store.Name,
						StoreUrl = store.Url
					});
				}
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult LicensePlugin(string systemName, LicensePluginModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
			if (descriptor == null || !descriptor.Installed || !descriptor.IsLicensable)
				return HttpNotFound();

			string failureMessage = null;
			var licenses = _licenseService.GetAllLicenses();

			foreach (var item in model.Licenses)
			{
				var existingLicense = licenses.FirstOrDefault(x => x.SystemName == systemName && x.LicenseKey == item.OldLicenseKey);

				if (existingLicense != null && (item.LicenseKey.IsEmpty() || existingLicense.LicenseKey != item.LicenseKey))
					_licenseService.DeleteLicense(existingLicense);

				if (item.LicenseKey.IsEmpty() || (existingLicense != null && existingLicense.LicenseKey == item.LicenseKey))
					continue;

				if (!_licenseService.Activate(systemName, item.LicenseKey, item.StoreId, item.StoreUrl, out failureMessage))
				{
					NotifyError(failureMessage);

					return RedirectToAction("List");
				}				
			}

			NotifySuccess(T("Admin.Configuration.Plugins.LicenseActivated"));

			return RedirectToAction("List");
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
