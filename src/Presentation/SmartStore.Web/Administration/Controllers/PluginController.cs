using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;
using SmartStore.Licensing;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class PluginController : AdminControllerBase
    {
        private readonly IPluginFinder _pluginFinder;
        private readonly ILanguageService _languageService;
        private readonly IProviderManager _providerManager;
        private readonly PluginMediator _pluginMediator;

        public PluginController(
            IPluginFinder pluginFinder,
            ILanguageService languageService,
            IProviderManager providerManager,
            PluginMediator pluginMediator)
        {
            _pluginFinder = pluginFinder;
            _languageService = languageService;
            _providerManager = providerManager;
            _pluginMediator = pluginMediator;
        }

        #region Utilities

        private bool IsLicensable(PluginDescriptor pluginDescriptor)
        {
            var result = false;

            try
            {
                result = LicenseChecker.IsLicensablePlugin(pluginDescriptor);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return result;
        }

        private LicensingData PrepareLicenseLabelModel(LicenseLabelModel model, PluginDescriptor pluginDescriptor, string url = null)
        {
            if (IsLicensable(pluginDescriptor))
            {
                // We always show license button to serve ability to delete a key.
                model.IsLicensable = true;
                model.LicenseUrl = Url.Action("LicensePlugin", new { systemName = pluginDescriptor.SystemName });

                var cachedLicense = LicenseChecker.GetLicense(pluginDescriptor.SystemName, url);
                if (cachedLicense == null)
                {
                    // Licensed plugin has not been used yet -> Check state.
                    model.LicenseState = LicenseChecker.CheckState(pluginDescriptor.SystemName, url);

                    // And try to get license data again.
                    cachedLicense = LicenseChecker.GetLicense(pluginDescriptor.SystemName, url);
                }

                if (cachedLicense != null)
                {
                    // Licensed plugin has been used.
                    model.LicenseState = cachedLicense.State;
                    model.TruncatedLicenseKey = cachedLicense.TruncatedLicenseKey;
                    model.RemainingDemoUsageDays = cachedLicense.RemainingDemoDays;
                }
                else
                {
                    // It's confusing to display a license state when there is no license data yet.
                    model.HideLabel = true;
                }

                return cachedLicense;
            }

            return null;
        }

        [NonAction]
        protected PluginModel PreparePluginModel(PluginDescriptor pluginDescriptor, bool forList = true)
        {
            var model = pluginDescriptor.ToModel();

            // Using GetResource because T could fallback to NullLocalizer here.
            model.Group = Services.Localization.GetResource("Admin.Plugins.KnownGroup." + pluginDescriptor.Group);

            if (forList)
            {
                model.FriendlyName = pluginDescriptor.GetLocalizedValue(Services.Localization, "FriendlyName");
                model.Description = pluginDescriptor.GetLocalizedValue(Services.Localization, "Description");
            }

            // Locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.FriendlyName = pluginDescriptor.GetLocalizedValue(Services.Localization, "FriendlyName", languageId, false);
                locale.Description = pluginDescriptor.GetLocalizedValue(Services.Localization, "Description", languageId, false);
            });

            // Stores
            model.SelectedStoreIds = Services.Settings.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArray();

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

                // License label
                PrepareLicenseLabelModel(model.LicenseLabel, pluginDescriptor);
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

            model.AvailableStores = Services.StoreService
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

        #region Plugins

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Plugin.Read)]
        public ActionResult List()
        {
            var model = PrepareLocalPluginsModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Plugin.Install)]
        public ActionResult ExecuteTasks(IEnumerable<string> pluginsToInstall, IEnumerable<string> pluginsToUninstall)
        {
            try
            {
                var tasksCount = 0;
                IEnumerable<PluginDescriptor> descriptors = null;

                // Uninstall first.
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

                // Now execute installations.
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

                // Restart application.
                if (tasksCount > 0)
                {
                    Services.WebHelper.RestartAppDomain(aggressive: true);
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Plugin.Read)]
        public ActionResult ReloadList()
        {
            Services.WebHelper.RestartAppDomain(aggressive: true);

            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Plugin.Read)]
        public ActionResult ConfigurePlugin(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            if (descriptor == null || !descriptor.Installed || !descriptor.IsConfigurable)
            {
                return HttpNotFound();
            }

            var model = PreparePluginModel(descriptor, false);
            model.FriendlyName = descriptor.GetLocalizedValue(Services.Localization, "FriendlyName");

            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Configuration.Plugin.Update)]
        public ActionResult SetSelectedStores(string pk /* SystemName */, string name, FormCollection form)
        {
            // Gets called from x-editable.
            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(pk, false);
                if (pluginDescriptor == null)
                {
                    return HttpNotFound("The plugin does not exist.");
                }

                string settingKey = pluginDescriptor.GetSettingKey("LimitedToStores");
                var storeIds = (form["value[]"] ?? "0").Split(',').Select(x => x.ToInt()).Where(x => x > 0).ToList();
                if (storeIds.Count > 0)
                {
                    Services.Settings.SetSetting<string>(settingKey, string.Join(",", storeIds));
                }
                else
                {
                    Services.Settings.DeleteSetting(settingKey);
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Plugin.Update)]
        public ActionResult UpdateStringResources(string systemName)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptors()
                .FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));

            var success = false;
            var message = "";
            if (pluginDescriptor == null)
            {
                message = T("Admin.Configuration.Plugins.Resources.UpdateFailure").Text;
            }
            else
            {
                Services.Localization.ImportPluginResourcesFromXml(pluginDescriptor, null, false);

                success = true;
                message = T("Admin.Configuration.Plugins.Resources.UpdateSuccess").Text;
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = success,
                    Message = message
                }
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Plugin.Update)]
        public ActionResult UpdateAllStringResources()
        {
            var pluginDescriptors = _pluginFinder.GetPluginDescriptors(false);

            foreach (var plugin in pluginDescriptors)
            {
                if (plugin.Installed)
                {
                    Services.Localization.ImportPluginResourcesFromXml(plugin, null, false);
                }
                else
                {
                    Services.Localization.DeleteLocaleStringResources(plugin.ResourceRootKey);
                }
            }

            return new JsonResult
            {
                Data = new { 
                    Success = true,
                    Message = T("Admin.Configuration.Plugins.Resources.UpdateSuccess").Text
                }
            };
        }

        #endregion

        #region Providers

        public ActionResult ConfigureProvider(string systemName, string listUrl)
        {
            var provider = _providerManager.GetProvider(systemName);
            if (provider == null || !provider.Metadata.IsConfigurable)
            {
                return HttpNotFound();
            }

            var infos = GetProviderInfos(provider);

            if (infos.ReadPermission.HasValue() && !Services.Permissions.Authorize(infos.ReadPermission))
            {
                return AccessDeniedView();
            }

            var model = _pluginMediator.ToProviderModel(provider);

            ViewBag.ListUrl = listUrl.NullEmpty() ?? infos.Url;

            return View(model);
        }

        public ActionResult EditProviderPopup(string systemName)
        {
            var provider = _providerManager.GetProvider(systemName);
            if (provider == null)
            {
                return HttpNotFound();
            }

            var infos = GetProviderInfos(provider);

            if (infos.ReadPermission.HasValue() && !Services.Permissions.Authorize(infos.ReadPermission))
            {
                return AccessDeniedPartialView();
            }

            var model = _pluginMediator.ToProviderModel(provider, true);
            var pageTitle = model.FriendlyName;

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata, languageId, false);
                locale.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata, languageId, false);

                if (pageTitle.IsEmpty() && languageId == Services.WorkContext.WorkingLanguage.Id)
                {
                    pageTitle = locale.FriendlyName;
                }
            });

            ViewBag.Title = pageTitle;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProviderPopup(string btnId, ProviderModel model)
        {
            var provider = _providerManager.GetProvider(model.SystemName);
            if (provider == null)
            {
                return HttpNotFound();
            }

            var infos = GetProviderInfos(provider);

            if (infos.UpdatePermission.HasValue() && !Services.Permissions.Authorize(infos.UpdatePermission))
            {
                return AccessDeniedPartialView();
            }

            _pluginMediator.SetSetting(provider.Metadata, "FriendlyName", model.FriendlyName);
            _pluginMediator.SetSetting(provider.Metadata, "Description", model.Description);

            foreach (var localized in model.Locales)
            {
                _pluginMediator.SaveLocalizedValue(provider.Metadata, localized.LanguageId, "FriendlyName", localized.FriendlyName);
                _pluginMediator.SaveLocalizedValue(provider.Metadata, localized.LanguageId, "Description", localized.Description);
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;

            return View(model);
        }

        [HttpPost]
        public ActionResult SortProviders(string providers)
        {
            try
            {
                var arr = providers.Split(',');
                var ordinal = 5;
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

        private (string ReadPermission, string UpdatePermission, string Url) GetProviderInfos(Provider<IProvider> provider)
        {
            string readPermission = null;
            string updatePermission = null;
            string url = null;

            var metadata = provider.Metadata;

            if (metadata.ProviderType == typeof(IPaymentMethod))
            {
                readPermission = Permissions.Configuration.PaymentMethod.Read;
                updatePermission = Permissions.Configuration.PaymentMethod.Update;
                url = Url.Action("Providers", "Payment");
            }
            else if (metadata.ProviderType == typeof(ITaxProvider))
            {
                readPermission = Permissions.Configuration.Tax.Read;
                updatePermission = Permissions.Configuration.Tax.Update;
                url = Url.Action("Providers", "Tax");
            }
            else if (metadata.ProviderType == typeof(IShippingRateComputationMethod))
            {
                readPermission = Permissions.Configuration.Shipping.Read;
                updatePermission = Permissions.Configuration.Shipping.Update;
                url = Url.Action("Providers", "Shipping");
            }
            else if (metadata.ProviderType == typeof(IWidget))
            {
                readPermission = Permissions.Cms.Widget.Read;
                updatePermission = Permissions.Cms.Widget.Update;
                url = Url.Action("Providers", "Widget");
            }
            else if (metadata.ProviderType == typeof(IExternalAuthenticationMethod))
            {
                readPermission = Permissions.Configuration.Authentication.Read;
                updatePermission = Permissions.Configuration.Authentication.Update;
                url = Url.Action("Providers", "ExternalAuthentication");
            }

            return (readPermission, updatePermission, url);
        }

        #endregion

        #region Licensing

        [Permission(Permissions.Configuration.Plugin.License)]
        public ActionResult LicensePlugin(string systemName, string licenseKey)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            if (descriptor == null || !descriptor.Installed)
            {
                return Content(T("Admin.Common.ResourceNotFound"));
            }

            var isLicensable = LicenseChecker.IsLicensablePlugin(descriptor, out bool singleLicenseForAllStores);
            if (!isLicensable)
            {
                return Content(T("Admin.Common.ResourceNotFound"));
            }

            var stores = Services.StoreService.GetAllStores();
            var model = new LicensePluginModel
            {
                SystemName = systemName,
                StoreLicenses = new List<LicensePluginModel.StoreLicenseModel>()
            };

            // Validate store url.
            foreach (var store in stores)
            {
                if (!Services.StoreService.IsStoreDataValid(store))
                {
                    model.InvalidDataStoreId = store.Id;
                    return View(model);
                }
            }

            if (singleLicenseForAllStores)
            {
                var licenseModel = new LicensePluginModel.StoreLicenseModel();
                var license = PrepareLicenseLabelModel(licenseModel.LicenseLabel, descriptor);
                if (license != null)
                {
                    licenseModel.LicenseKey = license.TruncatedLicenseKey;
                }

                model.StoreLicenses.Add(licenseModel);
            }
            else
            {
                foreach (var store in stores)
                {
                    var licenseModel = new LicensePluginModel.StoreLicenseModel
                    {
                        StoreId = store.Id,
                        StoreName = store.Name,
                        StoreUrl = store.Url
                    };

                    var license = PrepareLicenseLabelModel(licenseModel.LicenseLabel, descriptor, store.Url);
                    if (license != null)
                    {
                        licenseModel.LicenseKey = license.TruncatedLicenseKey;
                    }

                    model.StoreLicenses.Add(licenseModel);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Plugin.License)]
        public ActionResult LicensePlugin(string systemName, LicensePluginModel model)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            if (descriptor == null || !descriptor.Installed)
            {
                return HttpNotFound();
            }

            var isLicensable = IsLicensable(descriptor);
            if (!isLicensable)
            {
                return HttpNotFound();
            }

            if (model.StoreLicenses != null)
            {
                foreach (var item in model.StoreLicenses)
                {
                    var result = LicenseChecker.Activate(item.LicenseKey, descriptor.SystemName, item.StoreUrl);
                    if (result == null)
                    {
                        // Do nothing, skiped.
                    }
                    else if (result.Success)
                    {
                        NotifySuccess(T("Admin.Configuration.Plugins.LicenseActivated"));
                    }
                    else
                    {
                        if (result.IsFailureWarning)
                        {
                            NotifyWarning(result.ToString());
                        }
                        else
                        {
                            NotifyError(result.ToString());
                        }

                        return RedirectToAction("List");
                    }
                }
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Plugin.License)]
        public ActionResult LicenseResetStatusCheck(string systemName)
        {
            // Reset state for current store.
            var result = LicenseChecker.ResetState(systemName);
            LicenseCheckerResult subShopResult = null;

            var model = new LicenseLabelModel
            {
                IsLicensable = true,
                LicenseUrl = Url.Action("LicensePlugin", new { systemName = systemName }),
                LicenseState = result.State,
                TruncatedLicenseKey = result.TruncatedLicenseKey,
                RemainingDemoUsageDays = result.RemainingDemoDays
            };

            // Reset state for all other stores.
            if (result.Success)
            {
                var currentStoreId = Services.StoreContext.CurrentStore.Id;
                var allStores = Services.StoreService.GetAllStores();

                foreach (var store in allStores.Where(x => x.Id != currentStoreId && x.Url.HasValue()))
                {
                    subShopResult = LicenseChecker.ResetState(systemName, store.Url);
                    if (!subShopResult.Success)
                    {
                        result = subShopResult;
                        break;
                    }
                }
            }

            // Notify about result.
            if (result.Success)
            {
                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            }
            else
            {
                var message = HtmlUtils.ConvertPlainTextToHtml(result.ToString());
                if (result.IsFailureWarning)
                {
                    NotifyWarning(message);
                }
                else
                {
                    NotifyError(message);
                }
            }

            return PartialView("Partials/LicenseLabel", model);
        }

        #endregion
    }
}
