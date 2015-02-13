using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Events;
using SmartStore.Services.Stores;

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
			if (license == null)
				throw new ArgumentNullException("license");

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

			var license = GetAllLicenses().FirstOrDefault(x => x.Id == licenseId);
			return license;
		}

		/// <summary>
		/// Gets licenses by system name
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <returns>Licenses</returns>
		public IList<License> GetLicenses(string systemName)
		{
			var licenses = GetAllLicenses().Where(x => x.SystemName == systemName);
			return licenses.ToList();
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
	}
}
