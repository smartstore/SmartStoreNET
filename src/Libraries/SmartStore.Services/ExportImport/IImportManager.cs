
using System.IO;
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
    }
}
