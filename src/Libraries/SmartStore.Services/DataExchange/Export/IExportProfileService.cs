using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Export
{
	public interface IExportProfileService
	{
		/// <summary>
		/// Inserts an export profile
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <param name="name">The name of the profile</param>
		/// <param name="fileExtension">File extension supported by provider</param>
		/// <param name="features">Features supportde by provider</param>
		/// <param name="isSystemProfile">Whether the new profile is a system profile</param>
		/// <param name="profileSystemName">Profile system name</param>
		/// <param name="cloneFromProfileId">Identifier of a profile the settings should be copied from</param>
		/// <returns>New export profile</returns>
		ExportProfile InsertExportProfile(
			string providerSystemName,
			string name,
			string fileExtension,
			ExportFeatures features,
			bool isSystemProfile = false,
			string profileSystemName = null,
			int cloneFromProfileId = 0);

		/// <summary>
		/// Inserts an export profile
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <param name="isSystemProfile">Whether the new profile is a system profile</param>
		/// <param name="profileSystemName">Profile system name</param>
		/// <param name="cloneFromProfileId">Identifier of a profile the settings should be copied from</param>
		/// <returns>New export profile</returns>
		ExportProfile InsertExportProfile(
			Provider<IExportProvider> provider,
			bool isSystemProfile = false,
			string profileSystemName = null,
			int cloneFromProfileId = 0);

		/// <summary>
		/// Updates an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		void UpdateExportProfile(ExportProfile profile);

		/// <summary>
		/// Deletes an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="force">Whether to delete system profiles</param>
		void DeleteExportProfile(ExportProfile profile, bool force = false);

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
		/// Gets system export profile by provider system name
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <returns></returns>
		ExportProfile GetSystemExportProfile(string providerSystemName);

		/// <summary>
		/// Gets export profiles by provider system name
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <returns>List of export profiles</returns>
		IList<ExportProfile> GetExportProfilesBySystemName(string providerSystemName);


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
		/// Update an export deployment
		/// </summary>
		/// <param name="deployment">Export deployment</param>
		void UpdateExportDeployment(ExportDeployment deployment);

		/// <summary>
		/// Deleted an export deployment
		/// </summary>
		/// <param name="deployment">Export deployment</param>
		void DeleteExportDeployment(ExportDeployment deployment);
	}
}
