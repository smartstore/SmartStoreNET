using System.Collections.Generic;
using System.IO;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;

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
		/// Writes a single product
		/// </summary>
		/// <param name="writer">The XML writer</param>
		/// <param name="product">The product</param>
		/// <param name="context">Context objects</param>
		void WriteProductToXml(XmlWriter writer, Product product, XmlExportContext context);

		/// <summary>
		/// Export product list to XML
		/// </summary>
		/// <param name="stream">Stream to write</param>
		/// <param name="searchContext">Search context</param>
		void ExportProductsToXml(Stream stream, ProductSearchContext searchContext);

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        void ExportProductsToXlsx(Stream stream, IList<Product> products);

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        string ExportOrdersToXml(IList<Order> orders);

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        void ExportOrdersToXlsx(Stream stream, IList<Order> orders);

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
