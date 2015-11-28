using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.FacebookAuth.Core;
using SmartStore.FacebookAuth.Models;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.FacebookAuth.Controllers
{
    //[UnitOfWork]
	public class ExternalAuthFacebookController : PluginControllerBase
    {
        private readonly IOAuthProviderFacebookAuthorizer _oAuthProviderFacebookAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
		private readonly ICommonServices _services;

        public ExternalAuthFacebookController(
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
			ICommonServices services)
        {
            this._oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
			this._services = services;
        }

		private bool HasPermission(bool notify = true)
		{
			bool hasPermission = _services.Permissions.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods);

			if (notify && !hasPermission)
				NotifyError(_services.Localization.GetResource("Admin.AccessDenied.Description"));

			return hasPermission;
		}
        
		[AdminAuthorize, ChildActionOnly]
        public ActionResult Configure()
        {
			if (!HasPermission(false))
				return AccessDeniedPartialView();

            var model = new ConfigurationModel();
			int storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<FacebookExternalAuthSettings>(storeScope);

            model.ClientKeyIdentifier = settings.ClientKeyIdentifier;
            model.ClientSecret = settings.ClientSecret;

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, _services.Settings);
            
            return View(model);
        }

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
			if (!HasPermission(false))
				return Configure();

            if (!ModelState.IsValid)
                return Configure();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			int storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<FacebookExternalAuthSettings>(storeScope);

            settings.ClientKeyIdentifier = model.ClientKeyIdentifier;
            settings.ClientSecret = model.ClientSecret;

			storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _services.Settings);
			_services.Settings.ClearCache();

			NotifySuccess(_services.Localization.GetResource("Admin.Common.DataSuccessfullySaved"));

			return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View();
        }

		[NonAction]
		private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
			var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(Provider.SystemName, _services.StoreContext.CurrentStore.Id);
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

            if (result.Result != null)
				return result.Result;

            return HttpContext.Request.IsAuthenticated ?
				new RedirectResult(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/") :
				new RedirectResult(Url.LogOn(returnUrl));
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