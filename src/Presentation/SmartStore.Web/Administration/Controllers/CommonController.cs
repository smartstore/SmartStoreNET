using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Common;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using Telerik.Web.Mvc;

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
		private readonly Lazy<CommonSettings> _commonSettings;
        private readonly Lazy<CurrencySettings> _currencySettings;
        private readonly Lazy<MeasureSettings> _measureSettings;
        private readonly Lazy<IDateTimeHelper> _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly Lazy<IImageCache> _imageCache;
        private readonly Lazy<SecuritySettings> _securitySettings;
		private readonly Lazy<IMenuPublisher> _menuPublisher;
        private readonly Lazy<IPluginFinder> _pluginFinder;
        private readonly IGenericAttributeService _genericAttributeService;
		private readonly ICommonServices _services;
		private readonly Func<string, ICacheManager> _cache;

		private readonly static object s_lock = new object();

        #endregion

        #region Constructors

        public CommonController(
			Lazy<IPaymentService> paymentService,
			Lazy<IShippingService> shippingService,
            Lazy<ICurrencyService> currencyService,
			Lazy<IMeasureService> measureService,
            ICustomerService customerService,
			IUrlRecordService urlRecordService, 
			Lazy<CommonSettings> commonSettings,
			Lazy<CurrencySettings> currencySettings,
            Lazy<MeasureSettings> measureSettings,
			Lazy<IDateTimeHelper> dateTimeHelper,
            ILanguageService languageService,
			ILocalizationService localizationService,
            Lazy<IImageCache> imageCache,
			Lazy<SecuritySettings> securitySettings,
			Lazy<IMenuPublisher> menuPublisher,
            Lazy<IPluginFinder> pluginFinder,
            IGenericAttributeService genericAttributeService,
			ICommonServices services,
			Func<string, ICacheManager> cache)
        {
            this._paymentService = paymentService;
            this._shippingService = shippingService;
            this._currencyService = currencyService;
            this._measureService = measureService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
			this._commonSettings = commonSettings;
            this._currencySettings = currencySettings;
            this._measureSettings = measureSettings;
            this._dateTimeHelper = dateTimeHelper;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._imageCache = imageCache;
            this._securitySettings = securitySettings;
            this._menuPublisher = menuPublisher;
			this._pluginFinder = pluginFinder;
            this._genericAttributeService = genericAttributeService;
			this._services = services;
			this._cache = cache;
        }

        #endregion

        #region Methods

        #region Navbar & Menu

		[ChildActionOnly]
		public ActionResult Navbar()
		{
			var currentCustomer = _services.WorkContext.CurrentCustomer;

			ViewBag.UserName = _services.Settings.LoadSetting<CustomerSettings>().UsernamesEnabled ? currentCustomer.Username : currentCustomer.Email;
			ViewBag.Stores = _services.StoreService.GetAllStores();
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
			{
				ViewBag.CheckUpdateResult = AsyncRunner.RunSync(() => CheckUpdateAsync(false));
			}

			return PartialView();
		}

        [ChildActionOnly]
        public ActionResult Menu()
        {
			var cacheManager = _services.Cache;

			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
			string cacheKey = string.Format("smartstore.pres.adminmenu.navigation-{0}-{1}", _services.WorkContext.WorkingLanguage.Id, string.Join(",", customerRolesIds));

            var rootNode = cacheManager.Get(cacheKey, () =>
            {
				lock (s_lock) {
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
				var permitted = item.PermissionNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(x => _services.Permissions.Authorize(x.Trim()));
                if (!permitted)
                {
                    result = false;
                }
            }

            return result;
        }

		#endregion

		#region CheckUpdate

		public async Task<ActionResult> CheckUpdate(bool enforce = false)
		{
			var model = await CheckUpdateAsync(enforce);
			return View(model);
		}

		public ActionResult CheckUpdateSuppress(string myVersion, string newVersion)
		{
			CheckUpdateSuppressInternal(myVersion, newVersion);
			return RedirectToAction("Index", "Home");
		}

		public void CheckUpdateSuppressInternal(string myVersion, string newVersion)
		{
			var suppressKey = "SuppressUpdateMessage.{0}.{1}".FormatInvariant(myVersion, newVersion);
			_genericAttributeService.SaveAttribute<bool?>(_services.WorkContext.CurrentCustomer, suppressKey, true);
			_services.Cache.RemoveByPattern("Common.CheckUpdateResult");
		}

		[NonAction]
		private async Task<CheckUpdateResult> CheckUpdateAsync(bool enforce = false, bool forSuppress = false)
		{
			var curVersion = SmartStoreVersion.CurrentFullVersion;
			var lang = _services.WorkContext.WorkingLanguage.UniqueSeoCode;
			var cacheKeyPattern = "Common.CheckUpdateResult";
			var cacheKey = "{0}.{1}".FormatInvariant(cacheKeyPattern, lang);

			if (enforce)
			{
				_services.Cache.RemoveByPattern(cacheKeyPattern);
			}

			var result = await _services.Cache.Get(cacheKey, async () =>
			{
				var noUpdateResult = new CheckUpdateResult { UpdateAvailable = false, LanguageCode = lang, CurrentVersion = curVersion };

				try
				{
					string url = "http://dlm.smartstore.com/api/v1/apprelease/CheckUpdate?app=SMNET&version={0}&language={1}".FormatInvariant(curVersion, lang);

					using (var client = new HttpClient())
					{
						client.Timeout = TimeSpan.FromMilliseconds(3000);
						client.DefaultRequestHeaders.Accept.Clear();
						client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						client.DefaultRequestHeaders.UserAgent.ParseAdd("SmartStore.NET {0}".FormatInvariant(curVersion));
						client.DefaultRequestHeaders.Add("Authorization-Key", _services.StoreContext.CurrentStore.Url.TrimEnd('/'));
						
						HttpResponseMessage response = await client.GetAsync(url);
						
						if (response.StatusCode != HttpStatusCode.OK)
						{
							return noUpdateResult;
						}
						
						var jsonStr = await response.Content.ReadAsStringAsync();
						var model = JsonConvert.DeserializeObject<CheckUpdateResult>(jsonStr);
						
						model.UpdateAvailable = true;
						model.CurrentVersion = curVersion;
						model.LanguageCode = lang;

						if (CommonHelper.IsDevEnvironment || !_commonSettings.Value.AutoUpdateEnabled || !_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
						{
							model.AutoUpdatePossible = false;
						}

						// don't show message if user decided to suppress it
						var suppressKey = "SuppressUpdateMessage.{0}.{1}".FormatInvariant(curVersion, model.Version);
						if (enforce)
						{
							// but ignore user's decision if 'enforce'
							_genericAttributeService.SaveAttribute<bool?>(_services.WorkContext.CurrentCustomer, suppressKey, null);
						}
						var showMessage = enforce || _services.WorkContext.CurrentCustomer.GetAttribute<bool?>(suppressKey, _genericAttributeService).GetValueOrDefault() == false;
						if (!showMessage)
						{
							return noUpdateResult;
						}

						return model;
					}
				}
				catch (Exception ex)
				{
					Logger.Error("An error occurred while checking for update", ex);
					return noUpdateResult;
				}
			}, 1440 /* 24h * 60min. */);

			return result;
		}

		public ActionResult InstallUpdate(string packageUrl)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();
			
			try
			{
				Uri uri = new Uri(packageUrl);
				string fileName = System.IO.Path.GetFileName(uri.LocalPath);

				using (var wc = new WebClient())
				{
					var dir = CommonHelper.MapPath(AppUpdater.UpdatePackagePath, false);
					Directory.CreateDirectory(dir);
					wc.DownloadFile(packageUrl, Path.Combine(dir, fileName));
				}

				if (!InstallablePackageExists())
				{
					NotifyError(_localizationService.GetResource("Admin.CheckUpdate.AutoUpdateFailure"));
				}
				else
				{
					_services.WebHelper.RestartAppDomain();
				}
				
			}
			catch (Exception ex)
			{
				NotifyError(ex);
			}
			
			return RedirectToAction("CheckUpdate", new { enforce = true });
		}

		private bool InstallablePackageExists()
		{
			using (var updater = new AppUpdater())
			{
				return updater.InstallablePackageExists();
			}
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

		[HttpPost]
		public JsonResult SetGridState(string gridId, GridState state, string path)
		{
			if (gridId.HasValue() && state != null && path.HasValue())
			{
				var info = new GridStateInfo { State = state, Path = path };
				TempData[gridId] = info;
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
			model.HttpHost = _services.WebHelper.ServerVariables("HTTP_HOST");
            //Environment.GetEnvironmentVariable("USERNAME");

			try
			{
				var mbSize = _services.DbContext.SqlQuery<decimal>("Select Sum(size)/128.0 From sysfiles").FirstOrDefault();
				model.DatabaseSize = Convert.ToDouble(mbSize);
			}
			catch (Exception) {	}

			try
			{
				if (DataSettings.Current.IsValid())
				{
					model.DataProviderFriendlyName = DataSettings.Current.ProviderFriendlyName;
					model.ShrinkDatabaseEnabled = _services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance) && DataSettings.Current.IsSqlServer;
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
			var store = _services.StoreContext.CurrentStore;
            
            //store URL
			var storeUrl = store.Url.EnsureEndsWith("/");
			if (storeUrl.HasValue() && (storeUrl.IsCaseInsensitiveEqual(_services.WebHelper.GetStoreLocation(false)) || storeUrl.IsCaseInsensitiveEqual(_services.WebHelper.GetStoreLocation(true))))
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = _localizationService.GetResource("Admin.System.Warnings.URL.Match")
				});
			}
			else
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Warning,
					Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.URL.NoMatch"), storeUrl, _services.WebHelper.GetStoreLocation(false))
				});
			}

			// sitemap reachability
			var sitemapReachable = false;
			try
			{
				var sitemapUrl = Url.RouteUrl("SitemapSEO", (object)null, _securitySettings.Value.ForceSslForAllPages ? "https" : "http");
				var request = (HttpWebRequest)WebRequest.Create(sitemapUrl);
				request.Method = "HEAD";
				request.Timeout = 15000;

				using (var response = (HttpWebResponse)request.GetResponse())
				{
					sitemapReachable = (response.StatusCode == HttpStatusCode.OK);
				}
			}
			catch (WebException) { }

			if (sitemapReachable)
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = _localizationService.GetResource("Admin.System.Warnings.SitemapReachable.OK")
				});
			}
			else
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Warning,
					Text = _localizationService.GetResource("Admin.System.Warnings.SitemapReachable.Wrong")
				});
			}

            //primary exchange rate currency
			var perCurrency = store.PrimaryExchangeRateCurrency;
            if (perCurrency != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Set"),
                });

                if (perCurrency.Rate != 1)
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Rate1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.NotSet")
                });
            }

            //primary store currency
			var pscCurrency = store.PrimaryStoreCurrency;
            if (pscCurrency != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.Set"),
                });
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.NotSet")
                });
            }


            //base measure weight
			var bWeight = _measureService.Value.GetMeasureWeightById(_measureSettings.Value.BaseWeightId);
            if (bWeight != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Set"),
                });

                if (bWeight.Ratio != 1)
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.NotSet")
                });
            }


            //base dimension weight
			var bDimension = _measureService.Value.GetMeasureDimensionById(_measureSettings.Value.BaseDimensionId);
            if (bDimension != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Set"),
                });

                if (bDimension.Ratio != 1)
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.NotSet")
                });
            }

			// shipping rate coputation methods
			int activeShippingMethodCount = 0;

			try
			{
				activeShippingMethodCount = _shippingService.Value.LoadActiveShippingRateComputationMethods()
					.Where(x => x.Value.ShippingRateComputationMethodType == ShippingRateComputationMethodType.Offline)
					.Count();
			}
			catch { }

			if (activeShippingMethodCount > 1)
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Warning,
					Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.OnlyOneOffline")
				});
			}

            //payment methods
			int activePaymentMethodCount = 0;

			try
			{
				activePaymentMethodCount = _paymentService.Value.LoadActivePaymentMethods().Count();
			}
			catch { }

			if (activePaymentMethodCount > 0)
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.OK")
				});
			}
			else
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Fail,
					Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.NoActive")
				});
			}

            //incompatible plugins
			if (PluginManager.IncompatiblePlugins != null)
			{
				foreach (var pluginName in PluginManager.IncompatiblePlugins)
				{
					model.Add(new SystemWarningModel
					{
						Level = SystemWarningLevel.Warning,
						Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.IncompatiblePlugin"), pluginName)
					});
				}
			}

            //validate write permissions (the same procedure like during installation)
            var dirPermissionsOk = true;
			var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite(_services.WebHelper);
			foreach (string dir in dirsToCheck)
			{
				if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
				{
					model.Add(new SystemWarningModel
					{
						Level = SystemWarningLevel.Warning,
						Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.Wrong"), WindowsIdentity.GetCurrent().Name, dir)
					});
					dirPermissionsOk = false;
				}
			}
			if (dirPermissionsOk)
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = _localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.OK")
				});
			}

            var filePermissionsOk = true;
			var filesToCheck = FilePermissionHelper.GetFilesWrite(_services.WebHelper);
			foreach (string file in filesToCheck)
			{
				if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
				{
					model.Add(new SystemWarningModel
					{
						Level = SystemWarningLevel.Warning,
						Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.FilePermission.Wrong"), WindowsIdentity.GetCurrent().Name, file)
					});
					filePermissionsOk = false;
				}
			}
			if (filePermissionsOk)
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = _localizationService.GetResource("Admin.System.Warnings.FilePermission.OK")
				});
			}
            
            return View(model);
        }

		public ActionResult ShrinkDatabase()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			try
			{
				if (DataSettings.Current.IsSqlServer)
				{
					_services.DbContext.ExecuteSqlCommand("DBCC SHRINKDATABASE(0)", true);
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

			_imageCache.Value.DeleteCachedImages();

            return RedirectToAction("Maintenance");
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-guests")]
        public ActionResult MaintenanceDeleteGuests(MaintenanceModel model)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
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

        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var model = new LanguageSelectorModel();
			model.CurrentLanguage = _services.WorkContext.WorkingLanguage.ToModel();
			model.AvailableLanguages = _languageService
				 .GetAllLanguages(storeId: _services.StoreContext.CurrentStore.Id)
				 .Select(x => x.ToModel())
				 .ToList();
            return PartialView(model);
        }
        public ActionResult LanguageSelected(int customerlanguage)
        {
            var language = _languageService.GetLanguageById(customerlanguage);
            if (language != null)
            {
				_services.WorkContext.WorkingLanguage = language;
            }
			return Content(_localizationService.GetResource("Admin.Common.DataEditSuccess"));
        }

		public ActionResult ClearCache(string previousUrl)
        {
			var cacheManager = _services.Cache;
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

			_services.WebHelper.RestartAppDomain();

			if (previousUrl.HasValue())
				return Redirect(previousUrl);

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Search engine friendly names

        public ActionResult SeNames()
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new UrlRecordListModel();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult SeNames(GridCommand command, UrlRecordListModel model)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
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
            if (!_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

			var storeId = _services.StoreContext.CurrentStore.Id;
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            model.Key = model.Key.TrimSafe();
            model.Value = model.Value.TrimSafe();

            if (!ModelState.IsValid)
            {
                // display the first model error
                var modelStateErrorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrorMessages.FirstOrDefault());
            }

			var storeId = _services.StoreContext.CurrentStore.Id;

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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            model.Key = model.Key.TrimSafe();
            model.Value = model.Value.TrimSafe();

            if (!ModelState.IsValid)
            {
                // display the first model error
                var modelStateErrorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrorMessages.FirstOrDefault());
            }

			var storeId = _services.StoreContext.CurrentStore.Id;

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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
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
