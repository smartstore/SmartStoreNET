using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core.Configuration;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
    public abstract class PayPalRestApiControllerBase<TSetting> : PayPalPaymentControllerBase where TSetting : PayPalApiSettingsBase, ISettings, new()
    {
        public PayPalRestApiControllerBase(IPayPalService payPalService)
        {
            PayPalService = payPalService;
        }

        private string GetControllerName()
        {
            return GetType().Name.Replace("Controller", "");
        }

        protected IPayPalService PayPalService { get; private set; }

        [AdminAuthorize]
        public ActionResult UpsertExperienceProfile()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<TSetting>(storeScope);

            var store = Services.StoreService.GetStoreById(storeScope == 0 ? Services.StoreContext.CurrentStore.Id : storeScope);
            var session = new PayPalSessionData { ProviderSystemName = ProviderSystemName };

            var result = PayPalService.EnsureAccessToken(session, settings);
            if (result.Success)
            {
                result = PayPalService.UpsertCheckoutExperience(settings, session, store);
                if (result.Success && result.Id.HasValue())
                {
                    settings.ExperienceProfileId = result.Id;
                    Services.Settings.SaveSetting(settings, x => x.ExperienceProfileId, storeScope, true);
                }
            }

            if (result.Success)
            {
                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            }
            else
            {
                NotifyError(result.ErrorMessage);
            }

            return RedirectToAction("ConfigureProvider", "Plugin", new { area = "admin", systemName = ProviderSystemName });
        }

        [AdminAuthorize]
        public ActionResult DeleteExperienceProfile()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<TSetting>(storeScope);
            var session = new PayPalSessionData { ProviderSystemName = ProviderSystemName };

            var result = PayPalService.EnsureAccessToken(session, settings);
            if (result.Success)
            {
                result = PayPalService.DeleteCheckoutExperience(settings, session);
                if (result.Success)
                {
                    settings.ExperienceProfileId = null;
                    Services.Settings.SaveSetting(settings, x => x.ExperienceProfileId, storeScope, true);
                }
            }

            if (result.Success)
            {
                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            }
            else
            {
                NotifyError(result.ErrorMessage);
            }

            return RedirectToAction("ConfigureProvider", "Plugin", new { area = "admin", systemName = ProviderSystemName });
        }

        [AdminAuthorize]
        public ActionResult CreateWebhook()
        {
            var settings = Services.Settings.LoadSetting<TSetting>();
            var session = new PayPalSessionData { ProviderSystemName = ProviderSystemName };

            using (Services.Settings.BeginScope())
            {
                if (settings.WebhookId.HasValue())
                {
                    var result1 = PayPalService.DeleteWebhook(settings, session);
                    if (result1.Success)
                    {
                        settings.WebhookId = null;
                        Services.Settings.SaveSetting(settings, x => x.WebhookId, 0, false);
                    }
                }

                var url = Url.Action("Webhook", GetControllerName(), new { area = Plugin.SystemName }, "https");

                var result = PayPalService.EnsureAccessToken(session, settings);
                if (result.Success)
                {
                    result = PayPalService.CreateWebhook(settings, session, url);
                    if (result.Success)
                    {
                        settings.WebhookId = result.Id;
                        Services.Settings.SaveSetting(settings, x => x.WebhookId, 0, false);
                    }
                }

                if (result.Success)
                {
                    NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
                }
                else
                {
                    NotifyError(result.ErrorMessage);
                }
            }

            return RedirectToAction("ConfigureProvider", "Plugin", new { area = "admin", systemName = ProviderSystemName });
        }

        [AdminAuthorize]
        public ActionResult DeleteWebhook()
        {
            var settings = Services.Settings.LoadSetting<TSetting>();
            var session = new PayPalSessionData { ProviderSystemName = ProviderSystemName };

            if (settings.WebhookId.HasValue())
            {
                var result = PayPalService.EnsureAccessToken(session, settings);
                if (result.Success)
                {
                    result = PayPalService.DeleteWebhook(settings, session);
                    if (result.Success)
                    {
                        settings.WebhookId = null;
                        Services.Settings.SaveSetting(settings, x => x.WebhookId, 0, true);
                    }
                }

                if (result.Success)
                {
                    NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
                }
                else
                {
                    NotifyError(result.ErrorMessage);
                }
            }

            return RedirectToAction("ConfigureProvider", "Plugin", new { area = "admin", systemName = ProviderSystemName });
        }

        [ValidateInput(false)]
        public ActionResult Webhook()
        {
            HttpStatusCode result = HttpStatusCode.OK;

            try
            {
                string json = null;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    json = reader.ReadToEnd();
                }

                var settings = Services.Settings.LoadSetting<TSetting>();

                result = PayPalService.ProcessWebhook(settings, Request.Headers, json, ProviderSystemName);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, ex, null, null);
            }

            return new HttpStatusCodeResult(result);
        }

        protected bool SaveConfigurationModel(
            ApiConfigurationModel model,
            FormCollection form,
            Action<TSetting> map = null)
        {
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<TSetting>(storeScope);

            var oldClientId = settings.ClientId;
            var oldSecret = settings.Secret;
            var oldProfileId = settings.ExperienceProfileId;

            var validator = new PayPalApiConfigValidator(T, x =>
            {
                return storeScope == 0 || storeDependingSettingHelper.IsOverrideChecked(settings, x, form);
            });

            validator.Validate(model, ModelState);

            // Additional validation.
            if (ModelState.IsValid)
            {
                // PayPal review: check if credentials are valid.
                var credentialChanged = model.ClientId.HasValue() && model.Secret.HasValue() &&
                    (!model.ClientId.IsCaseInsensitiveEqual(settings.ClientId) || !model.Secret.IsCaseInsensitiveEqual(settings.Secret));

                if (credentialChanged)
                {
                    var session = new PayPalSessionData { ProviderSystemName = ProviderSystemName };
                    var tmpSettings = new PayPalApiSettingsBase
                    {
                        UseSandbox = model.UseSandbox,
                        ClientId = model.ClientId,
                        Secret = model.Secret
                    };

                    var result = PayPalService.EnsureAccessToken(session, tmpSettings);
                    if (!result.Success)
                    {
                        ModelState.AddModelError("", T("Plugins.SmartStore.PayPal.InvalidCredentials"));
                        ModelState.AddModelError("", result.ErrorMessage);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return false;
            }

            ModelState.Clear();
            model.TransactMode = TransactMode.AuthorizeAndCapture;

            MiniMapper.Map(model, settings);
            settings.ApiAccountName = model.ApiAccountName.TrimSafe();
            settings.ApiAccountPassword = model.ApiAccountPassword.TrimSafe();
            settings.ClientId = model.ClientId.TrimSafe();
            settings.ExperienceProfileId = model.ExperienceProfileId.TrimSafe();
            settings.Secret = model.Secret.TrimSafe();
            settings.Signature = model.Signature.TrimSafe();
            settings.WebhookId = model.WebhookId.TrimSafe();

            // Additional mapping.
            map?.Invoke(settings);

            // Credentials changed: reset profile and webhook id to avoid errors.
            if (!oldClientId.IsCaseInsensitiveEqual(settings.ClientId) || !oldSecret.IsCaseInsensitiveEqual(settings.Secret))
            {
                if (oldProfileId.IsCaseInsensitiveEqual(settings.ExperienceProfileId))
                {
                    settings.ExperienceProfileId = null;
                }

                settings.WebhookId = null;
            }

            using (Services.Settings.BeginScope())
            {
                storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
            }

            using (Services.Settings.BeginScope())
            {
                // Multistore context not possible, see IPN handling.
                Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);
            }

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
            return true;
        }
    }
}