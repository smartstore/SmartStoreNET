using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public interface IExportService
	{
		/// <summary>
		/// Inserts an export profile
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <returns>New export profile</returns>
		ExportProfile InsertExportProfile(Provider<IExportProvider> provider);

		/// <summary>
		/// Updates an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		void UpdateExportProfile(ExportProfile profile);

		/// <summary>
		/// Deletes an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		void DeleteExportProfile(ExportProfile profile);

		/// <summary>
		/// Get queryable export profiles
		/// </summary>
		/// <param name="enabled">Whether to filter enabled or disabled profiles</param>
		/// <returns>Export profiles</returns>
		IQueryable<ExportProfile> GetExportProfiles(bool? enabled = null);

		/// <summary>
		/// Gets an export profile by identifier
		/// </summary>
		/// <param name="id">Export profile identifier</param>
		/// <returns>Export profile</returns>
		ExportProfile GetExportProfileById(int id);


		/// <summary>
		/// Load all export providers
		/// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Export providers</returns>
		IEnumerable<Provider<IExportProvider>> LoadAllExportProviders(int storeId = 0);

		/// <summary>
		/// Load export provider by system name
		/// </summary>
		/// <param name="systemName">Provider system name</param>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Export provider</returns>
		Provider<IExportProvider> LoadProvider(string systemName, int storeId = 0);


		/// <summary>
		/// Deletes an export deployment
		/// </summary>
		/// <param name="deployment">Export deployment</param>
		void DeleteExportDeployment(ExportDeployment deployment);
	}
}
