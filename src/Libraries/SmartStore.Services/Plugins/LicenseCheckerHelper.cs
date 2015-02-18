using System;
using System.IO;
using System.Linq;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Licensing.Checker;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
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
			// TODO: remove use sandbox flag!
			LicenseChecker.Init(Path.Combine(CommonHelper.MapPath("~/App_Data/"), "Licensing.key"), true);
		}

		/// <summary>
		/// Activates a license key
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="key">License key</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="storeUrl">Store url</param>
		/// <param name="failureMessage">Failure message if any</param>
		/// <returns>True: Succeeded or skiped, False: Failure</returns>
		public static bool Activate(string systemName, string key, int storeId, string storeUrl, out string failureMessage)
		{
			failureMessage = null;

			var result = LicenseChecker.Activate(key, storeUrl);

			if (result.Success)
			{
				var storageService = EngineContext.Current.Resolve<ILicenseStorageService>();

				storageService.InsertLicense(new License
				{
					LicenseKey = key,
					SystemName = systemName,
					ActivatedOnUtc = DateTime.UtcNow,
					StoreId = storeId
				});

				return true;
			}

			failureMessage = result.ToString();
			return false;
		}

		/// <summary>
		/// Checks for a license with active status against license checker component
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="failureMessage">Failure message if any</param>
		public static bool HasActiveLicense(string systemName, int storeId, out string failureMessage)
		{
			failureMessage = null;
			var engine = EngineContext.Current;

			try
			{
				var storageService = engine.Resolve<ILicenseStorageService>();
				var licenses = storageService.GetAllLicenses();
				var license = licenses.FirstOrDefault(x => x.SystemName == systemName && (x.StoreId == storeId || x.StoreId == 0));

				if (license == null)
				{
					failureMessage = engine.Resolve<ILocalizationService>().GetResource("Admin.Plugins.NoLicenseFound").FormatWith(systemName);
					return false;
				}

				var store = engine.Resolve<IStoreService>().GetStoreById(storeId);
				string storeUrl = (store == null ? null : store.Url);

				var result = LicenseChecker.Check(license.LicenseKey, storeUrl);

				if (!result.Success)
					failureMessage = result.ToString();

				return (result.Status == LicenseCheckerStatus.Active);
			}
			catch (Exception exc)
			{
				engine.Resolve<ILogger>().Error(exc.Message, exc);
			}

			return true;	// do not bother merchant
		}

		/// <summary>
		/// Checks for a license with active status against license checker component
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="storeId">Store identifier</param>
		public static bool HasActiveLicense(string systemName, int storeId)
		{
			string failureMessage;
			return HasActiveLicense(systemName, storeId, out failureMessage);
		}
	}
}
