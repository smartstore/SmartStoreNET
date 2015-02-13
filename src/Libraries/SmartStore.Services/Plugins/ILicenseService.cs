using System.Collections.Generic;
using SmartStore.Core.Domain.Plugins;

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
	}
}
