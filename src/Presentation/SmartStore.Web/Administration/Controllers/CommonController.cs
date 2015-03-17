using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Collections;
using SmartStore.Utilities;
using SmartStore.Web.Framework.UI;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Data;
using Telerik.Web.Mvc;
using SmartStore.Services.Common;
using SmartStore.Core.Domain.Common;
using System.Reflection;
using Autofac;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CommonController : AdminControllerBase
    {
        #region Fields

        private readonly Lazy<IPaymentService> _paymentService;
        private readonly Lazy<IShippingService> _shippingService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly Lazy<IMeasureService> _measureService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly Lazy<CurrencySettings> _currencySettings;
        private readonly Lazy<MeasureSettings> _measureSettings;
        private readonly Lazy<IDateTimeHelper> _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly Lazy<IImageCache> _imageCache;
        private readonly Lazy<SecuritySettings> _securitySettings;
		private readonly Lazy<IMenuPublisher> _menuPublisher;
        private readonly Lazy<IPluginFinder> _pluginFinder;
        private readonly IGenericAttributeService _genericAttributeService;
		private readonly IDbContext _dbContext;
		private readonly Func<string, ICacheManager> _cache;

		private readonly static object _lock = new object();

        #endregion

        #region Constructors

        public CommonController(
			Lazy<IPaymentService> paymentService,
			Lazy<IShippingService> shippingService,
            Lazy<ICurrencyService> currencyService,
			Lazy<IMeasureService> measureService,
            ICustomerService customerService,
			IUrlRecordService urlRecordService, 
			IWebHelper webHelper,
			Lazy<CurrencySettings> currencySettings,
            Lazy<MeasureSettings> measureSettings,
			Lazy<IDateTimeHelper> dateTimeHelper,
            ILanguageService languageService,
			IWorkContext workContext,
			IStoreContext storeContext,
            IPermissionService permissionService,
			ILocalizationService localizationService,
            Lazy<IImageCache> imageCache,
			Lazy<SecuritySettings> securitySettings,
			Lazy<IMenuPublisher> menuPublisher,
            Lazy<IPluginFinder> pluginFinder,
            IGenericAttributeService genericAttributeService,
			IDbContext dbContext,
			Func<string, ICacheManager> cache)
        {
            this._paymentService = paymentService;
            this._shippingService = shippingService;
            this._currencyService = currencyService;
            this._measureService = measureService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._webHelper = webHelper;
            this._currencySettings = currencySettings;
            this._measureSettings = measureSettings;
            this._dateTimeHelper = dateTimeHelper;
            this._languageService = languageService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
            this._imageCache = imageCache;
            this._securitySettings = securitySettings;
            this._menuPublisher = menuPublisher;
			this._pluginFinder = pluginFinder;
            this._genericAttributeService = genericAttributeService;
			this._dbContext = dbContext;
			this._cache = cache;
        }

        #endregion

        #region Methods

        #region AdminMenu

        [ChildActionOnly]
        public ActionResult Menu()
        {
			var cacheManager = _cache("static");

            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
            string cacheKey = string.Format("smartstore.pres.adminmenu.navigation-{0}-{1}", _workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds));

            var rootNode = cacheManager.Get(cacheKey, () =>
            {
				lock (_lock) {
					return PrepareAdminMenu();
				}
            });

            return PartialView(rootNode);
        }

        private TreeNode<MenuItem> PrepareAdminMenu()
        {
            XmlSiteMap siteMap = new XmlSiteMap();
			siteMap.LoadFrom("~/Administration/sitemap.config");
            
            var rootNode = ConvertSitemapNodeToMenuItemNode(siteMap.RootNode);

			_menuPublisher.Value.RegisterMenus(rootNode, "admin");

			//TreeNode<MenuItem> pluginNode = null;

			//// "collect" menus from plugins
			//var providers = new List<IMenuProvider>();
			//var providerTypes = _typeFinder.FindClassesOfType<IMenuProvider>(ignoreInactivePlugins: true);

			//foreach (var type in providerTypes)
			//{
			//	try
			//	{
			//		var provider = Activator.CreateInstance(type) as IMenuProvider;
			//		if (provider.MenuName.IsCaseInsensitiveEqual("admin"))
			//		{
			//			providers.Add(provider);
			//		}
			//	}
			//	catch { }
			//}

			//if (providers.Any())
			//{
			//	pluginNode = rootNode.Children.FirstOrDefault(x => x.Value.Id == "plugins");
			//	providers.Each(x => x.BuildMenu(pluginNode));
			//}

			// hide based on permissions
            rootNode.TraverseTree(x => {
                if (!x.IsRoot)
                {
					if (!MenuItemAccessPermitted(x.Value))
                    {
                        x.Value.Visible = false;
                    }
                }
            });

            // hide dropdown nodes when no child is visible
			rootNode.TraverseTree(x =>
			{
				if (!x.IsRoot)
				{
					var item = x.Value;
					if (!item.IsGroupHeader && !item.HasRoute())
					{
						if (!x.Children.Any(child => child.Value.Visible))
						{
							item.Visible = false;
						}
					}
				}
			});

            return rootNode;
        }

        private TreeNode<MenuItem> ConvertSitemapNodeToMenuItemNode(SiteMapNode node)
        {
            var item = new MenuItem();
            var treeNode = new TreeNode<MenuItem>(item);

            if (node.RouteName.HasValue())
            {
                item.RouteName = node.RouteName;
            }
            else if (node.ActionName.HasValue() && node.ControllerName.HasValue())
            {
                item.ActionName = node.ActionName;
                item.ControllerName = node.ControllerName;
            }
            else if (node.Url.HasValue())
            {
                item.Url = node.Url;
            }
            item.RouteValues = node.RouteValues;
            
            item.Visible = node.Visible;
            item.Text = node.Title;
            item.Attributes.Merge(node.Attributes);

            if (node.Attributes.ContainsKey("permissionNames"))
                item.PermissionNames = node.Attributes["permissionNames"] as string;

            if (node.Attributes.ContainsKey("id"))
                item.Id = node.Attributes["id"] as string;

            if (node.Attributes.ContainsKey("resKey"))
                item.ResKey = node.Attributes["resKey"] as string;

			if (node.Attributes.ContainsKey("iconClass"))
				item.Icon = node.Attributes["iconClass"] as string;

            if (node.Attributes.ContainsKey("imageUrl"))
                item.ImageUrl = node.Attributes["imageUrl"] as string;

            if (node.Attributes.ContainsKey("isGroupHeader"))
                item.IsGroupHeader = Boolean.Parse(node.Attributes["isGroupHeader"] as string);

            // iterate children recursively
            foreach (var childNode in node.ChildNodes)
            {
                var childTreeNode = ConvertSitemapNodeToMenuItemNode(childNode);
                treeNode.Append(childTreeNode);
            }
            
            return treeNode;
        }

        private bool MenuItemAccessPermitted(MenuItem item)
        {
            var result = true;

			if (_securitySettings.Value.HideAdminMenuItemsBasedOnPermissions && item.PermissionNames.HasValue())
            {
                var permitted = item.PermissionNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(x => _permissionService.Authorize(x.Trim()));
                if (!permitted)
                {
                    result = false;
                }
            }

            return result;
        }

        #endregion

		[HttpPost]
		public JsonResult SetSelectedTab(string navId, string tabId, string path)
		{
			if (navId.HasValue() && tabId.HasValue() && path.HasValue())
			{
				var info = new SelectedTabInfo { TabId = tabId, Path = path };
				TempData["SelectedTab." + navId] = info;
			}
			return Json(new { Success = true });
		}

        public ActionResult SystemInfo()
        {
            var model = new SystemInfoModel();
            model.AppVersion = SmartStoreVersion.CurrentFullVersion;

            try
            {
                model.OperatingSystem = Environment.OSVersion.VersionString;
            }
            catch (Exception) { }
            try
            {
                model.AspNetInfo = RuntimeEnvironment.GetSystemVersion();
            }
            catch (Exception) { }
            try
            {
                model.IsFullTrust = AppDomain.CurrentDomain.IsFullyTrusted.ToString();
            }
            catch (Exception) { }

            model.ServerTimeZone = TimeZone.CurrentTimeZone.StandardName;
            model.ServerLocalTime = DateTime.Now;
            model.UtcTime = DateTime.UtcNow;
			model.HttpHost = _webHelper.ServerVariables("HTTP_HOST");
            //Environment.GetEnvironmentVariable("USERNAME");

			try
			{
				var mbSize = _dbContext.SqlQuery<decimal>("Select Sum(size)/128.0 From sysfiles").FirstOrDefault();
				model.DatabaseSize = Convert.ToDouble(mbSize);
			}
			catch (Exception) {	}

			try
			{
				if (DataSettings.Current.IsValid())
				{
					model.DataProviderFriendlyName = DataSettings.Current.ProviderFriendlyName;
					model.ShrinkDatabaseEnabled = DataSettings.Current.IsSqlServer;
				}
			}
			catch (Exception) { }

			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var fi = new FileInfo(assembly.Location);
				model.AppDate = fi.LastWriteTime.ToLocalTime();
			}
			catch (Exception) { }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                model.LoadedAssemblies.Add(new SystemInfoModel.LoadedAssembly()
                {
                    FullName =  assembly.FullName,
                    //we cannot use Location property in medium trust
                    //Location = assembly.Location
                });
            }
            return View(model);
        }

        public ActionResult Warnings()
        {
            var model = new List<SystemWarningModel>();
            
            //store URL
			var currentStoreUrl = _storeContext.CurrentStore.Url.EnsureEndsWith("/");
			if (currentStoreUrl.HasValue() && (currentStoreUrl.IsCaseInsensitiveEqual(_webHelper.GetStoreLocation(false)) || currentStoreUrl.IsCaseInsensitiveEqual(_webHelper.GetStoreLocation(true))))
                model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Pass,
                        Text = _localizationService.GetResource("Admin.System.Warnings.URL.Match")
                    });
            else
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Warning,
					Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.URL.NoMatch"), currentStoreUrl, _webHelper.GetStoreLocation(false))
                });


            //primary exchange rate currency
			var perCurrency = _currencyService.Value.GetCurrencyById(_currencySettings.Value.PrimaryExchangeRateCurrencyId);
            if (perCurrency != null)
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Set"),
                });
                if (perCurrency.Rate != 1)
                {
                    model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Rate1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.NotSet")
                });
            }

            //primary store currency
			var pscCurrency = _currencyService.Value.GetCurrencyById(_currencySettings.Value.PrimaryStoreCurrencyId);
            if (pscCurrency != null)
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.Set"),
                });
            }
            else
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.NotSet")
                });
            }


            //base measure weight
			var bWeight = _measureService.Value.GetMeasureWeightById(_measureSettings.Value.BaseWeightId);
            if (bWeight != null)
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Set"),
                });

                if (bWeight.Ratio != 1)
                {
                    model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.NotSet")
                });
            }


            //base dimension weight
			var bDimension = _measureService.Value.GetMeasureDimensionById(_measureSettings.Value.BaseDimensionId);
            if (bDimension != null)
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Set"),
                });

                if (bDimension.Ratio != 1)
                {
                    model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.NotSet")
                });
            }

            // shipping rate coputation methods
			if (_shippingService.Value.LoadActiveShippingRateComputationMethods()
                .Where(x => x.Value.ShippingRateComputationMethodType == ShippingRateComputationMethodType.Offline)
                .Count() > 1)
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.OnlyOneOffline")
                });

            //payment methods
			if (_paymentService.Value.LoadActivePaymentMethods()
                .Count() > 0)
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.OK")
                });
            else
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.NoActive")
                });

            //incompatible plugins
            if (PluginManager.IncompatiblePlugins != null)
                foreach (var pluginName in PluginManager.IncompatiblePlugins)
                    model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.IncompatiblePlugin"), pluginName )
                    });

            //validate write permissions (the same procedure like during installation)
            var dirPermissionsOk = true;
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite(_webHelper);
            foreach (string dir in dirsToCheck)
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                {
                    model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.Wrong"), WindowsIdentity.GetCurrent().Name, dir)
                    });
                    dirPermissionsOk = false;
                }
            if (dirPermissionsOk)
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.OK")
                });

            var filePermissionsOk = true;
            var filesToCheck = FilePermissionHelper.GetFilesWrite(_webHelper);
            foreach (string file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                {
                    model.Add(new SystemWarningModel()
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.FilePermission.Wrong"), WindowsIdentity.GetCurrent().Name, file)
                    });
                    filePermissionsOk = false;
                }
            if (filePermissionsOk)
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.FilePermission.OK")
                });
            
            
            return View(model);
        }

		public ActionResult ShrinkDatabase()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			try
			{
				if (DataSettings.Current.IsSqlServer)
				{
					_dbContext.ExecuteSqlCommand("DBCC SHRINKDATABASE(0)", true);
					NotifySuccess(_localizationService.GetResource("Common.ShrinkDatabaseSuccessful"));
				}
			}
			catch (Exception ex)
			{
				NotifyError(ex);
			}

			return RedirectToReferrer();
		}

        public ActionResult Maintenance()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new MaintenanceModel();
            model.DeleteGuests.EndDate = DateTime.UtcNow.AddDays(-7);
            model.DeleteGuests.OnlyWithoutShoppingCart = true;

            // image cache stats
            long imageCacheFileCount = 0;
            long imageCacheTotalSize = 0;
			_imageCache.Value.CacheStatistics(out imageCacheFileCount, out imageCacheTotalSize);
            model.DeleteImageCache.FileCount = imageCacheFileCount;
            model.DeleteImageCache.TotalSize = Prettifier.BytesToString(imageCacheTotalSize);

            return View(model);
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-image-cache")]
        public ActionResult MaintenanceDeleteImageCache()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

			_imageCache.Value.DeleteCachedImages();

            return RedirectToAction("Maintenance");
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-guests")]
        public ActionResult MaintenanceDeleteGuests(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            DateTime? startDateValue = (model.DeleteGuests.StartDate == null) ? null
							: (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(model.DeleteGuests.StartDate.Value, _dateTimeHelper.Value.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteGuests.EndDate == null) ? null
							: (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(model.DeleteGuests.EndDate.Value, _dateTimeHelper.Value.CurrentTimeZone).AddDays(1);

            model.DeleteGuests.NumberOfDeletedCustomers = _customerService.DeleteGuestCustomers(startDateValue, endDateValue, model.DeleteGuests.OnlyWithoutShoppingCart);

            return View(model);
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-exported-files")]
        public ActionResult MaintenanceDeleteFiles(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            DateTime? startDateValue = (model.DeleteExportedFiles.StartDate == null) ? null
							: (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(model.DeleteExportedFiles.StartDate.Value, _dateTimeHelper.Value.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteExportedFiles.EndDate == null) ? null
							: (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(model.DeleteExportedFiles.EndDate.Value, _dateTimeHelper.Value.CurrentTimeZone).AddDays(1);


            model.DeleteExportedFiles.NumberOfDeletedFiles = 0;
            string path = string.Format("{0}Content\\files\\exportimport\\", this.Request.PhysicalApplicationPath);
            foreach (var fullPath in System.IO.Directory.GetFiles(path))
            {
                try
                {
                    var fileName = Path.GetFileName(fullPath);
                    if (fileName.Equals("index.htm", StringComparison.InvariantCultureIgnoreCase))
                        continue;

					if (fileName.Equals("placeholder", StringComparison.InvariantCultureIgnoreCase))
						continue;

                    var info = new FileInfo(fullPath);
                    if ((!startDateValue.HasValue || startDateValue.Value < info.CreationTimeUtc)&&
                        (!endDateValue.HasValue || info.CreationTimeUtc < endDateValue.Value))
                    {
                        System.IO.File.Delete(fullPath);
                        model.DeleteExportedFiles.NumberOfDeletedFiles++;
                    }
                }
                catch (Exception exc)
                {
                    NotifyError(exc, false);
                }
            }

            return View(model);
        }

        [HttpPost, ActionName("Maintenance"), ValidateInput(false)]
        [FormValueRequired("execute-sql-query")]
        public ActionResult MaintenanceExecuteSql(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            if (model.SqlQuery.HasValue())
            {
                var dbContext = EngineContext.Current.Resolve<IDbContext>();
                try
                {
					dbContext.ExecuteSqlThroughSmo(model.SqlQuery);

                    NotifySuccess("The sql command was executed successfully.");
                }
                catch (Exception ex)
                {
					NotifyError("Error executing sql command: {0}".FormatCurrentUI(ex.Message));
                }
            }

            return View(model);
        }

        //language
        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var model = new LanguageSelectorModel();
            model.CurrentLanguage = _workContext.WorkingLanguage.ToModel();
			model.AvailableLanguages = _languageService
				 .GetAllLanguages(storeId: _storeContext.CurrentStore.Id)
				 .Select(x => x.ToModel())
				 .ToList();
            return PartialView(model);
        }
        public ActionResult LanguageSelected(int customerlanguage)
        {
            var language = _languageService.GetLanguageById(customerlanguage);
            if (language != null)
            {
                _workContext.WorkingLanguage = language;
            }
			return Content(_localizationService.GetResource("Admin.Common.DataEditSuccess"));
        }

		public ActionResult ClearCache(string previousUrl)
        {
			var cacheManager = _cache("static");
            cacheManager.Clear();

			cacheManager = _cache("aspnet");
			cacheManager.Clear();

			this.NotifySuccess(_localizationService.GetResource("Admin.Common.TaskSuccessfullyProcessed"));

			if (previousUrl.HasValue())
				return Redirect(previousUrl);

            return RedirectToAction("Index", "Home");
        }

		public ActionResult RestartApplication(string previousUrl)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            _webHelper.RestartAppDomain();

			if (previousUrl.HasValue())
				return Redirect(previousUrl);

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Search engine friendly names

        public ActionResult SeNames()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new UrlRecordListModel();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult SeNames(GridCommand command, UrlRecordListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var urlRecords = _urlRecordService.GetAllUrlRecords(model.SeName, command.Page - 1, command.PageSize);
            var gridModel = new GridModel<UrlRecordModel>
            {
                Data = urlRecords.Select(x =>
                {
                    string languageName;
                    if (x.LanguageId == 0)
                    {
                        languageName = _localizationService.GetResource("Admin.System.SeNames.Language.Standard");
                    }
                    else
                    {
                        var language = _languageService.GetLanguageById(x.LanguageId);
                        languageName = language != null ? language.Name : "Unknown";
                    }
                    return new UrlRecordModel()
                    {
                        Id = x.Id,
                        Name = x.Slug,
                        EntityId = x.EntityId,
                        EntityName = x.EntityName,
                        IsActive = x.IsActive,
                        Language = languageName,
                    };
                }),
                Total = urlRecords.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult DeleteSelectedSeNames(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var urlRecords = new List<UrlRecord>();
                foreach (var id in selectedIds)
                {
                    var urlRecord = _urlRecordService.GetUrlRecordById(id);
                    if (urlRecord != null)
                        urlRecords.Add(urlRecord);
                }
                foreach (var urlRecord in urlRecords)
                    _urlRecordService.DeleteUrlRecord(urlRecord);
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Generic Attributes

        [ChildActionOnly]
        public ActionResult GenericAttributes(string entityName, int entityId)
        {
            ViewBag.EntityName = entityName;
            ViewBag.EntityId = entityId;

            return PartialView();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributesSelect(string entityName, int entityId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var storeId = _storeContext.CurrentStore.Id;
            ViewBag.StoreId = storeId;

            var model = new List<GenericAttributeModel>();
            if (entityName.HasValue() && entityId > 0)
            {
                var attributes = _genericAttributeService.GetAttributesForEntity(entityId, entityName);
                var query = from attr in attributes
                            where attr.StoreId == storeId || attr.StoreId == 0
                            select new GenericAttributeModel
                            {
                                Id = attr.Id,
                                EntityId = attr.EntityId,
                                EntityName = attr.KeyGroup,
                                Key = attr.Key,
                                Value = attr.Value
                            };
                model.AddRange(query);
            }


            var result = new GridModel<GenericAttributeModel>
            {
                Data = model,
                Total = model.Count
            };
            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributeAdd(GenericAttributeModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            model.Key = model.Key.TrimSafe();
            model.Value = model.Value.TrimSafe();

            if (!ModelState.IsValid)
            {
                // display the first model error
                var modelStateErrorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrorMessages.FirstOrDefault());
            }

            var storeId = _storeContext.CurrentStore.Id;

            var attr = _genericAttributeService.GetAttribute<string>(model.EntityName, model.EntityId, model.Key, storeId);
            if (attr == null)
            {
                var ga = new GenericAttribute
                {
                    StoreId = storeId,
                    KeyGroup = model.EntityName,
                    EntityId = model.EntityId,
                    Key = model.Key,
                    Value = model.Value
                };
                _genericAttributeService.InsertAttribute(ga);
            }
            else
            {
                return Content(string.Format(_localizationService.GetResource("Admin.Common.GenericAttributes.NameAlreadyExists"), model.Key));
            }

            return GenericAttributesSelect(model.EntityName, model.EntityId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributeUpdate(GenericAttributeModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            model.Key = model.Key.TrimSafe();
            model.Value = model.Value.TrimSafe();

            if (!ModelState.IsValid)
            {
                // display the first model error
                var modelStateErrorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrorMessages.FirstOrDefault());
            }

            var storeId = _storeContext.CurrentStore.Id;

            var attr = _genericAttributeService.GetAttributeById(model.Id);
            // if the key changed, ensure it isn't being used by another attribute
            if (!attr.Key.IsCaseInsensitiveEqual(model.Key))
            {
                var attr2 = _genericAttributeService.GetAttributesForEntity(model.EntityId, model.EntityName)
                    .Where(x => x.StoreId == storeId && x.Key.Equals(model.Key, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
                if (attr2 != null && attr2.Id != attr.Id)
                {
                    return Content(string.Format(_localizationService.GetResource("Admin.Common.GenericAttributes.NameAlreadyExists"), model.Key));
                }
            }

            attr.Key = model.Key;
            attr.Value = model.Value;

            _genericAttributeService.UpdateAttribute(attr);

            return GenericAttributesSelect(model.EntityName, model.EntityId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributeDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var attr = _genericAttributeService.GetAttributeById(id);

            if (attr == null)
            {
                throw new System.Web.HttpException(404, "No resource found with the specified id");
            }

            _genericAttributeService.DeleteAttribute(attr);

            return GenericAttributesSelect(attr.KeyGroup, attr.EntityId, command);
        }

        #endregion
    }
}
