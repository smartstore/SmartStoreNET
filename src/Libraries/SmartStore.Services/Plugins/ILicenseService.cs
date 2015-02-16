using System.Collections.Generic;
using SmartStore.Core.Domain.Plugins;
using SmartStore.Licensing;

namespace SmartStore.Services.Plugins
{
	public partial interface ILicenseService
	{
		/// <summary>
		/// Deletes a license
		/// </summary>
		/// <param name="license">License</param>
		void DeleteLicense(License license);

		/// <summary>
		/// Inserts a license
		/// </summary>
		/// <param name="license">License</param>
		void InsertLicense(License license);

		/// <summary>
		/// Updates a license
		/// </summary>
		/// <param name="license">License</param>
		void UpdateLicense(License license);

		/// <summary>
		/// Gets all licenses
		/// </summary>
		/// <returns>Licenses</returns>
		IList<License> GetAllLicenses();

		/// <summary>
		/// Gets a license
		/// </summary>
		/// <param name="licenseId">License identifier</param>
		/// <returns>License</returns>
		License GetLicense(int licenseId);

		/// <summary>
		/// Gets licenses by system name
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <returns>Licenses</returns>
		IList<License> GetLicenses(string systemName);

		/// <summary>
		/// Activates a license key
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="key">License key</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="storeUrl">Store url</param>
		/// <param name="failureMessage">Failure message if any</param>
		/// <returns>True: Succeeded or skiped, False: Failure</returns>
		bool Activate(string systemName, string key, int storeId, string storeUrl, out string failureMessage);
	}
}
