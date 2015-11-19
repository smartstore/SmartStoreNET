using System;
using System.Threading;

namespace SmartStore.Services.DataExchange.Import
{
    /// <summary>
    /// Import manager interface
    /// </summary>
    public interface IImportManager
    {
		ImportResult ImportProducts(
			IDataTable table,
			CancellationToken cancellationToken,
			IProgress<ImportProgressInfo> progress = null);

		/// <summary>
		/// Dumps an <see cref="ImportResult"/> instance to a string
		/// </summary>
		/// <param name="result">The result instance</param>
		/// <returns>The report</returns>
		string CreateTextReport(ImportResult result);
    }

	public static class IImportManagerExtensions
	{
		public static ImportResult ImportProducts(this IImportManager manager, IDataTable table)
		{
			return manager.ImportProducts(table, CancellationToken.None);
		}
	}
}
