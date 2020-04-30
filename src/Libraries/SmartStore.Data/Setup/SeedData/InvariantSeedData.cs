using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
	{
		private SmartObjectContext _ctx;
		private string _sampleImagesPath;

		protected InvariantSeedData()
		{
		}

		public void Initialize(SmartObjectContext context)
		{
			_ctx = context;
			_sampleImagesPath = CommonHelper.MapPath("~/App_Data/Samples/");
		}

		#region Mandatory data creators

		public IList<MediaFile> Pictures()
		{
			var entities = new List<MediaFile>
			{
				CreatePicture("company-logo.png"),
				CreatePicture("product/allstar_charcoal.jpg"),
				CreatePicture("product/allstar_maroon.jpg"),
				CreatePicture("product/allstar_navy.jpg"),
				CreatePicture("product/allstar_purple.jpg"),
				CreatePicture("product/allstar_white.jpg"),
				CreatePicture("product/wayfarer_havana.png"),
				CreatePicture("product/wayfarer_havana_black.png"),
				CreatePicture("product/wayfarer_rayban-black.png")
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Store> Stores()
		{
			var imgCompanyLogo = _ctx.Set<MediaFile>().Where(x => x.Name == "company-logo.png").FirstOrDefault();
			
			var currency = _ctx.Set<Currency>().FirstOrDefault(x => x.CurrencyCode == "EUR");
			if (currency == null)
				currency = _ctx.Set<Currency>().First();
			
			var entities = new List<Store>()
			{
				new Store()
				{
					Name = "فروشگاه اول",
					Url = "http://www.yourStore.com/",
					Hosts = "yourstore.com,www.yourstore.com",
					SslEnabled = false,
					DisplayOrder = 1,
					LogoMediaFileId = imgCompanyLogo.Id,
					PrimaryStoreCurrencyId = currency.Id,
					PrimaryExchangeRateCurrencyId = currency.Id
				}
			};
			this.Alter(entities);
			return entities;
		}

		public IList<MeasureDimension> MeasureDimensions()
		{
			var entities = new List<MeasureDimension>()
			{
				new MeasureDimension()
				{
					Name = "میلی‌متر",
					SystemKeyword = "mm",
					Ratio = 25.4M,
					DisplayOrder = 1,
				},
				new MeasureDimension()
				{
					Name = "سانتی‌متر",
					SystemKeyword = "cm",
					Ratio = 0.254M,
					DisplayOrder = 2,
				},
				new MeasureDimension()
				{
					Name = "متر",
					SystemKeyword = "m",
					Ratio = 0.0254M,
					DisplayOrder = 3,
				},
				new MeasureDimension()
				{
					Name = "اینچ",
					SystemKeyword = "inch",
					Ratio = 1M,
					DisplayOrder = 4,
				},
				new MeasureDimension()
				{
					Name = "فوت",
					SystemKeyword = "ft",
					Ratio = 0.08333333M,
					DisplayOrder = 5,
				}
			};

			this.Alter(entities);
			return entities;
		}

		public IList<MeasureWeight> MeasureWeights()
		{
			var entities = new List<MeasureWeight>()
			{
				new MeasureWeight()
				{
					Name = "اونس", // Ounce, Unze
					SystemKeyword = "oz",
					Ratio = 16M,
					DisplayOrder = 5,
				},
				new MeasureWeight()
				{
					Name = "پوند", // Pound
					SystemKeyword = "lb",
					Ratio = 1M,
					DisplayOrder = 6,
				},

				new MeasureWeight()
				{
					Name = "کیلوگرم",
					SystemKeyword = "kg",
					Ratio = 0.45359237M,
					DisplayOrder = 1,
				},
				new MeasureWeight()
				{
					Name = "گرم",
					SystemKeyword = "g",
					Ratio = 453.59237M,
					DisplayOrder = 2,
				},
				new MeasureWeight()
				{
					Name = "لیتر",
					SystemKeyword = "l",
					Ratio = 0.45359237M,
					DisplayOrder = 3,
				},
				new MeasureWeight()
				{
					Name = "میلی‌لیتر",
					SystemKeyword = "ml",
					Ratio = 0.45359237M,
					DisplayOrder = 4,
				}
			};

			this.Alter(entities);
			return entities;
		}

		protected virtual string TaxNameBooks
		{
			get => "کتاب";
		}

		protected virtual string TaxNameDigitalGoods
		{
			get => "محصول قابل دانلود";
		}

		protected virtual string TaxNameJewelry
		{
			get => "جواهرات";
		}

		protected virtual string TaxNameApparel
		{
			get => "پوشاک و کفش";
		}

		protected virtual string TaxNameFood
		{
			get => "غذا";
		}

		protected virtual string TaxNameElectronics
		{
			get => "نرم افزار";
		}

		protected virtual string TaxNameTaxFree
		{
			get => "بدون مالیات";
		}

		public virtual decimal[] FixedTaxRates
		{
			get => new decimal[] { 0, 0, 0, 0, 0 };
		}

		public IList<TaxCategory> TaxCategories()
		{
			var entities = new List<TaxCategory>
			{
				new TaxCategory
				{
					Name = this.TaxNameBooks,
					DisplayOrder = 1,
				},
				new TaxCategory
				{
					Name = this.TaxNameElectronics,
					DisplayOrder = 5,
				},
				new TaxCategory
				{
					Name = this.TaxNameDigitalGoods,
					DisplayOrder = 10,
				},
				new TaxCategory
				{
					Name = this.TaxNameJewelry,
					DisplayOrder = 15,
				},
				new TaxCategory
				{
					Name = this.TaxNameApparel,
					DisplayOrder = 20,
				},
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Currency> Currencies()
		{
			var entities = new List<Currency>()
			{
				CreateCurrency("fa-IR", published: true, rate: 1M, order: 1, formatting: "#,##0 تومان"),
				CreateCurrency("en-US", published: false, rate: 1M, order: 2)

				//CreateCurrency("en-AU", published: true, rate: 0.94M, order: 10),
				//CreateCurrency("en-CA", published: true, rate: 0.98M, order: 15),
				//CreateCurrency("de-DE", rate: 0.79M, order: 20/*, formatting: string.Format("0.00 {0}", "\u20ac")*/),
				//CreateCurrency("de-CH", rate: 0.93M, order: 25, formatting: "CHF #,##0.00"),
				//CreateCurrency("zh-CN", rate: 6.48M, order: 30),
				//CreateCurrency("zh-HK", rate: 7.75M, order: 35),
				//CreateCurrency("ja-JP", rate: 80.07M, order: 40),
				//CreateCurrency("ru-RU", rate: 27.7M, order: 45),
				//CreateCurrency("tr-TR", rate: 1.78M, order: 50),
				//CreateCurrency("sv-SE", rate: 6.19M, order: 55)
			};

			this.Alter(entities);
			return entities;
		}

		public IList<ShippingMethod> ShippingMethods()
		{
			var entities = new List<ShippingMethod>()
			{
				new ShippingMethod
					{
						Name = "حضور در فروشگاه",
						Description ="سفارش خود را در محل فروشگاه دریافت نمایید.",
						DisplayOrder = 0
					},
				new ShippingMethod
					{
						Name = "ارسال با پیک",
						Description ="سفارش شما با پیک ارسال خواهد شد.",
						DisplayOrder = 1
					},
			};
			this.Alter(entities);
			return entities;
		}

		public IList<CustomerRole> CustomerRoles()
		{
			var crAdministrators = new CustomerRole
			{
				Name = "مدیر",
				Active = true,
				IsSystemRole = true,
				SystemName = SystemCustomerRoleNames.Administrators,
			};
			var crForumModerators = new CustomerRole
			{
				Name = "نویسنده",
				Active = true,
				IsSystemRole = true,
				SystemName = SystemCustomerRoleNames.ForumModerators,
			};
			var crRegistered = new CustomerRole
			{
				Name = "عضو",
				Active = true,
				IsSystemRole = true,
				SystemName = SystemCustomerRoleNames.Registered,
			};
			var crGuests = new CustomerRole
			{
				Name = "مهمان",
				Active = true,
				IsSystemRole = true,
				SystemName = SystemCustomerRoleNames.Guests,
			};
			var entities = new List<CustomerRole>
			{
				crAdministrators,
				crForumModerators,
				crRegistered,
				crGuests
			};
			this.Alter(entities);
			return entities;
		}

		public Address AdminAddress()
		{
            var cCountry = _ctx.Set<Country>()
                .Where(x => x.ThreeLetterIsoCode == "IRN")
                .FirstOrDefault();

			var entity = new Address()
			{
				FirstName = "محمد",
				LastName = "رضایی",
				PhoneNumber = "09123456789",
				Email = "admin@myshop.com",
				FaxNumber = "",
				Company = "وبتینا",
				Address1 = "خیابان اردیبهشت، کوچه نسترن",
				Address2 = "",
				City = "تهران",
				StateProvince = cCountry.StateProvinces.FirstOrDefault(),
				Country = cCountry,
				ZipPostalCode = "88818-15222",
				CreatedOnUtc = DateTime.UtcNow,
			};
			this.Alter(entity);
			return entity;
		}

		public Customer SearchEngineUser()
		{
			var entity = new Customer
			{
				Email = "builtin@search-engine-record.com",
				CustomerGuid = Guid.NewGuid(),
				PasswordFormat = PasswordFormat.Clear,
				AdminComment = "Built-in system guest record used for requests from search engines.",
				Active = true,
				IsSystemAccount = true,
				SystemName = SystemCustomerNames.SearchEngine,
				CreatedOnUtc = DateTime.UtcNow,
				LastActivityDateUtc = DateTime.UtcNow,
			};

			this.Alter(entity);
			return entity;
		}

		public Customer BackgroundTaskUser()
		{
			var entity = new Customer
			{
				Email = "builtin@background-task-record.com",
				CustomerGuid = Guid.NewGuid(),
				PasswordFormat = PasswordFormat.Clear,
				AdminComment = "Built-in system record used for background tasks.",
				Active = true,
				IsSystemAccount = true,
				SystemName = SystemCustomerNames.BackgroundTask,
				CreatedOnUtc = DateTime.UtcNow,
				LastActivityDateUtc = DateTime.UtcNow,
			};

			this.Alter(entity);
			return entity;
		}

		public Customer PdfConverterUser()
		{
			var entity = new Customer
			{
				Email = "builtin@pdf-converter-record.com",
				CustomerGuid = Guid.NewGuid(),
				PasswordFormat = PasswordFormat.Clear,
				AdminComment = "Built-in system record used for the PDF converter.",
				Active = true,
				IsSystemAccount = true,
				SystemName = SystemCustomerNames.PdfConverter,
				CreatedOnUtc = DateTime.UtcNow,
				LastActivityDateUtc = DateTime.UtcNow,
			};

			this.Alter(entity);
			return entity;
		}

		public IList<EmailAccount> EmailAccounts()
		{
			var entities = new List<EmailAccount>()
			{
				new EmailAccount
				{
					Email = "test@mail.com",
					DisplayName = "ایمیل",
					Host = "smtp.mail.com",
					Port = 25,
					Username = "123",
					Password = "123",
					EnableSsl = false,
					UseDefaultCredentials = false
				}
			};
			this.Alter(entities);
			return entities;
		}

		public IList<Topic> Topics()
		{
			var entities = new List<Topic>()
			{
				new Topic
					{
						SystemName = "AboutUs",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "درباره ما",
						Body = "<p>هر وبسایت کسب و کاری باید یک صفحه درباره ما داشته باشد. بخش «درباره ما» وبسایت شما مکانی برای بازدیدکنندگان‌تان است که در مورد آنچه که شما چه کسی هستید؟ چه کاری انجام می‌دهید؟، و آنچه که باعث شده کسب و کار خود را راه‌اندازی کنید، اطلاعات کسب می‌کنند. اینکه آیا در حال ایجاد یک وبسایت جدید برای شرکت هستید و یا می‌خواهید وبسایت موجود را تغییر دهید، باید مقداری از زمان خود را بر روی صفحه درباره ما بگذارید. راه‌های بسیاری برای بهبود آن وجود دارد. اهمیت صفحه درباره ما از آنجایی بیشتر می‌شود که بخواهیم یک بازدیدکننده، به مشتری تبدیل شود.</p>"
					},
				new Topic
					{
						SystemName = "CheckoutAsGuestOrRegister",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
                        RenderAsWidget = true,
                        WidgetWrapContent = false,
						Title = "",
						Body = "<p><strong>عضو شوید و از مزیت های آن بهره‌مند گردید!</strong><br /></p><ul><li>ثبت سریع و آسان سفارش</li><li>دسترسی آسان و ساده به لیست سفارشات قبلی</li><li>اطلاع از تخفیف‌ها و فروش ویژه</li></ul>"
					}, 
				new Topic
					{
						SystemName = "ConditionsOfUse",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "شرایط استفاده",
						Body = "<p>شرایط استفاده از وبسایت بدین شرح است: ....</p>"
					},
				new Topic
					{
						SystemName = "ContactUs",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
                        RenderAsWidget = true,
                        WidgetWrapContent = false,
                        Title = "تماس با ما",
						Body = "<p>کارشناسان مرکز شبانه روزی ارتباط با، ۲۴ ساعت شبانه روز، ۷ روز هفته و ۳۶۵ روز سال، پاسخ‌گوی پرسش‌ها و مشکلات احتمالی مشتریان هستند.</p>"
					},
				new Topic
					{
						SystemName = "ForumWelcomeMessage",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
                        RenderAsWidget = true,
                        WidgetWrapContent = false,
                        Title = "فروم",
						Body = "<p>..::خوش آمدید::..</p>"
					},
				new Topic
					{
						SystemName = "HomePageText",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
                        RenderAsWidget = true,
                        WidgetWrapContent = false,
                        Title = "به فروشگاه ما خوش آمدید",
						Body = "<p>برای اطلاع از تخفیف ها و فروش ویژه در سایت عضو شوید.</p></p>"
					},
				new Topic
					{
						SystemName = "LoginRegistrationInfo",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
                        RenderAsWidget = true,
                        WidgetWrapContent = false,
                        Title = "راهنمای عضویت/ورود",
						Body = "<p><strong>هنوز عضو نیستید؟?</strong></p><p>حساب خود را ایجاد کنید و تنوع ما را تجربه کنید. با یک حساب کاربری می توانید سفارشات را سریعتر انجام دهید و همیشه نمای کاملی از سفارشات فعلی و قبلی خود خواهید داشت.</p>"
					},
				new Topic
					{
						SystemName = "PrivacyInfo",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						ShortTitle = "Privacy",
						Title = "اطلاعات حریم خصوصی",
						Body = "<p><strong>فروشگاه ما</strong> به اطلاعات خصوصی اشخاصى که از خدمات سایت استفاده می‏‌کنند، احترام گذاشته و از آن محافظت می‏‌کند و متعهد می‏‌شود در حد توان از حریم شخصی شما دفاع کند و در این راستا، تکنولوژی مورد نیاز برای هرچه مطمئن‏‌تر و امن‏‌تر شدن استفاده شما از سایت را، توسعه دهد.</p>"
					},
				new Topic
					{
						SystemName = "ShippingInfo",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "ارسال",
						Body = "<p>شرایط ارسالو بازگشت کالا به شرح زیر است: ...</p>"
					},

				new Topic
					{
						SystemName = "Imprint",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "چاپ",
						Body = @"<p>Put your imprint information here. YOu can edit this in the admin site.</p>"
					},
				new Topic
					{
						SystemName = "Disclaimer",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "سلب مسئولیت",
						Body = "<p>ضمن تشکر از انتخاب این سایت ، از آنجا که استفاده از این سایت به هر شکل به معنای قبول شرایط مندرج در بخش قوانین است، تقاضا می شود آن را به دقت مطالعه فرمایید..</p>"
					},
				new Topic
					{
						SystemName = "PaymentInfo",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "اطلاعات پرداخت",
						Body = "<p>در صورت پرداخت آنلاین از طریق حساب کاربری و اتصال به درگاه بانک سامان و یا پارسیان از طریق حساب کاربری، نیازی به تکمیل فرم ثبت اطلاعات پرداخت ندارید و همان لجظه پس از پرداخت، حساب کاربری شما شارژ شده و یا در صورت موجود بودن صورتحساب، صورتحساب شما پرداخت می شود.</p>"
					},
			};
			this.Alter(entities);
			return entities;
		}

		public IList<ISettings> Settings()
		{
            var defaultDimensionId = _ctx.Set<MeasureDimension>().FirstOrDefault(x => x.SystemKeyword == "inch")?.Id ?? 0;
            var defaultWeightId = _ctx.Set<MeasureWeight>().FirstOrDefault(x => x.SystemKeyword == "lb")?.Id ?? 0;
            var defaultLanguageId = _ctx.Set<Language>().FirstOrDefault()?.Id ?? 0;
            var defaultEmailAccountId = _ctx.Set<EmailAccount>().FirstOrDefault()?.Id ?? 0;

			var entities = new List<ISettings>
			{
				new PdfSettings
				{
				},
				new CommonSettings
				{
				},
				new SeoSettings
				{
				},
				new SocialSettings
				{
				},
				new AdminAreaSettings
				{
				},
				new CatalogSettings
				{
				},
				new LocalizationSettings
				{
					DefaultAdminLanguageId = defaultLanguageId
				},
				new CustomerSettings
				{
				},
				new AddressSettings
				{
				},
				new MediaSettings
				{
				},
				new StoreInformationSettings
				{
				},
				new RewardPointsSettings
				{
				},
				new CurrencySettings
				{
				},
				new MeasureSettings
				{
					BaseDimensionId = defaultDimensionId,
					BaseWeightId = defaultWeightId,
				},
				new ShoppingCartSettings
				{
				},
				new OrderSettings
				{
				},
				new SecuritySettings
				{
				},
				new ShippingSettings
				{
				},
				new PaymentSettings
				{
					ActivePaymentMethodSystemNames = new List<string>
					{
						"Payments.CashOnDelivery",
						"Payments.Manual",
						"Payments.PayInStore",
						"Payments.Prepayment"
					}
				},
				new TaxSettings
				{
				},
				new BlogSettings
				{
				},
				new NewsSettings
				{
				},
				new ForumSettings
				{
				},
				new EmailAccountSettings
				{
					DefaultEmailAccountId = defaultEmailAccountId
				},
				new ThemeSettings
				{
				}
			};

			this.Alter(entities);
			return entities;
		}

		public IList<ProductTemplate> ProductTemplates()
		{
			var entities = new List<ProductTemplate>
							   {
									new ProductTemplate
									{
										Name = "قالب پیشفرض محصول",
										ViewPath = "Product",
										DisplayOrder = 10
									}
							   };
			this.Alter(entities);
			return entities;
		}

		public IList<CategoryTemplate> CategoryTemplates()
		{
			var entities = new List<CategoryTemplate>
							   {
								   new CategoryTemplate
									   {
										   Name = "گرید محصولات",
										   ViewPath = "CategoryTemplate.ProductsInGridOrLines",
										   DisplayOrder = 1
									   },
							   };
			this.Alter(entities);
			return entities;
		}

		public IList<ManufacturerTemplate> ManufacturerTemplates()
		{
			var entities = new List<ManufacturerTemplate>
							   {
								   new ManufacturerTemplate
									   {
										   Name = "گرید محصولات",
										   ViewPath = "ManufacturerTemplate.ProductsInGridOrLines",
										   DisplayOrder = 1
									   },
							   };
			this.Alter(entities);
			return entities;
		}

		public IList<ScheduleTask> ScheduleTasks()
		{
			var entities = new List<ScheduleTask>
			{
				new ScheduleTask
				{
					Name = "Send emails",
					CronExpression = "* * * * *", // every Minute
					Type = "SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
                    Priority = TaskPriority.High
				},
				new ScheduleTask
				{
					Name = "Delete guests",
					CronExpression = "*/10 * * * *", // Every 10 minutes
					Type = "SmartStore.Services.Customers.DeleteGuestsTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Delete logs",
					CronExpression = "0 1 * * *", // At 01:00
					Type = "SmartStore.Services.Logging.DeleteLogsTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Clear cache",
					CronExpression = "0 */12 * * *", // Every 12 hours
					Type = "SmartStore.Services.Caching.ClearCacheTask, SmartStore.Services",
					Enabled = false,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Update currency exchange rates",
					CronExpression = "0 */6 * * *", // Every 6 hours
					Type = "SmartStore.Services.Directory.UpdateExchangeRateTask, SmartStore.Services",
					Enabled = false,
					StopOnError = false,
                    Priority = TaskPriority.High
                },
				new ScheduleTask
				{
					Name = "Clear transient uploads",
					CronExpression = "30 1,13 * * *", // At 01:30 and 13:30
					Type = "SmartStore.Services.Media.TransientMediaClearTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Clear email queue",
					CronExpression = "0 2 * * *", // At 02:00
					Type = "SmartStore.Services.Messages.QueuedMessagesClearTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Cleanup temporary files",
					CronExpression = "30 3 * * *", // At 03:30
					Type = "SmartStore.Services.Common.TempFileCleanupTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false
				},
                new ScheduleTask
                {
                    Name = "Rebuild XML Sitemap",
                    CronExpression = "45 3 * * *",
                    Type = "SmartStore.Services.Seo.RebuildXmlSitemapTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false
                }
            };
			this.Alter(entities);
			return entities;
		}

		#endregion

		#region Sample data creators

		public IList<ForumGroup> ForumGroups()
		{
			var forumGroupGeneral = new ForumGroup
			{
				Name = "General",
				Description = "",
				DisplayOrder = 1
			};

			var entities = new List<ForumGroup>
			{
				forumGroupGeneral
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Forum> Forums()
		{
            var group = _ctx.Set<ForumGroup>().FirstOrDefault(c => c.DisplayOrder == 1);

            var newProductsForum = new Forum
			{
				ForumGroup = group,
				Name = "محصولات درخواستی",
				Description = "بحث در مورد محصولاتی که تمایل دارید به فروشگاه اضافه شود.",
				NumTopics = 0,
				NumPosts = 0,
				LastPostCustomerId = 0,
				LastPostTime = null,
				DisplayOrder = 1
			};

			var packagingShippingForum = new Forum
			{
				ForumGroup = group,
				Name = "ارسال و بسته بندی",
				Description = "بحث در مورد ارسال و بسته بندی سفارشات",
				NumTopics = 0,
				NumPosts = 0,
				LastPostTime = null,
				DisplayOrder = 20
			};


			var entities = new List<Forum>
			{
				newProductsForum, packagingShippingForum
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Discount> Discounts()
		{
			var sampleDiscountWithCouponCode = new Discount()
				{
					Name = "کد تخفیف",
					DiscountType = DiscountType.AssignedToSkus,
					DiscountLimitation = DiscountLimitationType.Unlimited,
					UsePercentage = false,
					DiscountAmount = 10,
					RequiresCouponCode = true,
					CouponCode = "takhfif"
				};
			var sampleDiscounTwentyPercentTotal = new Discount()
				{
					Name = "20% تخفیف روی کل سفارش",
					DiscountType = DiscountType.AssignedToOrderTotal,
					DiscountLimitation = DiscountLimitationType.Unlimited,
					UsePercentage = true,
					DiscountPercentage = 20,
					StartDateUtc = new DateTime(2013, 1, 1),
					EndDateUtc = new DateTime(2020, 1, 1),
					RequiresCouponCode = true,
					CouponCode = "order20"
				};

			var entities = new List<Discount>
			{
				sampleDiscountWithCouponCode, sampleDiscounTwentyPercentTotal
			};

			this.Alter(entities);
			return entities;
		}

		public IList<DeliveryTime> DeliveryTimes()
		{
			var entities = new List<DeliveryTime>()
			{
				new DeliveryTime
					{
						Name = "آماده ارسال",
						DisplayOrder = 0,
						ColorHexValue = "#008000"
					},
				new DeliveryTime
					{
						Name = "2-5 روز کاری",
						DisplayOrder = 1,
						ColorHexValue = "#FFFF00"
					},
				new DeliveryTime
					{
						Name = "7 روز کاری",
						DisplayOrder = 2,
						ColorHexValue = "#FF9900"
					},
			};
			this.Alter(entities);
			return entities;
		}

        public IList<QuantityUnit> QuantityUnits()
        {
            var count = 0;
            var entities = new List<QuantityUnit>();

            var quPluralEn = new Dictionary<string, string>
            {
                { "Piece", "قطعه" },
                { "Box", "جعبه" },
                { "Parcel", "بسته" },
                { "Palette", "پالت" },
                { "Unit", "عدد" },
                { "Sack", "گونی" },
                { "Bag", "کیسه" },
                { "Can", "قوطی" },  
                { "Bottle", "بطری" },
                { "Glass", "شیشه" },
                { "Bunch", "دسته" }
            };

            foreach (var qu in quPluralEn)
            {
                entities.Add(new QuantityUnit
                {
                    Name = qu.Value,
                    NamePlural = qu.Value,
                    Description = qu.Value,
                    IsDefault = qu.Key == "Piece",
                    DisplayOrder = count++
                });
            }
            
            this.Alter(entities);
            return entities;
        }

		public IList<BlogPost> BlogPosts()
		{
			var defaultLanguage = _ctx.Set<Language>().FirstOrDefault();
			var blogPostDiscountCoupon = new BlogPost()
			{
				AllowComments = true,
				Language = defaultLanguage,
				Title = "اگر جف بزوس تمام بیت کوین موجود در جهان را بخرد چه می‌شود؟",
				Body = "<p>در حال حاضر تمام بیت کوین موجود در جهان حدود ۱۳۰ میلیارد دلار می‌شود. یعنی اگر جف بزوس به عنوان ثروتمندترین مرد جهان تمام دارایی ۱۴۰ میلیارد دلاری خود را بیت کوین بخرد، باز هم ۱۰ میلیارد دلار اضافه می‌آورد. اما اگر واقعا چنین اتفاقی رخ دهد چه می‌شود؟ در این مطلب مدیر سابق استخراج بیت کوین در شرکت Genesis Mining به سوال ما پاسخ می‌دهد.<br>آقای سالتر می‌گوید تا امروز حدود ۱۸ میلیون عدد بیت کوین استخراج شده است، اما باید در نظر گرفت که بخشی از این تعداد گم شده یا به هر دلیل دیگر مثل فوت مالک، در دسترس نیست. علاوه بر این همه‌ی بیت کوین‌هایی که تا امروز استخراج شده‌اند در بازار خرید و فروش نمی‌شوند و آن تعدادی که در پلتفرم‌های مختلف قابل خرید است فقط بخش کوچکی از کل بیت کوین‌های موجود در جهان را شامل می‌شود.<br>اگر جف بزوس تمام بیت‌کوین‌های دنیا را بخرد بازهم ۱۰ میلیارد دلار اضافه می‌آوردبخشی که برای خرید و فروش عرضه می‌شود را بیت کوین سیال می‌گویند، اما واقعا نمی‌توان گفت دقیقا چند عدد بیت کوین سیال وجود دارد. یک مشکل دیگر هم این است که اگر جف بزوس به خریدن تمام بیت کوین‌های سیال مشغول شود، هرچه جلوتر می‌رود با کمتر شدن موجودی باعث بالا رفتن قیمت می‌شود. وقتی قیمت بیت کوین بالا می‌رود، بسیاری از مالکان بیت کوین فروش را متوقف می‌کنند تا منتظر شوند بیت کوین به بالاترین قیمت خود برسد.</p>",
				Tags = "بیت کوین, بلکچین",
				CreatedOnUtc = DateTime.UtcNow,
			};
			var blogPostCustomerService = new BlogPost()
			{
				AllowComments = true,
				Language = defaultLanguage,
				Title = "آیا استفاده از پهپادهای حمل کالا در زمان قرنطینه ایده مناسبی است؟",
				Body = "<p>در ماه‌های اخیر که شیوع ویروس کرونا محدودیت‌هایی را برای روال طبیعی زندگی ما ایجاد کرده است، بسیاری از افراد به خریدهای آنلاین و استفاده از سرویس‌های پستی روی آورده‌اند. اما چرا سرویس‌های پستی کشور ما یا بسیاری از کشورهای دیگر همچنان از همان ساختار قدیمی تحویل بار توسط نیروی انسانی استفاده می‌کنند و فناوری‌هایی مانند پهپاد حمل کالا یا ربات‌های تحویل مرسوله استفاده نمی‌کنند؟<br>ایده نگارش این مطلب زمانی به ذهنم رسید که شنیدم در دورانی که بسیاری از ما داوطلبانه قرنطینه را انتخاب کرده‌ایم، فعالیت‌ سرویس‌های پستی افزایش یافته است. همچنین، در حالی که به دلیل جلوگیری از گسترش ویروس کرونا بسیاری از مشاغل با محدودیت یا منع فعالیت روبرو شده‌اند، ماموران اداره پست همچنان مشغول فعالیت هستند و حتی حجم کارشان نسبت به قبل از دوران قرنطینه بیشتر هم شده است.</p>",
				Tags = "فروشگاه, خرید اینترنتی, پهپاد",
				CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
			};

			var entities = new List<BlogPost>
			{
				blogPostDiscountCoupon, blogPostCustomerService
			};

			this.Alter(entities);
			return entities;
		}

		public IList<NewsItem> NewsItems()
		{
			var defaultLanguage = _ctx.Set<Language>().FirstOrDefault();
			var news1 = new NewsItem()
			{
				AllowComments = true,
				Language = defaultLanguage,
				Title = "انتشار رسمی ویدئوهای محرمانه پنتاگون مهر تأییدی بر وجود فرازمینی‌ها است؟",
                Short = "وزارت دفاع آمریکا به‌تازگی با انتشار سه ویدئو نشان‌دهنده‌ی اجسام پرنده‌ی ناشناس، اعتبار آن‌ها را تأیید کرده است؛ اما این اذعان رسمی به‌معنای تأیید وجود بیگانگان است؟",
                Full = "<p>روز دوشنبه، وزارت دفاع ایالات متحده رسما سه ویدئویی را منتشر کرد که مواجهه‌ی خلبانان نیروی دریایی را با اجسام پرنده‌ی ناشناس (UFO) به‌تصویر می‌کشد. این رویدادها در سال‌های ۲۰۰۴ و ۲۰۱۵ به‌وقوع پیوست؛ اما ویدئوها از دید عموم پنهان ماند تا اینکه در سال ۲۰۱۷، نیویورک‌تایمز آن‌ها را به‌همراه سرمقاله‌ای درباره‌ی برنامه‌ی اسرارآمیز یوفو پنتاگون منتشر کرد. نیروی دریایی ارتش آمریکا پیش‌تر اذعان کرد ویدئوها معتبر است؛ اما پنتاگون تا امروز، هرگز مجوز انتشار آن‌ها را صادر نکرد.<br>هر سه ویدئو حاوی تصاویر ثبت‌شده‌ی خلبانان نیروهای دریایی است که جسمی بیضی‌شکل را با ظاهر عجیب در حال پرواز درون هوا و برفراز اقیانوس نشان می‌دهد.در ویدئو ضبط‌شده در سال ۲۰۱۵ به‌نام گیمبال(Gimbal)، جسمی پرنده به‌شکل بیضی پیش از ایستادن و چرخیدن، با سرعت برفراز ابرها حرکت می‌کند.خلبان که در حال فیلم‌برداری از این مواجهه است، جسم پرنده را در بی‌سیم «پهپاد» توصیف می‌کند.</p>",
				Published = true,
                MetaTitle = "انتشار رسمی ویدئوهای محرمانه پنتاگون مهر تأییدی بر وجود فرازمینی‌ها است",
				CreatedOnUtc = DateTime.Now
			};
			var news2 = new NewsItem()
            {
                AllowComments = true,
                Language = defaultLanguage,
                Title = "VOD‌ها پنج ماه برای تقویت زیرساخت‌های نظارتی فرصت دارند!",
                Short = "VOD‌ها پنج ماه مهلت دارند امکانات نظارتی ازجمله رده‌بندی سنی فیلم‌ها و سریال‌های خارجی و ایرانی را در پلتفرم خود اجرا کنند.",
                Full = "<p>محتوایی که در سرویس‌های VOD و رسانه‌های کاربرمحور به‌اشتراک گذاشته می‌شود، همواره محل بحث بوده است؛ اما به‌نظر می‌رسد در روزهایی که استفاده‌ی مردم به‌دلیل قرنطینه‌ی خانگی از این سرویس‌ها بیشتر شده است، نظارت بر محتوای آن‌ها نیز بیش از گذشته اهمیت پیدا می‌کند. سازمان تنظیم مقررات صوت و تصویر فراگیر در فضای مجازی (ساترا) متولی نظارت بر عملکرد سامانه‌های انتشار محتوایی ویدئویی در فضای مجازی است.<br>محمدرسول حاج‌اقلی، سرپرست اداره‌ی کل صدور مجوز و امور رسانه‌های‌ ساترا، مسئولیت صدور مجوزها را برعهده دارد و در گفت‌وگو با مهر، ضمن پذیرفتن مشکلات نظارتی و محتوایی موجود در رسانه‌های مجازی، از مهلت پنج‌ماهه‌ برای رفع آن‌ها خبر داده است.او می‌گوید ساترا الزامی در تعیین نوع فعالیت رسانه‌ها ندارد و این موضوع را متقاضی دریافت مجوز تعیین می‌کند.</p>",
                Published = true,
                MetaTitle = "VOD‌ها پنج ماه برای تقویت زیرساخت‌های نظارتی فرصت دارند",
                CreatedOnUtc = DateTime.Now
            };

            var entities = new List<NewsItem>
			{
				news1, news2
			};

			this.Alter(entities);
			return entities;
		}

		public IList<PollAnswer> PollAnswers()
		{
			var pollAnswer1 = new PollAnswer()
			{
				Name = "عالی",
				DisplayOrder = 1,
			};
			var pollAnswer2 = new PollAnswer()
			{
				Name = "خوب",
				DisplayOrder = 2,
			};
			var pollAnswer3 = new PollAnswer()
			{
				Name = "بد",
				DisplayOrder = 3,
			};
			var pollAnswer4 = new PollAnswer()
			{
				Name = "خیلی بد",
				DisplayOrder = 4,
			};
			var pollAnswer5 = new PollAnswer()
			{
				Name = "روزانه",
				DisplayOrder = 5,
			};
			var pollAnswer6 = new PollAnswer()
			{
				Name = "هر هفته",
				DisplayOrder = 6,
			};
			var pollAnswer7 = new PollAnswer()
			{
				Name = "هر دوهفته",
				DisplayOrder = 7,
			};
			var pollAnswer8 = new PollAnswer()
			{
				Name = "ماهی یکبار",
				DisplayOrder = 8,
			};

			var entities = new List<PollAnswer>
			{
				pollAnswer1, pollAnswer2, pollAnswer3, pollAnswer4, pollAnswer5,  pollAnswer6,  pollAnswer7,  pollAnswer8
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Poll> Polls()
		{
			var defaultLanguage = _ctx.Set<Language>().FirstOrDefault();
			var poll1 = new Poll
			{
				Language = defaultLanguage,
				Name = "نظر شما درباره فروشگاه چیست؟",
				SystemKeyword = "Blog",
				Published = true,
				DisplayOrder = 1,
			};

			poll1.PollAnswers.Add(new PollAnswer
			{
				Name = "عالی",
				DisplayOrder = 1,
			});

			poll1.PollAnswers.Add(new PollAnswer
			{
				Name = "خوب",
				DisplayOrder = 2,
			});

			poll1.PollAnswers.Add(new PollAnswer
			{
				Name = "عادی",
				DisplayOrder = 3,
			});

			poll1.PollAnswers.Add(new PollAnswer
			{
				Name = "ضعیف",
				DisplayOrder = 4,
			});


			var poll2 = new Poll
			{
				Language = defaultLanguage,
				Name = "هرچندبار خرید اینترنتی می کنید؟",
				SystemKeyword = "Blog",
				Published = true,
				DisplayOrder = 2,
			};

			poll2.PollAnswers.Add(new PollAnswer
			{
				Name = "روزانه",
				DisplayOrder = 1,
			});

			poll2.PollAnswers.Add(new PollAnswer
			{
				Name = "هفته‌ای یکبار",
				DisplayOrder = 2,
			});

			poll2.PollAnswers.Add(new PollAnswer
			{
				Name = "هر دو هفته",
				DisplayOrder = 3,
			});

			poll2.PollAnswers.Add(new PollAnswer
			{
				Name = "ماهی یکبار یا بیشتر",
				DisplayOrder = 4,
			});


			var entities = new List<Poll>
			{
				poll1, poll2
			};

			this.Alter(entities);
			return entities;
		}

		#region Alterations

		protected virtual void Alter(IList<MeasureDimension> entities)
		{
		}

		protected virtual void Alter(IList<MeasureWeight> entities)
		{
		}

		protected virtual void Alter(IList<TaxCategory> entities)
		{
		}

		protected virtual void Alter(IList<Currency> entities)
		{
		}

		protected virtual void Alter(IList<Country> entities)
		{
		}

		protected virtual void Alter(IList<ShippingMethod> entities)
		{
		}

		protected virtual void Alter(IList<CustomerRole> entities)
		{
		}

		protected virtual void Alter(Address entity)
		{
		}

		protected virtual void Alter(Customer entity)
		{
		}

		protected virtual void Alter(IList<DeliveryTime> entities)
		{
		}

        protected virtual void Alter(IList<QuantityUnit> entities)
        {
        }

		protected virtual void Alter(IList<EmailAccount> entities)
		{
		}

		protected virtual void Alter(IList<MessageTemplate> entities)
		{
		}

		protected virtual void Alter(IList<Topic> entities)
		{
		}

		protected virtual void Alter(IList<Store> entities)
		{
		}

		protected virtual void Alter(IList<MediaFile> entities)
		{
		}

		protected virtual void Alter(IList<ISettings> settings)
		{
		}

		protected virtual void Alter(IList<StoreInformationSettings> settings)
		{
		}

		protected virtual void Alter(IList<OrderSettings> settings)
		{
		}

		protected virtual void Alter(IList<MeasureSettings> settings)
		{
		}

		protected virtual void Alter(IList<ShippingSettings> settings)
		{
		}

		protected virtual void Alter(IList<PaymentSettings> settings)
		{
		}

		protected virtual void Alter(IList<TaxSettings> settings)
		{
		}

		protected virtual void Alter(IList<EmailAccountSettings> settings)
		{
		}

		protected virtual void Alter(IList<ActivityLogType> entities)
		{
		}

		protected virtual void Alter(IList<ProductTemplate> entities)
		{
		}

		protected virtual void Alter(IList<CategoryTemplate> entities)
		{
		}

		protected virtual void Alter(IList<ManufacturerTemplate> entities)
		{
		}

		protected virtual void Alter(IList<ScheduleTask> entities)
		{
		}

		protected virtual void Alter(IList<SpecificationAttribute> entities)
		{
		}

		protected virtual void Alter(IList<ProductAttribute> entities)
		{
		}

		protected virtual void Alter(IList<ProductAttributeOptionsSet> entities)
		{
		}

		protected virtual void Alter(IList<ProductAttributeOption> entities)
		{
		}

		protected virtual void Alter(IList<ProductVariantAttribute> entities)
		{
		}

		protected virtual void Alter(IList<Category> entities)
		{
		}

		protected virtual void Alter(IList<Manufacturer> entities)
		{
		}

		protected virtual void Alter(IList<Product> entities)
		{
		}

		protected virtual void Alter(IList<ForumGroup> entities)
		{
		}

		protected virtual void Alter(IList<Forum> entities)
		{
		}

		protected virtual void Alter(IList<Discount> entities)
		{
		}

		protected virtual void Alter(IList<BlogPost> entities)
		{
		}

		protected virtual void Alter(IList<NewsItem> entities)
		{
		}

		protected virtual void Alter(IList<Poll> entities)
		{
		}

		protected virtual void Alter(IList<PollAnswer> entities)
		{
		}

		protected virtual void Alter(IList<ProductTag> entities)
		{
		}

		protected virtual void Alter(IList<ProductBundleItem> entities)
		{
		}

		protected virtual void Alter(UrlRecord entity)
		{
		}

		#endregion Alterations

		#endregion Sample data creators

		#region Helpers

		protected SmartObjectContext DbContext
		{
			get
			{
				return _ctx;
			}
		}

		protected string SampleImagesPath
		{
			get
			{
				return _sampleImagesPath;
			}
		}

		public virtual UrlRecord CreateUrlRecordFor<T>(T entity) where T : BaseEntity, ISlugSupported, new()
		{
			var name = "";
			var languageId = 0;

			switch (entity)
			{
				case Category x:
					name = x.Name;
					break;
				case Manufacturer x:
					name = x.Name;
					break;
				case Product x:
					name = x.Name;
					break;
				case BlogPost x:
					name = x.Title;
					languageId = x.LanguageId;
					break;
				case NewsItem x:
					name = x.Title;
					languageId = x.LanguageId;
					break;
				case Topic x:
					name = SeoHelper.GetSeName(x.SystemName, true, false, true).Truncate(400);
					break;
			}

			if (name.HasValue())
			{
				var result = new UrlRecord
				{
					EntityId = entity.Id,
					EntityName = entity.GetUnproxiedType().Name,
					LanguageId = languageId,
					Slug = name,
					IsActive = true
				};

				this.Alter(result);
				return result;
			}
			
			return null;
		}

		protected MediaFile CreatePicture(string fileName, string seoFilename = null)
		{
			try
			{
				var ext = Path.GetExtension(fileName);
				var path = Path.Combine(_sampleImagesPath, fileName).Replace('/', '\\');
				var mimeType = MimeTypes.MapNameToMimeType(ext);
				var buffer = File.ReadAllBytes(path);
				var now = DateTime.UtcNow;

				var name = seoFilename.HasValue()
					? seoFilename.Truncate(100) + ext
					: Path.GetFileName(fileName).ToLower().Replace('_', '-');

				var file = new MediaFile
				{
					Name = name,
                    MediaType = "image",
                    MimeType = mimeType,
                    Extension = ext.EmptyNull().TrimStart('.'),
					CreatedOnUtc = now,
					UpdatedOnUtc = now,
					Size = buffer.Length,
					MediaStorage = new MediaStorage { Data = buffer },
					Version = 1 // so that FolderId is set later during track detection
				};

				return file;
			}
			catch (Exception ex) 
			{
				//throw ex;
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return null;
			}
		}

        protected void AddProductPicture(
            Product product,
            string imageName,
            string seName = null,
            int displayOrder = 1)
        {
			if (seName == null)
			{
				seName = GetSeName(Path.GetFileNameWithoutExtension(imageName));
			}

			product.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture(imageName, seName),
                DisplayOrder = displayOrder
            });
        }

        protected string GetSeName(string name)
		{
			return SeoHelper.GetSeName(name, true, false);
		}

		protected Currency CreateCurrency(string locale, decimal rate = 1M, string formatting = "", bool published = false, int order = 1)
		{
			RegionInfo info = null;
			Currency currency = null;
			try
			{
				info = new RegionInfo(locale);
				if (info != null)
				{
					currency = new Currency
					{
						DisplayLocale = locale,
						Name = info.CurrencyNativeName,
						CurrencyCode = info.ISOCurrencySymbol,
						Rate = rate,
						CustomFormatting = formatting,
						Published = published,
						DisplayOrder = order
					};
				}
			}
			catch
			{
				return null;
			}

			return currency;
		}

		protected string FormatAttributeXml(int attributeId, int valueId, bool withRootTag = true)
		{
			var xml = $"<ProductVariantAttribute ID=\"{attributeId}\"><ProductVariantAttributeValue><Value>{valueId}</Value></ProductVariantAttributeValue></ProductVariantAttribute>";

			if (withRootTag)
			{
				return string.Concat("<Attributes>", xml, "</Attributes>");
			}

			return xml;
		}
		protected string FormatAttributeXml(int attributeId1, int valueId1, int attributeId2, int valueId2)
		{
			return string.Concat(
				"<Attributes>",
				FormatAttributeXml(attributeId1, valueId1, false),
				FormatAttributeXml(attributeId2, valueId2, false),
				"</Attributes>");
		}
		protected string FormatAttributeXml(int attributeId1, int valueId1, int attributeId2, int valueId2, int attributeId3, int valueId3)
		{
			return string.Concat(
				"<Attributes>",
				FormatAttributeXml(attributeId1, valueId1, false),
				FormatAttributeXml(attributeId2, valueId2, false),
				FormatAttributeXml(attributeId3, valueId3, false),
				"</Attributes>");
		}

		#endregion
	}
}