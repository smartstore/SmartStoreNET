using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Admin.Models.ExternalAuthentication;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ExternalAuthenticationController : AdminControllerBase
    {
        #region Fields

        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ISettingService _settingService;
        private readonly PluginMediator _pluginMediator;

        #endregion

        #region Constructors

        public ExternalAuthenticationController(
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ISettingService settingService,
            PluginMediator pluginMediator)
        {
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _settingService = settingService;
            _pluginMediator = pluginMediator;
        }

        #endregion

        #region Methods

        [Permission(Permissions.Configuration.Authentication.Read)]
        public ActionResult Providers()
        {
            var methodsModel = new List<AuthenticationMethodModel>();
            var methods = _openAuthenticationService.LoadAllExternalAuthenticationMethods();
            foreach (var method in methods)
            {
                var model = _pluginMediator.ToProviderModel<IExternalAuthenticationMethod, AuthenticationMethodModel>(method);
                model.IsActive = method.IsMethodActive(_externalAuthenticationSettings);
                methodsModel.Add(model);
            }

            return View(methodsModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Authentication.Activate)]
        public ActionResult ActivateProvider(string systemName, bool activate)
        {
            var method = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(systemName);
            bool dirty = method.IsMethodActive(_externalAuthenticationSettings) != activate;
            if (dirty)
            {
                if (!activate)
                    _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(method.Metadata.SystemName);
                else
                    _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(method.Metadata.SystemName);

                _settingService.SaveSetting(_externalAuthenticationSettings);
                _pluginMediator.ActivateDependentWidgets(method.Metadata, activate);
            }

            return RedirectToAction("Providers");
        }

        #endregion
    }
}
