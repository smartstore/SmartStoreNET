using System.Collections.Generic;
using System.IO;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public interface IExportManager
    {
        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        string ExportManufacturersToXml(IList<Manufacturer> manufacturers);

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        string ExportCategoriesToXml();

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="customers">Customers</param>
        void ExportCustomersToXlsx(Stream stream, IList<Customer> customers);

        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        string ExportCustomersToXml(IList<Customer> customers);
    }
}
