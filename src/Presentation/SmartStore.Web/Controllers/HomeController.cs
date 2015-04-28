﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Seo;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Topics;

namespace SmartStore.Web.Controllers
{
    public partial class HomeController : PublicControllerBase
	{
		#region Fields

		private readonly ICommonServices _services;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ITopicService> _topicService;
		private readonly Lazy<IQueuedEmailService> _queuedEmailService;
		private readonly Lazy<IEmailAccountService> _emailAccountService;
		private readonly Lazy<ISitemapGenerator> _sitemapGenerator;
		private readonly Lazy<CaptchaSettings> _captchaSettings;
		private readonly Lazy<CommonSettings> _commonSettings;

		#endregion

		#region Constructors

		public HomeController(
			ICommonServices services,
			Lazy<ICategoryService> categoryService,
			Lazy<IProductService> productService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ITopicService> topicService,
			Lazy<IQueuedEmailService> queuedEmailService,
			Lazy<IEmailAccountService> emailAccountService,
			Lazy<ISitemapGenerator> sitemapGenerator,
			Lazy<CaptchaSettings> captchaSettings,
			Lazy<CommonSettings> commonSettings)
        {
			this._services = services;
			this._categoryService = categoryService;
			this._productService = productService;
			this._manufacturerService = manufacturerService;
			this._topicService = topicService;
			this._queuedEmailService = queuedEmailService;
			this._emailAccountService = emailAccountService;
			this._sitemapGenerator = sitemapGenerator;
			this._captchaSettings = captchaSettings;
			this._commonSettings = commonSettings;

			T = NullLocalizer.Instance;
        }
        
        #endregion

		public Localizer T { get; set; }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Index()
        {
			return View();
        }


        [ChildActionOnly]
        public ActionResult ContentSlider()
        {
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			var settings = _services.Settings.LoadSetting<ContentSliderSettings>();

            settings.BackgroundPictureUrl = pictureService.GetPictureUrl(settings.BackgroundPictureId, 0, false);

            var slides = settings.Slides
				.Where(s => 
					s.LanguageCulture == _services.WorkContext.WorkingLanguage.LanguageCulture && 
					(!s.LimitedToStores || (s.SelectedStoreIds != null && s.SelectedStoreIds.Contains(_services.StoreContext.CurrentStore.Id)))
				)
				.OrderBy(s => s.DisplayOrder);
            
            foreach (var slide in slides)
            {
                slide.PictureUrl = pictureService.GetPictureUrl(slide.PictureId, 0, false);
                slide.Button1.Url = CheckButtonUrl(slide.Button1.Url);
                slide.Button2.Url = CheckButtonUrl(slide.Button2.Url);
                slide.Button3.Url = CheckButtonUrl(slide.Button3.Url);
            }

            settings.Slides = slides.ToList();

            return PartialView(settings);
        }

