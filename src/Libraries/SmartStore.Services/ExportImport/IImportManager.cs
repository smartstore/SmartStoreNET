using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Async;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Import manager interface
    /// </summary>
    public interface IImportManager
    {
		Task<ImportResult> ImportProductsFromExcelAsync(
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
		public static Task<ImportResult> ImportProductsFromExcelAsync(
			this IImportManager importManager,
			Stream stream,
			IProgress<ImportProgressInfo> progress = null)
		{
			return importManager.ImportProductsFromExcelAsync(stream, CancellationToken.None, progress);
		}

		public static ImportResult ImportProductsFromExcel(this IImportManager importManager, Stream stream)
		{
			//Func<Task<ImportResult>> fn = () => importManager.ImportProductsFromExcelAsync(stream);
			//return fn.RunSync();
			return AsyncRunner.RunSync(() => importManager.ImportProductsFromExcelAsync(stream));
		}
	}
}
