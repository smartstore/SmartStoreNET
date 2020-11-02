using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.FacebookAuth.Core;
using SmartStore.FacebookAuth.Models;
using SmartStore.Services.Authentication.External;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.FacebookAuth.Controllers
{
    public class ExternalAuthFacebookController : PluginControllerBase
    {
        private readonly IOAuthProviderFacebookAuthorizer _oAuthProviderFacebookAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;

        public ExternalAuthFacebookController(
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings)
        {
            _oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
        }

        [LoadSetting, AdminAuthorize, ChildActionOnly]
        [Permission(Permissions.Configuration.Authentication.Read)]
        public ActionResult Configure(FacebookExternalAuthSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);

            var host = Services.StoreContext.CurrentStore.GetHost(true);
            model.RedirectUrl = $"{host}Plugins/SmartStore.FacebookAuth/logincallback/";

            return View(model);
        }

        [SaveSetting, HttpPost, AdminAuthorize, ChildActionOnly]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Authentication.Update)]
        public ActionResult Configure(FacebookExternalAuthSettings settings, ConfigurationModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            MiniMapper.Map(model, settings);
            settings.ClientKeyIdentifier = model.ClientKeyIdentifier.TrimSafe();
            settings.ClientSecret = model.ClientSecret.TrimSafe();

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return RedirectToConfiguration(FacebookExternalAuthMethod.SystemName, true);
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            var settings = Services.Settings.LoadSetting<FacebookExternalAuthSettings>(Services.StoreContext.CurrentStore.Id);

            if (settings.ClientKeyIdentifier.HasValue() && settings.ClientSecret.HasValue())
            {
                return View();
            }

            return new EmptyResult();
        }

        public ActionResult Login(string returnUrl)
        {
            return LoginInternal(returnUrl, false);
        }

        public ActionResult LoginCallback(string returnUrl)
        {
            return LoginInternal(returnUrl, true);
        }

        [NonAction]
        private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
            var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(FacebookExternalAuthMethod.SystemName, Services.StoreContext.CurrentStore.Id);
            if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings))
            {
                NotifyError(T("Plugins.CannotLoadModule", T("Plugins.FriendlyName.SmartStore.FacebookAuth")));
                return new RedirectResult(Url.LogOn(returnUrl));
            }

            var viewModel = new LoginModel();
            TryUpdateModel(viewModel);

            var result = _oAuthProviderFacebookAuthorizer.Authorize(returnUrl, verifyResponse);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    {
                        if (!result.Success)
                        {
                            result.Errors.Each(x => NotifyError(x));
                        }

                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AssociateOnLogon:
                    {
                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });
                    }
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });
                    }
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                    }
                default:
                    break;
            }

            if (result.Result != null)
            {
                return result.Result;
            }

            return HttpContext.Request.IsAuthenticated ?
                RedirectToReferrer(returnUrl, "~/") :
                new RedirectResult(Url.LogOn(returnUrl));
        }
    }
}