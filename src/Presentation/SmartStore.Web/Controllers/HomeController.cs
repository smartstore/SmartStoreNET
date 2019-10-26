﻿using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Email;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Controllers
{
	public partial class HomeController : PublicControllerBase
	{
		private readonly Lazy<ITopicService> _topicService;
		private readonly Lazy<CaptchaSettings> _captchaSettings;
		private readonly Lazy<CommonSettings> _commonSettings;
		private readonly Lazy<PrivacySettings> _privacySettings;

		public HomeController(
			Lazy<ITopicService> topicService,
			Lazy<CaptchaSettings> captchaSettings,
			Lazy<CommonSettings> commonSettings,
			Lazy<PrivacySettings> privacySettings)
        {
			_topicService = topicService;
			_captchaSettings = captchaSettings;
			_commonSettings = commonSettings;
			_privacySettings = privacySettings;
		}
		
        [RewriteUrl(SslRequirement.No)]
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult StoreClosed()
		{
			return View();
		}

		[RewriteUrl(SslRequirement.No)]
		[GdprConsent]
		public ActionResult ContactUs()
		{
			var topic = _topicService.Value.GetTopicBySystemName("ContactUs", 0, false);

            var model = new ContactUsModel
			{
				Email = Services.WorkContext.CurrentCustomer.Email,
				FullName = Services.WorkContext.CurrentCustomer.GetFullName(),
				FullNameRequired = _privacySettings.Value.FullNameOnContactUsRequired,
				DisplayCaptcha = _captchaSettings.Value.CanDisplayCaptcha && _captchaSettings.Value.ShowOnContactUsPage,
                MetaKeywords = topic?.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic?.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic?.GetLocalized(x => x.MetaTitle),
            };

			return View(model);
		}

		[HttpPost, ActionName("ContactUs")]
		[ValidateCaptcha, ValidateHoneypot]
		[GdprConsent]
		public ActionResult ContactUsSend(ContactUsModel model, bool captchaValid)
		{
			// Validate CAPTCHA
			if (_captchaSettings.Value.CanDisplayCaptcha && _captchaSettings.Value.ShowOnContactUsPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			if (ModelState.IsValid)
			{
				var customer = Services.WorkContext.CurrentCustomer;
				var email = model.Email.Trim();
				var fullName = model.FullName;
				var subject = T("ContactUs.EmailSubject", Services.StoreContext.CurrentStore.Name);
				var body = Core.Html.HtmlUtils.ConvertPlainTextToHtml(model.Enquiry.HtmlEncode());

				// Required for some SMTP servers
				EmailAddress sender = null;
				if (!_commonSettings.Value.UseSystemEmailForContactUsForm)
				{
					sender = new EmailAddress(email, fullName);
				}

				// email
				var msg = Services.MessageFactory.SendContactUsMessage(customer, email, fullName, subject, body, sender);

				if (msg?.Email?.Id != null)
				{
					model.SuccessfullySent = true;
					model.Result = T("ContactUs.YourEnquiryHasBeenSent");
					Services.CustomerActivity.InsertActivity("PublicStore.ContactUs", T("ActivityLog.PublicStore.ContactUs"));
				}
				else
				{
					ModelState.AddModelError("", T("Common.Error.SendMail"));
					model.Result = T("Common.Error.SendMail");
				}

				return View(model);
			}

			model.DisplayCaptcha = _captchaSettings.Value.CanDisplayCaptcha && _captchaSettings.Value.ShowOnContactUsPage;

			return View(model);
		}

		[RewriteUrl(SslRequirement.No)]
		public ActionResult Sitemap()
		{
            return RedirectPermanent(Services.StoreContext.CurrentStore.Url);
		}
    }
}
