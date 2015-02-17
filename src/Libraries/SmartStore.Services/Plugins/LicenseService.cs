using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Licensing;
using SmartStore.Utilities;

namespace SmartStore.Services.Plugins
{
	public partial class LicenseService : ILicenseService
	{
		private const string LICENSE_ALL_KEY = "SmartStore.license.all";
		private const string LICENSE_PATTERN_KEY = "SmartStore.license.";

		private readonly IRepository<License> _licenseRepository;
		private readonly ICacheManager _cacheManager;
		private readonly ICommonServices _commonService;
		private readonly ILogger _logger;

		public LicenseService(
			IRepository<License> licenseRepository,
			ICacheManager cacheManager,
			ICommonServices commonService,
			ILogger logger)
		{
			_licenseRepository = licenseRepository;
			_cacheManager = cacheManager;
			_commonService = commonService;
			_logger = logger;

			// init licensing component
			LicensingService.UseSandbox = true;		// TODO: remove!
			LicensingService.LocalFilePath = Path.Combine(CommonHelper.MapPath("~/App_Data/"), "Licensing.key");
		}

		/// <summary>
		/// Deletes a license
		/// </summary>
		/// <param name="license">License</param>
		public void DeleteLicense(License license)
		{
			if (license == null)
				return;

			_licenseRepository.Delete(license);

			_cacheManager.RemoveByPattern(LICENSE_PATTERN_KEY);

			_commonService.EventPublisher.EntityDeleted(license);
		}

		/// <summary>
		/// Inserts a license
		/// </summary>
		/// <param name="license">License</param>
		public void InsertLicense(License license)
		{
			if (license == null || license.LicenseKey.IsEmpty())
				throw new ArgumentNullException("license");

			var licenses = GetAllLicenses();
			if (licenses.FirstOrDefault(x => x.LicenseKey == license.LicenseKey) != null)
				return;		// only one record per key

			_licenseRepository.Insert(license);

			_cacheManager.RemoveByPattern(LICENSE_PATTERN_KEY);

			_commonService.EventPublisher.EntityInserted(license);
		}

		/// <summary>
		/// Updates a license
		/// </summary>
		/// <param name="license">License</param>
		public void UpdateLicense(License license)
		{
			if (license == null)
				throw new ArgumentNullException("license");

			_licenseRepository.Update(license);

			_cacheManager.RemoveByPattern(LICENSE_PATTERN_KEY);

			_commonService.EventPublisher.EntityUpdated(license);
		}

		/// <summary>
		/// Gets all licenses
		/// </summary>
		/// <returns>Licenses</returns>
		public IList<License> GetAllLicenses()
		{
			return _cacheManager.Get(LICENSE_ALL_KEY, () =>
			{
				var licenses = _licenseRepository.Table.ToList();
				return licenses;
			});
		}

		/// <summary>
		/// Gets a license
		/// </summary>
		/// <param name="key">License key</param>
		/// <returns>License</returns>
		public License GetLicense(string key)
		{
			if (key.HasValue())
			{
				var licenses = GetAllLicenses();
				return licenses.FirstOrDefault(x => x.LicenseKey == key);
			}
			return null;
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
		public bool Activate(string systemName, string key, int storeId, string storeUrl, out string failureMessage)
		{
			failureMessage = null;

			var licensing = LicensingService.GetLicensing(key);
			if (licensing != null && licensing.Status == LicensingStatus.Active)
				return true;

			var result = LicensingService.Activate(key, storeUrl);
			if (result.Success)
			{
				InsertLicense(new License
				{
					LicenseKey = key,
					UsageId = result.ResponseMembers["USAGE_ID"],
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
		/// Checks for a license with active status
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="failureMessage">Failure message if any</param>
		public bool HasActiveLicense(string systemName, int storeId, out string failureMessage)
		{
			failureMessage = null;

			try
			{
				var licenses = GetAllLicenses();
				var license = licenses.FirstOrDefault(x => x.SystemName == systemName && (x.StoreId == storeId || x.StoreId == 0));

				if (license == null)
				{
					failureMessage = _commonService.Localization.GetResource("Admin.Plugins.NoLicenseFound").FormatWith(systemName);
					return false;
				}

				var store = _commonService.StoreService.GetStoreById(storeId);
				string storeUrl = (store == null ? null : store.Url);

				var result = LicensingService.Check(license.LicenseKey, storeUrl, license.UsageId);

				if (!result.Success)
					failureMessage = result.ToString();

				return (result.Status == LicensingStatus.Active);
			}
			catch (Exception exc)
			{
				_logger.Error(exc.Message, exc);
			}
			return true;	// do not bother merchant
		}

		/// <summary>
		/// Checks for a license with active status
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="storeId">Store identifier</param>
		public bool HasActiveLicense(string systemName, int storeId)
		{
			string failureMessage;
			return HasActiveLicense(systemName, storeId, out failureMessage);
		}
	}
}
