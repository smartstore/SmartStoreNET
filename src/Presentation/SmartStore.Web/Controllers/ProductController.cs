using System;
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
    public partial class ProductController : PublicControllerBase
	{
		#region Fields

		private readonly ICommonServices _services;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IProductService> _productService;
		//private readonly Lazy<IManufacturerService> _manufacturerService;
		//private readonly Lazy<ITopicService> _topicService;
		//private readonly Lazy<IQueuedEmailService> _queuedEmailService;
		//private readonly Lazy<IEmailAccountService> _emailAccountService;
		//private readonly Lazy<ISitemapGenerator> _sitemapGenerator;
		//private readonly Lazy<CaptchaSettings> _captchaSettings;
		//private readonly Lazy<CommonSettings> _commonSettings;

		#endregion

		#region Constructors

		public ProductController(
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
			//this._manufacturerService = manufacturerService;
			//this._topicService = topicService;
			//this._queuedEmailService = queuedEmailService;
			//this._emailAccountService = emailAccountService;
			//this._sitemapGenerator = sitemapGenerator;
			//this._captchaSettings = captchaSettings;
			//this._commonSettings = commonSettings;

			T = NullLocalizer.Instance;
        }
        
        #endregion

		public Localizer T { get; set; }


		#region Products

		#endregion
	}
}
