﻿using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Models.Newsletter;

namespace SmartStore.Web.Controllers
{
	public partial class NewsletterController : PublicControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkflowMessageService _workflowMessageService;
		private readonly IStoreContext _storeContext;

        private readonly CustomerSettings _customerSettings;

        public NewsletterController(
            IWorkContext workContext,
			INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkflowMessageService workflowMessageService,
			CustomerSettings customerSettings,
			IStoreContext storeContext)
        {
            this._workContext = workContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workflowMessageService = workflowMessageService;
            this._customerSettings = customerSettings;
			this._storeContext = storeContext;
        }

        [ChildActionOnly]
        public ActionResult NewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");

            return PartialView(new NewsletterBoxModel());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Subscribe(bool subscribe, string email)
        {
            string result;
            var success = false;

			if (!email.IsEmail())
			{
				result = T("Newsletter.Email.Wrong");
			}
			else
			{
				//subscribe/unsubscribe
				email = email.Trim();

				var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(email, _storeContext.CurrentStore.Id);
				if (subscription != null)
				{
					if (subscribe)
					{
						if (!subscription.Active)
						{
							_workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
						}
						result = T("Newsletter.SubscribeEmailSent");
					}
					else
					{
						if (subscription.Active)
						{
							_workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
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
						StoreId = _storeContext.CurrentStore.Id
					};

					_newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
					_workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

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
