using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Common;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Models.Newsletter;

namespace SmartStore.Web.Controllers
{
    public partial class NewsletterController : PublicControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly CustomerSettings _customerSettings;
        private readonly Lazy<PrivacySettings> _privacySettings;

        public NewsletterController(
            IWorkContext workContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            CustomerSettings customerSettings,
            IStoreContext storeContext,
            Lazy<PrivacySettings> privacySettings)
        {
            _workContext = workContext;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _customerSettings = customerSettings;
            _storeContext = storeContext;
            _privacySettings = privacySettings;
        }

        [HttpPost]
        [GdprConsent]
        public ActionResult Subscribe(bool subscribe, string email)
        {
            string result;
            var success = false;
            var customer = Services.WorkContext.CurrentCustomer;
            var hasConsentedToGdpr = customer.GetAttribute<bool>(SystemCustomerAttributeNames.HasConsentedToGdpr);
            var hasConsented = ViewData["GdprConsent"] != null ? (bool)ViewData["GdprConsent"] : hasConsentedToGdpr;

            if (!hasConsented && _privacySettings.Value.DisplayGdprConsentOnForms)
            {
                return Json(new
                {
                    Success = success,
                    Result = String.Empty
                });
            }

            if (!email.IsEmail())
            {
                result = T("Newsletter.Email.Wrong");
            }
            else
            {
                // subscribe/unsubscribe
                email = email.Trim();

                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(email, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    if (subscribe)
                    {
                        if (!subscription.Active)
                        {
                            Services.MessageFactory.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        result = T("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {
                            Services.MessageFactory.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        result = T("Newsletter.UnsubscribeEmailSent");
                    }
                }
                else if (subscribe)
                {
                    subscription = new NewsLetterSubscription
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        Active = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        StoreId = _storeContext.CurrentStore.Id,
                        WorkingLanguageId = _workContext.WorkingLanguage.Id
                    };

                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    Services.MessageFactory.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

                    result = T("Newsletter.SubscribeEmailSent");
                }
                else
                {
                    result = T("Newsletter.UnsubscribeEmailSent");
                }
                success = true;
            }

            return Json(new
            {
                Success = success,
                Result = result,
            });
        }

        public ActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
            {
                return HttpNotFound();
            }

            var model = new SubscriptionActivationModel();

            if (active)
            {
                subscription.Active = active;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
            {
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
            }

            model.Result = T(active ? "Newsletter.ResultActivated" : "Newsletter.ResultDeactivated");

            return View(model);
        }
    }
}
