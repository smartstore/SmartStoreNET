using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using NuGet;
using SmartStore.Core.Data;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.Packaging
{
    public sealed class AppUpdater : DisposableObject
    {
        public const string UpdatePackagePath = "~/App_Data/Update";

        private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private TraceLogger _logger;

        #region Package update

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public bool InstallablePackageExists()
        {
            string packagePath = null;
            var package = FindPackage(false, out packagePath);

            if (package == null)
                return false;

            if (!ValidatePackage(package))
                return false;

            if (!CheckEnvironment())
                return false;

            return true;
        }

        internal bool TryUpdateFromPackage()
        {
            // NEVER EVER (!!!) make an attempt to auto-update in a dev environment!!!!!!!
            if (CommonHelper.IsDevEnvironment)
                return false;

            using (_rwLock.GetUpgradeableReadLock())
            {
                try
                {
                    string packagePath = null;
                    var package = FindPackage(true, out packagePath);

                    if (package == null)
                        return false;

                    if (!ValidatePackage(package))
                        return false;

                    if (!CheckEnvironment())
                        return false;

                    using (_rwLock.GetWriteLock())
                    {
                        Backup();

                        var info = ExecuteUpdate(package);

                        if (info != null)
                        {
                            var newPath = packagePath + ".applied";
                            if (File.Exists(newPath))
                            {
                                File.Delete(packagePath);
                            }
                            else
                            {
                                File.Move(packagePath, newPath);
                            }
                        }

                        return info != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An error occured while updating the application");
                    return false;
                }
            }
        }

        private TraceLogger CreateLogger(IPackage package)
        {
            var logFile = Path.Combine(CommonHelper.MapPath(UpdatePackagePath, false), "Updater.{0}.log".FormatInvariant(package.Version.ToString()));
            return new TraceLogger(logFile);
        }

        private IPackage FindPackage(bool createLogger, out string path)
        {
            path = null;
            var dir = CommonHelper.MapPath(UpdatePackagePath, false);

            if (!Directory.Exists(dir))
                return null;

            var files = Directory.GetFiles(dir, "SmartStore.*.nupkg", SearchOption.TopDirectoryOnly);

            // TODO: allow more than one package in folder and return newest
            if (files.Length == 0 || files.Length > 1)
                return null;

            try
            {
                path = files[0];
                IPackage package = new ZipPackage(files[0]);
                if (createLogger)
                {
                    _logger = CreateLogger(package);
                    _logger.Info("Found update package '{0}'".FormatInvariant(package.GetFullName()));
                }
                return package;
            }
            catch { }

            return null;
        }

        private bool ValidatePackage(IPackage package)
        {
            if (package.Id != "SmartStore")
                return false;

            var currentVersion = new SemanticVersion(SmartStoreVersion.Version);
            return package.Version > currentVersion;
        }

        private bool CheckEnvironment()
        {
            // TODO: Check it :-)
            return true;
        }

        private void Backup()
        {
            var source = new DirectoryInfo(CommonHelper.MapPath("~/"));

            var tempPath = CommonHelper.MapPath("~/App_Data/_Backup/App/SmartStore");
            string localTempPath = null;
            for (int i = 0; i < 50; i++)
            {
                localTempPath = tempPath + (i == 0 ? "" : "." + i.ToString());
                if (!Directory.Exists(localTempPath))
                {
                    Directory.CreateDirectory(localTempPath);
                    break;
                }
                localTempPath = null;
            }

            if (localTempPath == null)
            {
                var exception = new SmartException("Too many backups in '{0}'.".FormatInvariant(tempPath));
                _logger.Error(exception);
                throw exception;
            }

            var backupFolder = new DirectoryInfo(localTempPath);
            var folderUpdater = new FolderUpdater(_logger);
            folderUpdater.Backup(source, backupFolder, "App_Data", "Media");

            _logger.Info("Backup successfully created in folder '{0}'.".FormatInvariant(localTempPath));
        }

        private PackageInfo ExecuteUpdate(IPackage package)
        {
            var appPath = CommonHelper.MapPath("~/");

            var logger = new NugetLogger(_logger);

            var project = new FileBasedProjectSystem(appPath) { Logger = logger };

            var nullRepository = new NullSourceRepository();

            var projectManager = new ProjectManager(
                nullRepository,
                new DefaultPackagePathResolver(appPath),
                project,
                nullRepository
                )
            { Logger = logger };

            // Perform the update
            projectManager.AddPackageReference(package, true, false);

            var info = new PackageInfo
            {
                Id = package.Id,
                Name = package.Title ?? package.Id,
                Version = package.Version.ToString(),
                Type = "App",
                Path = appPath
            };

            _logger.Info("Update '{0}' successfully executed.".FormatInvariant(info.Name));

            return info;
        }

        #endregion

        #region Migrations

        internal void ExecuteMigrations()
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            var currentVersion = SmartStoreVersion.Version;
            var prevVersion = DataSettings.Current.AppVersion ?? new Version(1, 0);

            if (prevVersion < new Version(3, 0, 0))
            {
                throw new ApplicationException($"Smartstore {currentVersion} does not support automatic upgrade from version {prevVersion}. Please upgrade to version 3.x first.");
            }

            TryMigrateDefaultTenant();

            if (prevVersion >= currentVersion)
                return;

            if (prevVersion < new Version(2, 1))
            {
                // we introduced app migrations in V2.1. So any version prior 2.1
                // has to perform the initial migration
                MigrateInitial();
            }

            if (prevVersion <= new Version(3, 1, 5, 0))
            {
                // We updated to Lucene.Net 4.8.
                DeleteSearchIndex();
            }

            DataSettings.Current.AppVersion = currentVersion;
            DataSettings.Current.Save();
        }

        private bool TryMigrateDefaultTenant()
        {
            // We introduced basic multi-tenancy in V3 [...]

            if (!IsPreTenancyVersion())
            {
                return false;
            }

            var tenantDir = Directory.CreateDirectory(CommonHelper.MapPath("~/App_Data/Tenants/Default"));
            var tenantTempDir = tenantDir.CreateSubdirectory("_temp");

            var appDataDir = CommonHelper.MapPath("~/App_Data");

            // Move Settings.txt
            File.Move(Path.Combine(appDataDir, "Settings.txt"), Path.Combine(tenantDir.FullName, "Settings.txt"));

            // Move InstalledPlugins.txt
            File.Move(Path.Combine(appDataDir, "InstalledPlugins.txt"), Path.Combine(tenantDir.FullName, "InstalledPlugins.txt"));

            // Move SmartStore.db.sdf
            var path = Path.Combine(appDataDir, "SmartStore.db.sdf");
            if (File.Exists(path))
            {
                File.Move(path, Path.Combine(tenantDir.FullName, "SmartStore.db.sdf"));
            }

            Func<string, string, bool> moveTenantFolder = (sourceFolder, targetFolder) =>
            {
                var sourcePath = Path.Combine(appDataDir, sourceFolder);

                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, Path.Combine(tenantDir.FullName, targetFolder ?? sourceFolder));
                    return true;
                }

                return false;
            };

            // Move tenant specific Folders
            moveTenantFolder("ImportProfiles", null);
            moveTenantFolder("ExportProfiles", null);
            moveTenantFolder("Indexing", null);
            moveTenantFolder("Lucene", null);
            moveTenantFolder("_temp\\BizBackups", null);
            moveTenantFolder("_temp\\ShopConnector", null);

            // Move all media files and folders to new subfolder "Default"
            var mediaInfos = (new DirectoryInfo(CommonHelper.MapPath("~/Media"))).EnumerateFileSystemInfos().Where(x => !x.Name.IsCaseInsensitiveEqual("Default"));
            var mediaFiles = mediaInfos.OfType<FileInfo>();
            var mediaDirs = mediaInfos.OfType<DirectoryInfo>().ToArray();
            var tenantMediaDir = new DirectoryInfo(CommonHelper.MapPath("~/Media/Default"));
            if (!tenantMediaDir.Exists)
            {
                tenantMediaDir.Create();
            }

            foreach (var dir in mediaDirs)
            {
                dir.MoveTo(Path.Combine(tenantMediaDir.FullName, dir.Name));
            }

            foreach (var file in mediaFiles)
            {
                file.MoveTo(Path.Combine(tenantMediaDir.FullName, file.Name));
            }

            return true;
        }

        private bool IsPreTenancyVersion()
        {
            var appDataDir = CommonHelper.MapPath("~/App_Data");

            return File.Exists(Path.Combine(appDataDir, "Settings.txt"))
                && File.Exists(Path.Combine(appDataDir, "InstalledPlugins.txt"))
                && !Directory.Exists(Path.Combine(appDataDir, "Tenants\\Default"));
        }

        private void MigrateInitial()
        {
            var installedPlugins = PluginFileParser.ParseInstalledPluginsFile();
            if (installedPlugins.Count == 0)
                return;

            var renamedPlugins = new List<string>();

            var pluginRenameMap = new Dictionary<string, string>
            {
                { "CurrencyExchange.ECB", null /* null means: remove it */ },
                { "CurrencyExchange.MoneyConverter", null },
                { "ExternalAuth.OpenId", null },
                { "Tax.Free", null },
                { "Api.WebApi", "SmartStore.WebApi" },
                { "DiscountRequirement.MustBeAssignedToCustomerRole", "SmartStore.DiscountRules" },
                { "DiscountRequirement.HadSpentAmount", "SmartStore.DiscountRules" },
                { "DiscountRequirement.HasAllProducts", "SmartStore.DiscountRules" },
                { "DiscountRequirement.HasOneProduct", "SmartStore.DiscountRules" },
                { "DiscountRequirement.Store", "SmartStore.DiscountRules" },
                { "DiscountRequirement.BillingCountryIs", "SmartStore.DiscountRules" },
                { "DiscountRequirement.ShippingCountryIs", "SmartStore.DiscountRules" },
                { "DiscountRequirement.HasPaymentMethod", "SmartStore.DiscountRules.HasPaymentMethod" },
                { "DiscountRequirement.HasShippingOption", "SmartStore.DiscountRules.HasShippingOption" },
                { "DiscountRequirement.PurchasedAllProducts", "SmartStore.DiscountRules.PurchasedProducts" },
                { "DiscountRequirement.PurchasedOneProduct", "SmartStore.DiscountRules.PurchasedProducts" },
                { "PromotionFeed.Froogle", "SmartStore.GoogleMerchantCenter" },
                { "PromotionFeed.Billiger", "SmartStore.Billiger" },
                { "PromotionFeed.ElmarShopinfo", "SmartStore.ElmarShopinfo" },
                { "PromotionFeed.Guenstiger", "SmartStore.Guenstiger" },
                { "Payments.AccardaKar", "SmartStore.AccardaKar" },
                { "Payments.AmazonPay", "SmartStore.AmazonPay" },
                { "Developer.DevTools", "SmartStore.DevTools" },
                { "ExternalAuth.Facebook", "SmartStore.FacebookAuth" },
                { "ExternalAuth.Twitter", "SmartStore.TwitterAuth" },
                { "SMS.Clickatell", "SmartStore.Clickatell" },
                { "Widgets.GoogleAnalytics", "SmartStore.GoogleAnalytics" },
                { "Misc.DemoShop", "SmartStore.DemoShopControlling" },
                { "Admin.OrderNumberFormatter", "SmartStore.OrderNumberFormatter" },
                { "Admin.Debitoor", "SmartStore.Debitoor" },
                { "Widgets.ETracker", "SmartStore.ETracker" },
                { "Payments.PayPalDirect", "SmartStore.PayPal" },
                { "Payments.PayPalStandard", "SmartStore.PayPal" },
                { "Payments.PayPalExpress", "SmartStore.PayPal" },
                { "Developer.Glimpse", "SmartStore.Glimpse" },
                { "Import.Biz", "SmartStore.BizImporter" },
                { "Payments.Sofortueberweisung", "SmartStore.Sofortueberweisung" },
                { "Payments.PostFinanceECommerce", "SmartStore.PostFinanceECommerce" },
                { "Misc.MailChimp", "SmartStore.MailChimp" },
                { "Mobile.SMS.Verizon", "SmartStore.Verizon" },
                { "Widgets.LivePersonChat", "SmartStore.LivePersonChat" },
                { "Payments.CashOnDelivery", "SmartStore.OfflinePayment" },
                { "Payments.Invoice", "SmartStore.OfflinePayment" },
                { "Payments.PayInStore", "SmartStore.OfflinePayment" },
                { "Payments.Prepayment", "SmartStore.OfflinePayment" },
                { "Payments.IPaymentCreditCard", "SmartStore.IPayment" },
                { "Payments.IPaymentDirectDebit", "SmartStore.IPayment" },
                { "Payments.AuthorizeNet", "SmartStore.AuthorizeNet" },
                { "Shipping.AustraliaPost", "SmartStore.AustraliaPost" },
                { "Shipping.CanadaPost", "SmartStore.CanadaPost" },
                { "Shipping.Fedex", "SmartStore.Fedex" },
                { "Shipping.UPS", "SmartStore.UPS" },
                { "Payments.Manual", "SmartStore.OfflinePayment" },
                { "Shipping.USPS", "SmartStore.USPS" },
                { "Widgets.TrustedShopsSeal", "SmartStore.TrustedShops" },
                { "Widgets.TrustedShopsCustomerReviews", "SmartStore.TrustedShops" },
                { "Widgets.TrustedShopsCustomerProtection", "SmartStore.TrustedShops" },
                { "Shipping.ByWeight", "SmartStore.ShippingByWeight" },
                { "Payments.DirectDebit", "SmartStore.OfflinePayment" },
                { "Tax.FixedRate", "SmartStore.Tax" },
                { "Tax.CountryStateZip", "SmartStore.Tax" },
                { "Shipping.ByTotal", "SmartStore.Shipping" },
                { "Shipping.FixedRate", "SmartStore.Shipping" }
            };

            foreach (var name in installedPlugins)
            {
                if (pluginRenameMap.ContainsKey(name))
                {
                    string newName = pluginRenameMap[name];
                    if (newName != null && !renamedPlugins.Contains(newName))
                    {
                        renamedPlugins.Add(newName);
                    }
                }
                else
                {
                    renamedPlugins.Add(name);
                }
            }

            PluginFileParser.SaveInstalledPluginsFile(renamedPlugins);
        }

        private void DeleteSearchIndex()
        {
            var tenantPath = CommonHelper.MapPath(DataSettings.Current.TenantPath);

            try
            {
                var indexingDir = new DirectoryInfo(Path.Combine(tenantPath, "Indexing"));
                if (indexingDir.Exists)
                {
                    indexingDir.Delete(true);
                }
            }
            catch { }

            try
            {
                var luceneDir = new DirectoryInfo(Path.Combine(tenantPath, "Lucene"));
                if (luceneDir.Exists)
                {
                    luceneDir.Delete(true);
                }
            }
            catch { }
        }

        #endregion

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (_logger != null)
                {
                    _logger.Dispose();
                    _logger = null;
                }
            }
        }
    }

}
