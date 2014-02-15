using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core.Data;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Import manager interface
    /// </summary>
    public interface IImportManager
    {
        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
		ImportResult ImportProductsFromXlsx(Stream stream);

		Task<ImportResult> ImportProductsFromExcelAsync(
			Stream stream,
 			CancellationToken cancellationToken,
			IProgress<ImportProgressInfo> progress = null);
    }

	public static class IImportManagerExtensions
	{
		public static Task<ImportResult> ImportProductsFromExcelAsync(
			this IImportManager importManager,
			Stream stream,
			IProgress<ImportProgressInfo> progress = null)
		{
			return importManager.ImportProductsFromExcelAsync(stream, CancellationToken.None, progress);
		}
	}
}
