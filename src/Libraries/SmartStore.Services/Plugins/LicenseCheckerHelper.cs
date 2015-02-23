using System;
using System.IO;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Licensing.Checker;
using SmartStore.Utilities;

namespace SmartStore.Services.Plugins
{
	public static class LicenseCheckerHelper
	{
		/// <summary>
		/// Initializes the license checker component
		/// </summary>
		public static void Init()
		{
			var languageSeoCode = EngineContext.Current.Resolve<IWorkContext>().GetDefaultLanguageSeoCode();

			// TODO: remove use sandbox flag!
			LicenseChecker.Init(Path.Combine(CommonHelper.MapPath("~/App_Data/"), "Licensing.key"), true, languageSeoCode);
		}

		/// <summary>
		/// Activates a license key
		/// </summary>
		/// <param name="descriptor">Plugin descriptor</param>
		/// <param name="key">License key</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="storeUrl">Store url</param>
		/// <returns>True: Succeeded or skiped, False: Failure</returns>
		public static bool Activate(PluginDescriptor descriptor, License license, string storeUrl, ICommonServices commonService)
		{
			var licensable = descriptor.Instance() as ILicensable;

			var data = new LicenseCheckerData
			{
				Key = license.LicenseKey,
				SystemName = descriptor.SystemName,
				Version = descriptor.Version.ToString(),
				HasSingleLicenseForAllVersions = licensable.HasSingleLicenseForAllVersions
			};

			if (!licensable.HasSingleLicenseForAllStores)
				data.Url = storeUrl;

			var result = LicenseChecker.Activate(data);

			if (result.Success)
			{
				commonService.Notifier.Success(new LocalizedString(commonService.Localization.GetResource("Admin.Configuration.Plugins.LicenseActivated")));
			}
			else
			{
				commonService.Notifier.Add(result.IsFailureWarning ? NotifyType.Warning : NotifyType.Error, new LocalizedString(result.ToString()));
			}

			return (result.Success || result.IsFailureWarning);
		}

		/// <summary>
		/// Checks for a license with active status against license checker component
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="failureMessage">Failure message if any</param>
		public static bool HasActiveLicense(PluginDescriptor descriptor, int storeId, out string failureMessage,
			ICommonServices commonService, ILicenseStorageService licenseStorageService, ILogger logger)
		{
			failureMessage = null;

			try
			{
				if (descriptor.IsLicensable)
				{
					var licenses = licenseStorageService.GetAllLicenses();
					var licensable = descriptor.Instance() as ILicensable;

					var license = licenses.FirstOrDefault(x => 
						x.SystemName == descriptor.SystemName &&
						(x.StoreId == storeId || x.StoreId == 0)
					);

					if (license == null)
					{
						failureMessage = commonService.Localization.GetResource("Admin.Plugins.NoLicenseFound").FormatWith(descriptor.SystemName);
						return false;
					}

					Version version = (licensable.HasSingleLicenseForAllVersions ? null : descriptor.Version);

					var data = new LicenseCheckerData
					{
						Key = license.LicenseKey,
						SystemName = descriptor.SystemName,
						Version = descriptor.Version.ToString(),
						HasSingleLicenseForAllVersions = licensable.HasSingleLicenseForAllVersions
					};

					if (!licensable.HasSingleLicenseForAllStores)
					{
						var store = commonService.StoreService.GetStoreById(storeId);

						data.Url = (store == null ? null : store.Url);
					}

					var result = LicenseChecker.Check(data);

					if (result.Success)
						return (result.Status == LicenseCheckerStatus.Active);
					
					failureMessage = result.ToString();
					return false;
				}
			}
			catch (Exception exc)
			{
				logger.Error(exc.Message, exc);
			}
			return true;	// do not bother merchant
		}

		/// <summary>
		/// Checks for a license with active status against license checker component
		/// </summary>
		/// <param name="descriptor">Plugin descriptor</param>
		/// <param name="storeId">Store identifier</param>
		public static bool HasActiveLicense(PluginDescriptor descriptor, int storeId,
			ICommonServices commonService, ILicenseStorageService licenseStorageService, ILogger logger)
		{
			string failureMessage;

			return HasActiveLicense(descriptor, storeId, out failureMessage, commonService, licenseStorageService, logger);
		}
	}
}