		public ActionResult StoreClosed()
		{
			return View();
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult ContactUs()
		{
			var model = new ContactUsModel()
			{
				Email = _services.WorkContext.CurrentCustomer.Email,
				FullName = _services.WorkContext.CurrentCustomer.GetFullName(),
				DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage
			};

			return View(model);
		}

		[HttpPost, ActionName("ContactUs")]
		[CaptchaValidator]
		public ActionResult ContactUsSend(ContactUsModel model, bool captchaValid)
		{
			//validate CAPTCHA
			if (_captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			if (ModelState.IsValid)
			{
				string email = model.Email.Trim();
				string fullName = model.FullName;
				string subject = T("ContactUs.EmailSubject", _services.StoreContext.CurrentStore.Name);

				var emailAccount = _emailAccountService.Value.GetEmailAccountById(EngineContext.Current.Resolve<EmailAccountSettings>().DefaultEmailAccountId);
				if (emailAccount == null)
					emailAccount = _emailAccountService.Value.GetAllEmailAccounts().FirstOrDefault();

				string from = null;
				string fromName = null;
				string body = Core.Html.HtmlUtils.FormatText(model.Enquiry, false, true, false, false, false, false);
				//required for some SMTP servers
				if (_commonSettings.Value.UseSystemEmailForContactUsForm)
				{
					from = emailAccount.Email;
					fromName = emailAccount.DisplayName;
					body = string.Format("<strong>From</strong>: {0} - {1}<br /><br />{2}",
						Server.HtmlEncode(fullName),
						Server.HtmlEncode(email), body);
				}
				else
				{
					from = email;
					fromName = fullName;
				}
				_queuedEmailService.Value.InsertQueuedEmail(new QueuedEmail
				{
					From = from,
					FromName = fromName,
					To = emailAccount.Email,
					ToName = emailAccount.DisplayName,
					Priority = 5,
					Subject = subject,
					Body = body,
					CreatedOnUtc = DateTime.UtcNow,
					EmailAccountId = emailAccount.Id,
					ReplyTo = email,
					ReplyToName = fullName
				});

				model.SuccessfullySent = true;
				model.Result = T("ContactUs.YourEnquiryHasBeenSent");

				//activity log
				_services.CustomerActivity.InsertActivity("PublicStore.ContactUs", T("ActivityLog.PublicStore.ContactUs"));

				return View(model);
			}

			model.DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage;
			return View(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult SitemapSeo()
		{
			if (!_commonSettings.Value.SitemapEnabled)
				return HttpNotFound();

			var roleIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
			string cacheKey = ModelCacheEventConsumer.SITEMAP_XML_MODEL_KEY.FormatInvariant(_services.WorkContext.WorkingLanguage.Id, string.Join(",", roleIds), _services.StoreContext.CurrentStore.Id);
			var sitemap = _services.Cache.Get(cacheKey, () =>
			{
				return _sitemapGenerator.Value.Generate(this.Url);
			}, 120);

			return Content(sitemap, "text/xml");
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult Sitemap()
		{
			if (!_commonSettings.Value.SitemapEnabled)
				return HttpNotFound();

			var roleIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
			string cacheKey = ModelCacheEventConsumer.SITEMAP_PAGE_MODEL_KEY.FormatInvariant(_services.WorkContext.WorkingLanguage.Id, string.Join(",", roleIds), _services.StoreContext.CurrentStore.Id);

			var result = _services.Cache.Get(cacheKey, () =>
			{
				var model = new SitemapModel();
				if (_commonSettings.Value.SitemapIncludeCategories)
				{
					var categories = _categoryService.Value.GetAllCategories();
					model.Categories = categories.Select(x => x.ToModel()).ToList();
				}

				if (_commonSettings.Value.SitemapIncludeManufacturers)
				{
					var manufacturers = _manufacturerService.Value.GetAllManufacturers();
					model.Manufacturers = manufacturers.Select(x => x.ToModel()).ToList();
				}

				if (_commonSettings.Value.SitemapIncludeProducts)
				{
					//limit product to 200 until paging is supported on this page
					IList<int> filterableSpecificationAttributeOptionIds = null;

					var productSearchContext = new ProductSearchContext();

					productSearchContext.OrderBy = ProductSortingEnum.Position;
					productSearchContext.PageSize = 200;
					productSearchContext.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
					productSearchContext.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
					productSearchContext.VisibleIndividuallyOnly = true;

					var products = _productService.Value.SearchProducts(productSearchContext);

					model.Products = products.Select(product => new ProductOverviewModel()
					{
						Id = product.Id,
						Name = product.GetLocalized(x => x.Name).EmptyNull(),
						ShortDescription = product.GetLocalized(x => x.ShortDescription),
						FullDescription = product.GetLocalized(x => x.FullDescription),
						SeName = product.GetSeName(),
					}).ToList();
				}
				if (_commonSettings.Value.SitemapIncludeTopics)
				{
					var topics = _topicService.Value.GetAllTopics(_services.StoreContext.CurrentStore.Id)
						 .ToList()
						 .FindAll(t => t.IncludeInSitemap);

					model.Topics = topics.Select(topic => new TopicModel()
					{
						Id = topic.Id,
						SystemName = topic.SystemName,
						IncludeInSitemap = topic.IncludeInSitemap,
						IsPasswordProtected = topic.IsPasswordProtected,
						Title = topic.GetLocalized(x => x.Title),
					})
					.ToList();
				}
				return model;
			});

			return View(result);
		}

        #region helper functions
        
        private string CheckButtonUrl(string url) 
        {
            if (!String.IsNullOrEmpty(url))
            {
				if (url.StartsWith("//") || url.StartsWith("/") || url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    //  //www.domain.de/dir
                    //  http://www.domain.de/dir
                    // nothing needs to be done
					return url;
                }
                else if (url.StartsWith("~/"))
                {
                    //  ~/directory
                    return Url.Content(url);
                }
                else
                {
                    //  directory
                    return Url.Content("~/" + url);
                }
            }

            return url.EmptyNull();
        }
        
        #endregion helper functions

    }
}
