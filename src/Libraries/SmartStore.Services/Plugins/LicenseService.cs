using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Events;
using SmartStore.Licensing;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.Plugins
{
	public partial class LicenseService : ILicenseService
	{
		private const string LICENSE_ALL_KEY = "SmartStore.license.all";
		private const string LICENSE_PATTERN_KEY = "SmartStore.license.";

		private readonly IRepository<License> _licenseRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICacheManager _cacheManager;
		private readonly IStoreService _storeService;

		public LicenseService(
			IRepository<License> licenseRepository,
			IEventPublisher eventPublisher,
			ICacheManager cacheManager,
			IStoreService storeService)
		{
			_licenseRepository = licenseRepository;
			_eventPublisher = eventPublisher;
			_cacheManager = cacheManager;
			_storeService = storeService;

			// init licensing component
			LicensingService.UseSandbox = true;
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

			_eventPublisher.EntityDeleted(license);
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

			_eventPublisher.EntityInserted(license);
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

			_eventPublisher.EntityUpdated(license);
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
		/// <param name="licenseId">License identifier</param>
		/// <returns>License</returns>
		public License GetLicense(int licenseId)
		{
			if (licenseId == 0)
				return null;

			var licenses = GetAllLicenses();
			return licenses.FirstOrDefault(x => x.Id == licenseId);
		}

		/// <summary>
		/// Gets licenses by system name
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <returns>Licenses</returns>
		public IList<License> GetLicenses(string systemName)
		{
			var licenses = GetAllLicenses();
			return licenses.Where(x => x.SystemName == systemName).ToList();
		}

		/// <summary>
		/// Determines whether a plugin has an active license for all stores
		/// </summary>
		//public bool IsLicensedForAllStores(string systemName)
		//{
		//	var licenses = GetAllLicenses();

		//	if (licenses.FirstOrDefault(x => x.SystemName == systemName && x.StoreId == 0) != null)
		//		return true;

		//	foreach (var store in _storeService.GetAllStores())
		//	{
		//		if (licenses.FirstOrDefault(x => x.SystemName == systemName && x.StoreId == store.Id) == null)
		//			return false;
		//	}
		//	return true;
		//}

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
	}
}
