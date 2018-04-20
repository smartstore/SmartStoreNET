using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Common;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Plugins;
using SmartStore.Data.Caching;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CommonController : AdminControllerBase
    {
		const string CHECKUPDATE_CACHEKEY_PREFIX = "admin:common:checkupdateresult";

		private readonly Lazy<IPaymentService> _paymentService;
        private readonly Lazy<IShippingService> _shippingService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly Lazy<IMeasureService> _measureService;
        private readonly ICustomerService _customerService;
		private readonly Lazy<CommonSettings> _commonSettings;
        private readonly Lazy<CurrencySettings> _currencySettings;
        private readonly Lazy<MeasureSettings> _measureSettings;
        private readonly Lazy<IDateTimeHelper> _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly Lazy<IImageCache> _imageCache;
		private readonly Lazy<IImportProfileService> _importProfileService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IDbCache _dbCache;
		private readonly ITaskScheduler _taskScheduler;
		private readonly ICommonServices _services;
		private readonly Lazy<ISiteMapService> _siteMapService;

        public CommonController(
			Lazy<IPaymentService> paymentService,
			Lazy<IShippingService> shippingService,
            Lazy<ICurrencyService> currencyService,
			Lazy<IMeasureService> measureService,
            ICustomerService customerService,
			Lazy<CommonSettings> commonSettings,
			Lazy<CurrencySettings> currencySettings,
            Lazy<MeasureSettings> measureSettings,
			Lazy<IDateTimeHelper> dateTimeHelper,
            ILanguageService languageService,
			ILocalizationService localizationService,
            Lazy<IImageCache> imageCache,
			Lazy<IImportProfileService> importProfileService,
			IGenericAttributeService genericAttributeService,
			IDbCache dbCache,
			ITaskScheduler taskScheduler,
			ICommonServices services,
			Lazy<ISiteMapService> siteMapService)
        {
            _paymentService = paymentService;
            _shippingService = shippingService;
            _currencyService = currencyService;
            _measureService = measureService;
            _customerService = customerService;
			_commonSettings = commonSettings;
            _currencySettings = currencySettings;
            _measureSettings = measureSettings;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _imageCache = imageCache;
			_importProfileService = importProfileService;
            _genericAttributeService = genericAttributeService;
			_dbCache = dbCache;
			_taskScheduler = taskScheduler;
			_services = services;
			_siteMapService = siteMapService;
        }

        #region Navbar & Menu

		[ChildActionOnly]
		public ActionResult Navbar()
		{
			var currentCustomer = _services.WorkContext.CurrentCustomer;

			ViewBag.UserName = _services.Settings.LoadSetting<CustomerSettings>().UsernamesEnabled ? currentCustomer.Username : currentCustomer.Email;
			ViewBag.Stores = _services.StoreService.GetAllStores();
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
			{
				ViewBag.CheckUpdateResult = AsyncRunner.RunSync(() => CheckUpdateInternalAsync(false));
			}

			return PartialView();
		}

        [ChildActionOnly]
        public ActionResult Menu()
        {
			var rootNode = _siteMapService.Value.GetRootNode("admin");
            return PartialView(rootNode);
        }

		#endregion

		#region CheckUpdate

		public async Task<ActionResult> CheckUpdate(bool enforce = false)
		{
			var model = await CheckUpdateInternalAsync(enforce);
			return View(model);
		}

		public ActionResult CheckUpdateSuppress(string myVersion, string newVersion)
		{
			CheckUpdateSuppressInternal(myVersion, newVersion);
			return RedirectToAction("Index", "Home");
		}

		private void CheckUpdateSuppressInternal(string myVersion, string newVersion)
		{
			var suppressKey = "SuppressUpdateMessage.{0}.{1}".FormatInvariant(myVersion, newVersion);
			_genericAttributeService.SaveAttribute<bool?>(_services.WorkContext.CurrentCustomer, suppressKey, true);
			_services.Cache.RemoveByPattern(CHECKUPDATE_CACHEKEY_PREFIX + "*");
		}

		private async Task<CheckUpdateResult> CheckUpdateInternalAsync(bool enforce = false, bool forSuppress = false)
		{
			var curVersion = SmartStoreVersion.CurrentFullVersion;
			var lang = _services.WorkContext.WorkingLanguage.UniqueSeoCode;
			var cacheKey = "{0}-{1}".FormatInvariant(CHECKUPDATE_CACHEKEY_PREFIX, lang);

			if (enforce)
			{
				_services.Cache.RemoveByPattern(CHECKUPDATE_CACHEKEY_PREFIX + "*");
			}

			var result = await _services.Cache.GetAsync(cacheKey, async () => 
			{
				var noUpdateResult = new CheckUpdateResult { UpdateAvailable = false, LanguageCode = lang, CurrentVersion = curVersion };

				try
				{
					string url = "https://dlm.smartstore.com/api/v1/apprelease/CheckUpdate?app=SMNET&version={0}&language={1}".FormatInvariant(curVersion, lang);

					using (var client = new HttpClient())
					{
						client.Timeout = TimeSpan.FromMilliseconds(3000);
						client.DefaultRequestHeaders.Accept.Clear();
						client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						client.DefaultRequestHeaders.UserAgent.ParseAdd("SmartStore.NET {0}".FormatInvariant(curVersion));
						client.DefaultRequestHeaders.Add("Authorization-Key", _services.StoreContext.CurrentStore.Url.TrimEnd('/'));
						client.DefaultRequestHeaders.Add("X-Application-ID", HostingEnvironment.ApplicationID);

						HttpResponseMessage response = await client.GetAsync(url);

						if (response.StatusCode != HttpStatusCode.OK)
						{
							return noUpdateResult;
						}

						var jsonStr = response.Content.ReadAsStringAsync().Result;
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
					Logger.Error(ex, "An error occurred while checking for update");
					return noUpdateResult;
				}
			});

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

		#region UI Helpers

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
		public JsonResult SetGridState(string gridId, GridStateInfo.GridState state, string path)
		{
			if (gridId.HasValue() && state != null && path.HasValue())
			{
				var info = new GridStateInfo { State = state, Path = path };
				TempData[gridId] = info;
			}
			return Json(new { Success = true });
		}

		#endregion

		#region Maintenance

		public ActionResult SystemInfo()
        {
            var model = new SystemInfoModel();
            model.AppVersion = SmartStoreVersion.CurrentFullVersion;

            try
            {
                model.OperatingSystem = "{0} (x{1})".FormatInvariant(Environment.OSVersion.VersionString, Environment.Is64BitProcess ? "64" : "32");
            }
            catch { }
            try
            {
                model.AspNetInfo = RuntimeEnvironment.GetSystemVersion();
            }
            catch { }
            try
            {
                model.IsFullTrust = AppDomain.CurrentDomain.IsFullyTrusted.ToString();
            }
            catch { }

            model.ServerTimeZone = TimeZone.CurrentTimeZone.StandardName;
            model.ServerLocalTime = DateTime.Now;
            model.UtcTime = DateTime.UtcNow;
			model.HttpHost = _services.WebHelper.ServerVariables("HTTP_HOST");

			try
			{
				var mbSize = _services.DbContext.SqlQuery<decimal>("Select Sum(size)/128.0 From sysfiles").FirstOrDefault();
				model.DatabaseSize = Convert.ToInt64(mbSize * 1024 *1024);

				model.UsedMemorySize = GetPrivateBytes();
			}
			catch {	}

			try
			{
				if (DataSettings.Current.IsValid())
				{
					model.DataProviderFriendlyName = DataSettings.Current.ProviderFriendlyName;
					model.ShrinkDatabaseEnabled = _services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance) && DataSettings.Current.IsSqlServer;
				}
			}
			catch { }

			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var fi = new FileInfo(assembly.Location);
				model.AppDate = fi.LastWriteTime.ToLocalTime();
			}
			catch { }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                model.LoadedAssemblies.Add(new SystemInfoModel.LoadedAssembly
                {
                    FullName =  assembly.FullName,
                    //we cannot use Location property in medium trust
                    //Location = assembly.Location
                });
            }
            return View(model);
        }

		private long GetPrivateBytes()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var process = System.Diagnostics.Process.GetCurrentProcess();
			process.Refresh();

			return process.PrivateMemorySize64;
		}

        public ActionResult Warnings()
        {
            var model = new List<SystemWarningModel>();
			var store = _services.StoreContext.CurrentStore;
            
            // Store URL
			// ====================================
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

			// TaskScheduler reachability
			// ====================================
			string taskSchedulerUrl = null;
			try
			{
				taskSchedulerUrl = Path.Combine(_taskScheduler.BaseUrl.EnsureEndsWith("/"), "noop").Replace(Path.DirectorySeparatorChar, '/');
				var request = WebHelper.CreateHttpRequestForSafeLocalCall(new Uri(taskSchedulerUrl));
				request.Method = "HEAD";
				request.Timeout = 5000;

				using (var response = (HttpWebResponse)request.GetResponse())
				{
					var status = response.StatusCode;
					var warningModel = new SystemWarningModel();
					warningModel.Level = (status == HttpStatusCode.OK ? SystemWarningLevel.Pass : SystemWarningLevel.Fail);

					if (status == HttpStatusCode.OK)
					{
						warningModel.Text = T("Admin.System.Warnings.TaskScheduler.OK");
					}
					else
					{
						warningModel.Text = T("Admin.System.Warnings.TaskScheduler.Fail", _taskScheduler.BaseUrl, status + " - " + status.ToString());
					}

					model.Add(warningModel);
				}
			}
			catch (WebException exception)
			{
				var msg = T("Admin.System.Warnings.TaskScheduler.Fail", _taskScheduler.BaseUrl, exception.Message);

				var xxx = T("Admin.System.Warnings.TaskScheduler.Fail");

				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Fail,
					Text = msg
				});

				Logger.Error(exception, msg);
			}

			// Sitemap reachability
			// ====================================
			string sitemapUrl = null;
			try
			{
				sitemapUrl = WebHelper.GetAbsoluteUrl(Url.RouteUrl("SitemapSEO"), this.Request);
				var request = WebHelper.CreateHttpRequestForSafeLocalCall(new Uri(sitemapUrl));
				request.Method = "HEAD";
				request.Timeout = 15000;

				using (var response = (HttpWebResponse)request.GetResponse())
				{
					var status = response.StatusCode;
					var warningModel = new SystemWarningModel();
					warningModel.Level = (status == HttpStatusCode.OK ? SystemWarningLevel.Pass : SystemWarningLevel.Warning);

					switch (status)
					{
						case HttpStatusCode.OK:
							warningModel.Text = T("Admin.System.Warnings.SitemapReachable.OK");
							break;
						default:
							if (status == HttpStatusCode.MethodNotAllowed)
								warningModel.Text = T("Admin.System.Warnings.SitemapReachable.MethodNotAllowed");
							else
								warningModel.Text = T("Admin.System.Warnings.SitemapReachable.Wrong");

							warningModel.Text = string.Concat(warningModel.Text, " ", T("Admin.Common.HttpStatus", (int)status, status.ToString()));
							break;
					}

					model.Add(warningModel);
				}
			}
			catch (WebException exception)
			{
				model.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Warning,
					Text = T("Admin.System.Warnings.SitemapReachable.Wrong")
				});

				Logger.Warn(exception, T("Admin.System.Warnings.SitemapReachable.Wrong"));
			}

			// Primary exchange rate currency
			// ====================================
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

			// Primary store currency
			// ====================================
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


			// Base measure weight
			// ====================================
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


			// Base dimension weight
			// ====================================
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

			// Shipping rate coputation methods
			// ====================================
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

			// Payment methods
			// ====================================
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

			// Incompatible plugins
			// ====================================
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

			// Validate write permissions (the same procedure like during installation)
			// ====================================
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
					NotifySuccess(T("Common.ShrinkDatabaseSuccessful"));
				}
			}
			catch (Exception ex)
			{
				NotifyError(ex);
			}

			return RedirectToReferrer();
		}

		public async Task<ActionResult> GarbageCollect()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			try
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				await Task.Delay(500);
				NotifySuccess(T("Admin.System.SystemInfo.GarbageCollectSuccessful"));
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

			_imageCache.Value.Clear();

			// get rid of cached image metadata
			_services.Cache.Clear();

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
			model.DeleteExportedFiles.NumberOfDeletedFolders = 0;

			var appPath = this.Request.PhysicalApplicationPath;

			string[] paths = new string[]
			{
				appPath + @"Exchange\",
				appPath + @"App_Data\Tenants\{0}\ExportProfiles\".FormatInvariant(DataSettings.Current.TenantName)
			};

			foreach (var path in paths)
			{
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

						if ((!startDateValue.HasValue || startDateValue.Value < info.CreationTimeUtc) &&
							(!endDateValue.HasValue || info.CreationTimeUtc < endDateValue.Value))
						{
							if (FileSystemHelper.Delete(fullPath))
								++model.DeleteExportedFiles.NumberOfDeletedFiles;
						}
					}
					catch (Exception exc)
					{
						NotifyError(exc, false);
					}
				}

				var dir = new DirectoryInfo(path);

				foreach (var dirInfo in dir.GetDirectories())
				{
					if ((!startDateValue.HasValue || startDateValue.Value < dirInfo.LastWriteTimeUtc) &&
						(!endDateValue.HasValue || dirInfo.LastWriteTimeUtc < endDateValue.Value))
					{
						FileSystemHelper.ClearDirectory(dirInfo.FullName, true);
						++model.DeleteExportedFiles.NumberOfDeletedFolders;
					}
				}
			}

			// clear unreferenced profile folders
			var importProfileFolders = _importProfileService.Value.GetImportProfiles()
				.Select(x => x.FolderName)
				.ToList();

			var infoImportProfiles = new DirectoryInfo(CommonHelper.MapPath(DataSettings.Current.TenantPath + "/" + "ImportProfiles"));

			foreach (var infoSubFolder in infoImportProfiles.GetDirectories())
			{
				if (!importProfileFolders.Contains(infoSubFolder.Name))
				{
					FileSystemHelper.ClearDirectory(infoSubFolder.FullName, true);
					++model.DeleteExportedFiles.NumberOfDeletedFolders;
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

                    NotifySuccess(T("Admin.System.Maintenance.SqlQuery.Succeeded"));
                }
                catch (Exception exception)
                {
					NotifyError(exception);
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
			_services.Cache.Clear();

			HttpContext.Cache.RemoveByPattern("*");

			this.NotifySuccess(_localizationService.GetResource("Admin.Common.TaskSuccessfullyProcessed"));

			if (previousUrl.HasValue())
			{
				return Redirect(previousUrl);
			}

			return RedirectToAction("Index", "Home");
        }

		public ActionResult ClearDatabaseCache(string previousUrl)
		{
			_dbCache.Clear();

			this.NotifySuccess(_localizationService.GetResource("Admin.Common.TaskSuccessfullyProcessed"));

			if (previousUrl.HasValue())
			{
				return Redirect(previousUrl);
			}			

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
			var model = new GridModel<GenericAttributeModel>();

			var storeId = _services.StoreContext.CurrentStore.Id;
			ViewBag.StoreId = storeId;

			if (_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
			{
				if (entityName.HasValue() && entityId > 0)
				{
					var attributes = _genericAttributeService.GetAttributesForEntity(entityId, entityName);

					model.Data = attributes
						.Where(x => x.StoreId == storeId || x.StoreId == 0)
						.Select(x => new GenericAttributeModel
						{
							Id = x.Id,
							EntityId = x.EntityId,
							EntityName = x.KeyGroup,
							Key = x.Key,
							Value = x.Value
						})
						.ToList();

					model.Total = model.Data.Count();
				}
				else
				{
					model.Data = Enumerable.Empty<GenericAttributeModel>();
				}
			}
			else
			{
				model.Data = Enumerable.Empty<GenericAttributeModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributeAdd(GenericAttributeModel model, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
			{
				model.Key = model.Key.TrimSafe();
				model.Value = model.Value.TrimSafe();

				if (!ModelState.IsValid)
				{
					var modelStateErrorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrorMessages.FirstOrDefault());
				}

				var storeId = _services.StoreContext.CurrentStore.Id;

				var attr = _genericAttributeService.GetAttribute<string>(model.EntityName, model.EntityId, model.Key, storeId);
				if (attr == null)
				{
					_genericAttributeService.InsertAttribute(new GenericAttribute
					{
						StoreId = storeId,
						KeyGroup = model.EntityName,
						EntityId = model.EntityId,
						Key = model.Key,
						Value = model.Value
					});
				}
				else
				{
					return Content(T("Admin.Common.GenericAttributes.NameAlreadyExists", model.Key));
				}
			}

            return GenericAttributesSelect(model.EntityName, model.EntityId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributeUpdate(GenericAttributeModel model, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
			{
				model.Key = model.Key.TrimSafe();
				model.Value = model.Value.TrimSafe();

				if (!ModelState.IsValid)
				{
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
						return Content(T("Admin.Common.GenericAttributes.NameAlreadyExists", model.Key));
					}
				}

				attr.Key = model.Key;
				attr.Value = model.Value;

				_genericAttributeService.UpdateAttribute(attr);
			}

            return GenericAttributesSelect(model.EntityName, model.EntityId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GenericAttributeDelete(int id, GridCommand command)
        {
            var attr = _genericAttributeService.GetAttributeById(id);

			if (_services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel))
			{
				_genericAttributeService.DeleteAttribute(attr);
			}

            return GenericAttributesSelect(attr.KeyGroup, attr.EntityId, command);
        }

        #endregion
    }
}
