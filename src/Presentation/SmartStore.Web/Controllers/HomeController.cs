using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Email;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Framework.Filters;

namespace SmartStore.Web.Controllers
{
    public partial class HomeController : PublicControllerBase
	{
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ICatalogSearchService> _catalogSearchService;
		private readonly Lazy<CatalogHelper> _catalogHelper;
		private readonly Lazy<ITopicService> _topicService;
		private readonly Lazy<IXmlSitemapGenerator> _sitemapGenerator;
		private readonly Lazy<CaptchaSettings> _captchaSettings;
		private readonly Lazy<CommonSettings> _commonSettings;
		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<CustomerSettings> _customerSettings;
		private readonly Lazy<PrivacySettings> _privacySettings;

		public HomeController(
			Lazy<ICategoryService> categoryService,
			Lazy<IProductService> productService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ICatalogSearchService> catalogSearchService,
			Lazy<CatalogHelper> catalogHelper,
			Lazy<ITopicService> topicService,
			Lazy<IXmlSitemapGenerator> sitemapGenerator,
			Lazy<CaptchaSettings> captchaSettings,
			Lazy<CommonSettings> commonSettings,
			Lazy<SeoSettings> seoSettings,
			Lazy<CustomerSettings> customerSettings,
			Lazy<PrivacySettings> privacySettings)
        {
			_categoryService = categoryService;
			_productService = productService;
			_manufacturerService = manufacturerService;
			_catalogSearchService = catalogSearchService;
			_catalogHelper = catalogHelper;
			_topicService = topicService;
			_sitemapGenerator = sitemapGenerator;
			_captchaSettings = captchaSettings;
			_commonSettings = commonSettings;
			_seoSettings = seoSettings;
            _customerSettings = customerSettings;
			_privacySettings = privacySettings;
		}
		
        [RequireHttpsByConfig(SslRequirement.No)]
        public ActionResult Index()
        {
			return View();
        }

		public ActionResult StoreClosed()
		{
			return View();
		}

		[RequireHttpsByConfig(SslRequirement.No)]
		[GdprConsent]
		public ActionResult ContactUs()
		{
            var topic = _topicService.Value.GetTopicBySystemName("ContactUs", 0, false);

            var model = new ContactUsModel
			{
				Email = Services.WorkContext.CurrentCustomer.Email,
				FullName = Services.WorkContext.CurrentCustomer.GetFullName(),
				FullNameRequired = _privacySettings.Value.FullNameOnContactUsRequired,
				DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage,
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
			if (_captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			if (ModelState.IsValid)
			{
				var customer = Services.WorkContext.CurrentCustomer;
				var email = model.Email.Trim();
				var fullName = model.FullName;
				var subject = T("ContactUs.EmailSubject", Services.StoreContext.CurrentStore.Name);
				var body = Core.Html.HtmlUtils.FormatText(model.Enquiry, false, true, false, false, false, false);

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

			model.DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage;
			return View(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult SitemapSeo(int? index = null)
		{
			if (!_seoSettings.Value.XmlSitemapEnabled)
				return HttpNotFound();
			
			string content = _sitemapGenerator.Value.GetSitemap(index);

			if (content == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sitemap index is out of range.");
			}

			return Content(content, "text/xml", Encoding.UTF8);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult Sitemap()
		{
            return RedirectPermanent(Services.StoreContext.CurrentStore.Url);
		}
    }
}
