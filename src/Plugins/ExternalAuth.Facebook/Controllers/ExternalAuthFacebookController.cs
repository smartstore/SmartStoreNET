using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.ExternalAuth.Facebook.Core;
using SmartStore.Plugin.ExternalAuth.Facebook.Models;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.ExternalAuth.Facebook.Controllers
{
    //[UnitOfWork]
    public class ExternalAuthFacebookController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly FacebookExternalAuthSettings _facebookExternalAuthSettings;
        private readonly IOAuthProviderFacebookAuthorizer _oAuthProviderFacebookAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
		private readonly IStoreContext _storeContext;
		private readonly IPluginFinder _pluginFinder;

        public ExternalAuthFacebookController(ISettingService settingService,
            FacebookExternalAuthSettings facebookExternalAuthSettings,
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
			IStoreContext storeContext,
			IPluginFinder pluginFinder)
        {
            this._settingService = settingService;
            this._facebookExternalAuthSettings = facebookExternalAuthSettings;
            this._oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
			this._storeContext = storeContext;
			this._pluginFinder = pluginFinder;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.ClientKeyIdentifier = _facebookExternalAuthSettings.ClientKeyIdentifier;
            model.ClientSecret = _facebookExternalAuthSettings.ClientSecret;
            
            return View("SmartStore.Plugin.ExternalAuth.Facebook.Views.ExternalAuthFacebook.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _facebookExternalAuthSettings.ClientKeyIdentifier = model.ClientKeyIdentifier;
            _facebookExternalAuthSettings.ClientSecret = model.ClientSecret;
            _settingService.SaveSetting(_facebookExternalAuthSettings);
            
            return View("SmartStore.Plugin.ExternalAuth.Facebook.Views.ExternalAuthFacebook.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View("SmartStore.Plugin.ExternalAuth.Facebook.Views.ExternalAuthFacebook.PublicInfo");
        }


        public ActionResult Login(string returnUrl)
        {
            var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName("ExternalAuth.Facebook");
            if (processor == null ||
                !processor.IsMethodActive(_externalAuthenticationSettings) ||
				!processor.PluginDescriptor.Installed ||
				!_pluginFinder.AuthenticateStore(processor.PluginDescriptor, _storeContext.CurrentStore.Id))
                throw new SmartException("Facebook module cannot be loaded");

            var viewModel = new LoginModel();
            TryUpdateModel(viewModel);

            var result = _oAuthProviderFacebookAuthorizer.Authorize(returnUrl);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    {
                        if (!result.Success)
                            foreach (var error in result.Errors)
                                ExternalAuthorizerHelper.AddErrorsToDisplay(error);

                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AssociateOnLogon:
                    {
                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    {
                        //result
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                    }
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                    }
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                    }
                default:
                    break;
            }

            if (result.Result != null) return result.Result;
            return HttpContext.Request.IsAuthenticated ? new RedirectResult(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/") : new RedirectResult(Url.LogOn(returnUrl));
        }
    }
}