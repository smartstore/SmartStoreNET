using System;
using System.IO;
using System.Threading;
using SmartStore.Core.Data;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Import manager interface
    /// </summary>
    public interface IImportManager
    {
		ImportResult ImportProductsFromExcel(
			Stream stream,
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
		public static ImportResult ImportProductsFromExcel(this IImportManager importManager, Stream stream)
		{
			return importManager.ImportProductsFromExcel(stream, CancellationToken.None);
		}
	}
}
