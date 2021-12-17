using System;
using System.Web.Mvc;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Email;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Controllers
{
    public partial class HomeController : PublicControllerBase
    {
        private readonly Lazy<ITopicService> _topicService;
        private readonly Lazy<CaptchaSettings> _captchaSettings;
        private readonly Lazy<CommonSettings> _commonSettings;
        private readonly Lazy<PrivacySettings> _privacySettings;
        private readonly Lazy<HomePageSettings> _homePageSettings;
        private readonly Lazy<StoreInformationSettings> _storeInformationSettings;

        public HomeController(
            Lazy<ITopicService> topicService,
            Lazy<CaptchaSettings> captchaSettings,
            Lazy<CommonSettings> commonSettings,
            Lazy<PrivacySettings> privacySettings,
            Lazy<HomePageSettings> homePageSettings,
            Lazy<StoreInformationSettings> storeInformationSettings)
        {
            _topicService = topicService;
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _privacySettings = privacySettings;
            _homePageSettings = homePageSettings;
            _storeInformationSettings = storeInformationSettings;
        }

        [RewriteUrl(SslRequirement.No)]
        public ActionResult Index()
        {
            var storeId = Services.StoreContext.CurrentStore.Id;
            ViewBag.MetaTitle = _homePageSettings.Value.GetLocalizedSetting(x => x.MetaTitle, storeId);
            ViewBag.MetaDescription = _homePageSettings.Value.GetLocalizedSetting(x => x.MetaDescription, storeId);
            ViewBag.MetaKeywords = _homePageSettings.Value.GetLocalizedSetting(x => x.MetaKeywords, storeId);

            return View();
        }

        public ActionResult StoreClosed()
        {
            if (!_storeInformationSettings.Value.StoreClosed)
            {
                return RedirectToRoute("HomePage");
            }

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
        public ActionResult ContactUsSend(ContactUsModel model, string captchaError)
        {
            if (_captchaSettings.Value.ShowOnContactUsPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (ModelState.IsValid)
            {
                var customer = Services.WorkContext.CurrentCustomer;
                var email = model.Email.Trim();
                var fullName = model.FullName;
                var subject = T("ContactUs.EmailSubject", Services.StoreContext.CurrentStore.Name);
                var body = Core.Html.HtmlUtils.ConvertPlainTextToHtml(model.Enquiry.HtmlEncode());

                // Required for some SMTP servers.
                EmailAddress sender = null;
                if (!_commonSettings.Value.UseSystemEmailForContactUsForm)
                {
                    sender = new EmailAddress(email, fullName);
                }

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
