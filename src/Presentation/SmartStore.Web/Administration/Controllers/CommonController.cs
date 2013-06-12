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
using SmartStore.Core.Domain;
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
using Telerik.Web.Mvc;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Data;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CommonController : AdminControllerBase
    {
        #region Fields

        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IImageCache _imageCache; // codehint: sm-add
        private readonly SecuritySettings _securitySettings; // codehint: sm-add
        private readonly ITypeFinder _typeFinder; // codehint: sm-add
        private readonly IPluginFinder _pluginFinder; // codehint: sm-add

		private readonly static object _lock = new object();	// codehint: sm-add

        #endregion

        #region Constructors

        public CommonController(IPaymentService paymentService,
			IShippingService shippingService,
            ICurrencyService currencyService,
			IMeasureService measureService,
            ICustomerService customerService,
			IUrlRecordService urlRecordService, 
			IWebHelper webHelper,
			CurrencySettings currencySettings,
            MeasureSettings measureSettings,
			IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
			IWorkContext workContext,
			IStoreContext storeContext,
            IPermissionService permissionService,
			ILocalizationService localizationService,
            IImageCache imageCache,
			SecuritySettings securitySettings,
			ITypeFinder typeFinder,
            IPluginFinder pluginFinder)
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
            this._imageCache = imageCache; // codehint: sm-add
            this._securitySettings = securitySettings; // codehint: sm-add
            this._typeFinder = typeFinder; // codehint: sm-add
			this._pluginFinder = pluginFinder;	// codehint: sm-add
        }

        #endregion

        #region Methods

        #region AdminMenu

        [ChildActionOnly]
        public ActionResult Menu()
        {
            var cacheManager = EngineContext.Current.Resolve<ICacheManager>("sm_cache_static");

            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
            string cacheKey = string.Format("smartstore.pres.adminmenu.navigation-{0}-{1}", _workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds));

            var rootNode = cacheManager.Get(cacheKey, () =>
            {
				lock (_lock) {
					return PrepareAdminMenu();
				}
            });

            //var rootNode = PrepareAdminMenu();

            return PartialView(rootNode);
        }

        private TreeNode<MenuItem> PrepareAdminMenu()
        {
            SiteMapBase siteMap;
            if (!SiteMapManager.SiteMaps.TryGetValue("admin", out siteMap))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("admin", x => x.LoadFrom("~/Administration/sitemap.config"));
                siteMap = SiteMapManager.SiteMaps["admin"];
            }
            
            var rootNode = ConvertSitemapNodeToMenuItemNode(siteMap.RootNode);

            TreeNode<MenuItem> pluginNode = null;

            // "collect" menus from plugins
            if (!_securitySettings.HideAdminMenuItemsBasedOnPermissions || _permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
            {
                var providers = new List<IMenuProvider>();
                var providerTypes = _typeFinder.FindClassesOfType<IMenuProvider>();

                foreach (var type in providerTypes)
                {
                    var pluginDescriptor = _pluginFinder.GetPluginDescriptorByAssembly(type.Assembly, false);
                    if (pluginDescriptor != null && !pluginDescriptor.Installed)
                    {
                        continue;
                    }

                    try
                    {
                        var provider = Activator.CreateInstance(type) as IMenuProvider;
                        providers.Add(provider);
                    }
                    catch { }
                }

                if (providers.Any())
                {
                    var pluginItem = new MenuItem().ToBuilder()
                        .Text("Plugins")
                        .ResKey("Admin.Plugins")
                        .PermissionNames("ManagePlugins")
                        .ToItem();
                    pluginNode = rootNode.Append(pluginItem);

                    providers.Each(x => x.BuildMenu(pluginNode));
                }
            }

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

            // hide plugins node when no child is visible
            if (pluginNode != null)
            {
                if (!pluginNode.Children.Any(x => x.Value.Visible))
                {
                    pluginNode.Value.Visible = false;
                }
            }

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

            if (_securitySettings.HideAdminMenuItemsBasedOnPermissions && item.PermissionNames.HasValue())
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
			var currentStoreUrl = _storeContext.CurrentStore.Url;
			if (!String.IsNullOrEmpty(currentStoreUrl) &&
				(currentStoreUrl.Equals(_webHelper.GetStoreLocation(false), StringComparison.InvariantCultureIgnoreCase)
                ||
				currentStoreUrl.Equals(_webHelper.GetStoreLocation(true), StringComparison.InvariantCultureIgnoreCase)
                ))
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
            var perCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
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
            var pscCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
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
            var bWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
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
            var bDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
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

            //shipping rate coputation methods
            if (_shippingService.LoadActiveShippingRateComputationMethods()
                .Where(x => x.ShippingRateComputationMethodType == ShippingRateComputationMethodType.Offline)
                .Count() > 1)
                model.Add(new SystemWarningModel()
                {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.OnlyOneOffline")
                });

            //payment methods
            if (_paymentService.LoadActivePaymentMethods()
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

        public ActionResult Maintenance()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new MaintenanceModel();
            model.DeleteGuests.EndDate = DateTime.UtcNow.AddDays(-7);
            model.DeleteGuests.OnlyWithoutShoppingCart = true;

            // image cache stats (codehint: sm-add)
            long imageCacheFileCount = 0;
            long imageCacheTotalSize = 0;
            _imageCache.CacheStatistics(out imageCacheFileCount, out imageCacheTotalSize);
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

            _imageCache.DeleteCachedImages();

            return RedirectToAction("Maintenance");
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-guests")]
        public ActionResult MaintenanceDeleteGuests(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            DateTime? startDateValue = (model.DeleteGuests.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteGuests.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

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
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteExportedFiles.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            model.DeleteExportedFiles.NumberOfDeletedFiles = 0;
            string path = string.Format("{0}Content\\files\\exportimport\\", this.Request.PhysicalApplicationPath);
            foreach (var fullPath in System.IO.Directory.GetFiles(path))
            {
                try
                {
                    var fileName = Path.GetFileName(fullPath);
                    if (fileName.Equals("index.htm", StringComparison.InvariantCultureIgnoreCase))
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
                    ErrorNotification(exc, false);
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
                    dbContext.ExecuteSqlCommand(model.SqlQuery);
                    SuccessNotification("The sql command was executed successfully.");
                }
                catch (Exception ex)
                {
                    ErrorNotification("Error executing sql command: {0}".FormatCurrentUI(ex.Message));
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
            var model = new LanguageSelectorModel();
            model.CurrentLanguage = _workContext.WorkingLanguage.ToModel();
			model.AvailableLanguages = _languageService
				.GetAllLanguages(storeId: _storeContext.CurrentStore.Id)
				.Select(x => x.ToModel())
				.ToList();
            return PartialView("LanguageSelector", model);
        }

        public ActionResult ClearCache()
        {
            var cacheManager = new DefaultCacheManager(new StaticCache());
            cacheManager.Clear();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult RestartApplication()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            //restart application
            _webHelper.RestartAppDomain();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Searh engine friendly names
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
    }
}
