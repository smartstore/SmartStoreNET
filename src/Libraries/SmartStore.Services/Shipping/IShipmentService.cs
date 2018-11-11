using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Shipping
{
    /// <summary>
    /// Shipment service interface
    /// </summary>
    public partial interface IShipmentService
    {
        /// <summary>
        /// Deletes a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        void DeleteShipment(Shipment shipment);

        /// <summary>
        /// Search shipments
        /// </summary>
		/// <param name="trackingNumber">Search by tracking number</param>
        /// <param name="createdFrom">Created date from; null to load all records</param>
        /// <param name="createdTo">Created date to; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
		IPagedList<Shipment> GetAllShipments(string trackingNumber, DateTime? createdFrom, DateTime? createdTo, 
            int pageIndex, int pageSize);
        
        /// <summary>
        /// Get shipments by identifiers
        /// </summary>
        /// <param name="shipmentIds">Shipment identifiers</param>
        /// <returns>Shipments</returns>
        IList<Shipment> GetShipmentsByIds(int[] shipmentIds);

		/// <summary>
		/// Get shipments by order identifiers
		/// </summary>
		/// <param name="orderIds">Order identifiers</param>
		/// <returns>Shipments</returns>
		Multimap<int, Shipment> GetShipmentsByOrderIds(int[] orderIds);

        /// <summary>
        /// Gets a shipment
        /// </summary>
        /// <param name="shipmentId">Shipment identifier</param>
        /// <returns>Shipment</returns>
        Shipment GetShipmentById(int shipmentId);

        /// <summary>
        /// Inserts a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        void InsertShipment(Shipment shipment);

        /// <summary>
        /// Updates the shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        void UpdateShipment(Shipment shipment);



        /// <summary>
        /// Deletes a shipment item
        /// </summary>
		/// <param name="shipmentItem">Shipment item</param>
        void DeleteShipmentItem(ShipmentItem shipmentItem);

        /// <summary>
        /// Gets a shipment item
        /// </summary>
        /// <param name="shipmentItemId">shipment item identifier</param>
        /// <returns>shipment item</returns>
        ShipmentItem GetShipmentItemById(int shipmentItemId);

        /// <summary>
        /// Inserts a shipment item
        /// </summary>
        /// <param name="shipmentItem">shipment item</param>
        void InsertShipmentItem(ShipmentItem shipmentItem);

        /// <summary>
        /// Updates the shipment item
        /// </summary>
        /// <param name="shipmentItem">shipment item</param>
        void UpdateShipmentItem(ShipmentItem shipmentItem);
    }
}
