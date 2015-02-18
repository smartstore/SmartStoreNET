using System.Collections.Generic;
using SmartStore.Core.Domain.Plugins;

namespace SmartStore.Services.Plugins
{
	public partial interface ILicenseStorageService
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
		/// <param name="key">License key</param>
		/// <returns>License</returns>
		License GetLicense(string key);
	}
}
