using System.Collections.Generic;
using System.IO;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface IPdfService
    {
        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="lang">Language</param>
        void PrintOrdersToPdf(Stream stream, IList<Order> orders, Language lang);

		/// <summary>
		/// Print an order to PDF
		/// </summary>
		/// <param name="orders">Orders</param>
		byte[] PrintOrdersToPdf(IList<Order> orders);


        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="lang">Language</param>
        void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, Language lang);

		/// <summary>
		/// Print packaging slips to PDF
		/// </summary>
		/// <param name="shipments">Shipments</param>
		byte[] PrintPackagingSlipsToPdf(IList<Shipment> shipments);


        /// <summary>
        /// Print product collection to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        /// <param name="lang">Language</param>
        void PrintProductsToPdf(Stream stream, IList<Product> products, Language lang);

		/// <summary>
		/// Print product collection to PDF
		/// </summary>
		/// <param name="products">Products</param>
		byte[] PrintProductsToPdf(IList<Product> products);
    }
}