﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Logging;
using Log = SmartStore.Core.Logging;
using NuGet;
using NuGetPackageManager = NuGet.PackageManager;
using SmartStore.Core.Data;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Packaging
{
	
	public sealed class AppUpdater : DisposableObject
	{
		public const string UpdatePackagePath = "~/App_Data/Update";
		
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		private TraceLogger _logger;

		#region Package update

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
					_logger.Error("An error occured while updating the application: {0}".FormatCurrent(ex.Message), ex);
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
			if (files == null || files.Length == 0 || files.Length > 1)
				return null;

			IPackage package = null;

			try
			{
				path = files[0];
				package = new ZipPackage(files[0]);
				if (createLogger)
				{
					_logger = CreateLogger(package);
					_logger.Information("Found update package '{0}'".FormatInvariant(package.GetFullName()));
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
				_logger.Error(exception.Message, exception);
				throw exception;
			}

			var backupFolder = new DirectoryInfo(localTempPath);
			var folderUpdater = new FolderUpdater(_logger);
			folderUpdater.Backup(source, backupFolder, "App_Data", "Media");

			_logger.Information("Backup successfully created in folder '{0}'.".FormatInvariant(localTempPath));
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
				) { Logger = logger };

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

			_logger.Information("Update '{0}' successfully executed.".FormatInvariant(info.Name));

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

			if (prevVersion >= currentVersion)
				return;

			if (prevVersion < new Version(2, 1))
			{
				// we introduced app migrations in V2.1. So any version prior 2.1
				// has to perform the initial migration
				MigrateInitial();
			}

			DataSettings.Current.AppVersion = currentVersion;
			DataSettings.Current.Save();
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
