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
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="customers">Customers</param>
        void ExportCustomersToXlsx(Stream stream, IList<Customer> customers);
    }
}
