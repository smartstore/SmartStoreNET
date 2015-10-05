using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public interface IExportService
	{
		/// <summary>
		/// Creates a volatile export project
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <returns>New export profile</returns>
		ExportProfile CreateVolatileProfile(Provider<IExportProvider> provider);

		/// <summary>
		/// Inserts an export profile
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <param name="cloneFromProfileId">Identifier of a profile the settings should be copied from</param>
		/// <returns>New export profile</returns>
		ExportProfile InsertExportProfile(Provider<IExportProvider> provider, int cloneFromProfileId = 0);

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
		/// Gets export profiles by provider system name
		/// </summary>
		/// <param name="systemName">Provider system name</param>
		/// <returns>List of export profiles</returns>
		IList<ExportProfile> GetExportProfilesBySystemName(string systemName);


		/// <summary>
		/// Load all export providers
		/// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <param name="showHidden">Whether to load hidden providers</param>
		/// <returns>Export providers</returns>
		IEnumerable<Provider<IExportProvider>> LoadAllExportProviders(int storeId = 0, bool showHidden = true);

		/// <summary>
		/// Load export provider by system name
		/// </summary>
		/// <param name="systemName">Provider system name</param>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Export provider</returns>
		Provider<IExportProvider> LoadProvider(string systemName, int storeId = 0);

		/// <summary>
		/// Get export deployment by identifier
		/// </summary>
		/// <param name="id">Export deployment identifier</param>
		/// <returns>Export deployment</returns>
		ExportDeployment GetExportDeploymentById(int id);

		/// <summary>
		/// Deleted a export deployment
		/// </summary>
		/// <param name="deployment">Export deployment</param>
		void DeleteExportDeployment(ExportDeployment deployment);
	}
}
