using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Import
{
	public interface IImportProfileService
	{
		/// <summary>
		/// Inserts an import profile
		/// </summary>
		/// <param name="fileName">Name of the import file</param>
		/// <param name="name">Profile name</param>
		/// <param name="entityType"></param>
		/// <returns>Inserted import profile</returns>
		ImportProfile InsertImportProfile(string fileName, string name, ImportEntityType entityType);

		/// <summary>
		/// Updates an import profile
		/// </summary>
		/// <param name="profile">Import profile</param>
		void UpdateImportProfile(ImportProfile profile);

		/// <summary>
		/// Deletes an import profile
		/// </summary>
		/// <param name="profile">Import profile</param>
		void DeleteImportProfile(ImportProfile profile);

		/// <summary>
		/// Get queryable import profiles
		/// </summary>
		/// <param name="enabled">Whether to filter enabled or disabled profiles</param>
		/// <returns>Import profiles</returns>
		IQueryable<ImportProfile> GetImportProfiles(bool? enabled = null);

		/// <summary>
		/// Gets an import profile by identifier
		/// </summary>
		/// <param name="id">Import profile identifier</param>
		/// <returns>Import profile</returns>
		ImportProfile GetImportProfileById(int id);

		Dictionary<string, string> GetImportableEntityProperties(ImportEntityType entityType);
	}
}
