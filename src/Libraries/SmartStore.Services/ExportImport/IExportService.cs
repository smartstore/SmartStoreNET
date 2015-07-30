using System.Linq;
using SmartStore.Core.Domain;

namespace SmartStore.Services.ExportImport
{
	public interface IExportService
	{
		/// <summary>
		/// Inserts an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		void InsertExportProfile(ExportProfile profile);

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
		/// <returns></returns>
		IQueryable<ExportProfile> GetExportProfiles(bool? enabled = null);
	}
}
