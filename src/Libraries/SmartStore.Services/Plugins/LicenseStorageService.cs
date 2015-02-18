using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Core.Events;

namespace SmartStore.Services.Plugins
{
	public partial class LicenseStorageService : ILicenseStorageService
	{
		private const string LICENSE_ALL_KEY = "SmartStore.license.all";
		private const string LICENSE_PATTERN_KEY = "SmartStore.license.";

		private readonly IRepository<License> _licenseRepository;
		private readonly ICacheManager _cacheManager;
		private readonly ICommonServices _commonService;

		public LicenseStorageService(
			IRepository<License> licenseRepository,
			ICacheManager cacheManager,
			ICommonServices commonService)
		{
			_licenseRepository = licenseRepository;
			_cacheManager = cacheManager;
			_commonService = commonService;
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
		/// <returns>All licenses</returns>
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
	}
}
