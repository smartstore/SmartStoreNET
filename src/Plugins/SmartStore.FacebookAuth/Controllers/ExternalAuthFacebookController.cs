﻿using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.FacebookAuth.Core;
using SmartStore.FacebookAuth.Models;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.FacebookAuth.Controllers
{
    //[UnitOfWork]
	public class ExternalAuthFacebookController : PluginControllerBase
    {
        private readonly ISettingService _settingService;
        private readonly FacebookExternalAuthSettings _facebookExternalAuthSettings;
        private readonly IOAuthProviderFacebookAuthorizer _oAuthProviderFacebookAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
		private readonly IStoreContext _storeContext;
		private readonly IPermissionService _permissionService;

        public ExternalAuthFacebookController(ISettingService settingService,
            FacebookExternalAuthSettings facebookExternalAuthSettings,
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
			IStoreContext storeContext,
			IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._facebookExternalAuthSettings = facebookExternalAuthSettings;
            this._oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
			this._storeContext = storeContext;
			this._permissionService = permissionService;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
				return Content("Access denied");

            var model = new ConfigurationModel();
            model.ClientKeyIdentifier = _facebookExternalAuthSettings.ClientKeyIdentifier;
            model.ClientSecret = _facebookExternalAuthSettings.ClientSecret;
            
            return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
				return Content("Access denied");

            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _facebookExternalAuthSettings.ClientKeyIdentifier = model.ClientKeyIdentifier;
            _facebookExternalAuthSettings.ClientSecret = model.ClientSecret;
            _settingService.SaveSetting(_facebookExternalAuthSettings);
            
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View();
        }

		[NonAction]
		private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
			var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName("SmartStore.FacebookAuth", _storeContext.CurrentStore.Id);
			if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings))
			{
				throw new SmartException("Facebook module cannot be loaded");
			}

            var viewModel = new LoginModel();
            TryUpdateModel(viewModel);

			var result = _oAuthProviderFacebookAuthorizer.Authorize(returnUrl, verifyResponse);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    {
                        if (!result.Success)
                            foreach (var error in result.Errors)
								NotifyError(error);

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

		public ActionResult Login(string returnUrl)
		{
			return LoginInternal(returnUrl, false);
		}

		public ActionResult LoginCallback(string returnUrl)
		{
			return LoginInternal(returnUrl, true);
		}
	}
}