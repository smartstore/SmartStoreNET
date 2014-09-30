using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Plugin.ExternalAuth.OpenId.Core;
using SmartStore.Plugin.ExternalAuth.OpenId.Models;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.ExternalAuth.OpenId.Controllers
{
    //[UnitOfWork]
	public class ExternalAuthOpenIdController : PluginControllerBase
    {
        private readonly IOpenIdProviderAuthorizer _openIdProviderAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
		private readonly IStoreContext _storeContext;
		private readonly ISettingService _settingService;	// codehint: sm-add

        public ExternalAuthOpenIdController(IOpenIdProviderAuthorizer openIdProviderAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
			IStoreContext storeContext,
			ISettingService settingService)
        {
            this._openIdProviderAuthorizer = openIdProviderAuthorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
			this._storeContext = storeContext;
			this._settingService = settingService;	// codehint: sm-add
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View();
        }

        public ActionResult Login(string returnUrl)
        {
			var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName("ExternalAuth.OpenId", _storeContext.CurrentStore.Id);
			if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings))
			{
				throw new SmartException("OpenID module cannot be loaded");
			}

            if (!_openIdProviderAuthorizer.IsOpenIdCallback)
            {
                var viewModel = new LoginModel();
                TryUpdateModel(viewModel);
                _openIdProviderAuthorizer.EnternalIdentifier = viewModel.ExternalIdentifier;
            }

            var result = _openIdProviderAuthorizer.Authorize(returnUrl);
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
    }
}