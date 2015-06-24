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
		private readonly ICommonServices _commonServices;

        public ExternalAuthFacebookController(
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
			ICommonServices commonServices)
        {
            this._oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
			this._commonServices = commonServices;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
				return Content("Access denied");

            var model = new ConfigurationModel();
			int storeScope = this.GetActiveStoreScopeConfiguration(_commonServices.StoreService, _commonServices.WorkContext);
			var settings = _commonServices.Settings.LoadSetting<FacebookExternalAuthSettings>(storeScope);

            model.ClientKeyIdentifier = settings.ClientKeyIdentifier;
            model.ClientSecret = settings.ClientSecret;

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, _commonServices.Settings);
            
            return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
		public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
			if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
				return Content("Access denied");

            if (!ModelState.IsValid)
                return Configure();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			int storeScope = this.GetActiveStoreScopeConfiguration(_commonServices.StoreService, _commonServices.WorkContext);
			var settings = _commonServices.Settings.LoadSetting<FacebookExternalAuthSettings>(storeScope);

            settings.ClientKeyIdentifier = model.ClientKeyIdentifier;
            settings.ClientSecret = model.ClientSecret;

			storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _commonServices.Settings);
			_commonServices.Settings.ClearCache();

			NotifySuccess(_commonServices.Localization.GetResource("Admin.Common.DataSuccessfullySaved"));

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
			var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(Provider.SystemName, _commonServices.StoreContext.CurrentStore.Id);
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