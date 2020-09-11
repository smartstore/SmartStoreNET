using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Events;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Shipping
{
    /// <summary>
    /// Shipment service
    /// </summary>
    public partial class ShipmentService : IShipmentService
    {
        #region Fields

        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<ShipmentItem> _siRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shipmentRepository">Shipment repository</param>
        /// <param name="siRepository">shipment item repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ShipmentService(IRepository<Shipment> shipmentRepository,
            IRepository<ShipmentItem> siRepository,
            IRepository<Order> orderRepository,
            IEventPublisher eventPublisher)
        {
            this._shipmentRepository = shipmentRepository;
            this._siRepository = siRepository;
            this._orderRepository = orderRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual void DeleteShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            int orderId = shipment.OrderId;

            _shipmentRepository.Delete(shipment);

            if (orderId != 0)
            {
                var order = _orderRepository.GetById(orderId);
                _eventPublisher.PublishOrderUpdated(order);
            }
        }

        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="trackingNumber">Search by tracking number</param>
        /// <param name="createdFrom">Created date from; null to load all records</param>
        /// <param name="createdTo">Created date to; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        public virtual IPagedList<Shipment> GetAllShipments(string trackingNumber, DateTime? createdFrom, DateTime? createdTo,
            int pageIndex, int pageSize)
        {
            var query = _shipmentRepository.Table.Expand(x => x.Order);
            if (!String.IsNullOrEmpty(trackingNumber))
                query = query.Where(s => s.TrackingNumber.Contains(trackingNumber));
            if (createdFrom.HasValue)
                query = query.Where(s => createdFrom.Value <= s.CreatedOnUtc);
            if (createdTo.HasValue)
                query = query.Where(s => createdTo.Value >= s.CreatedOnUtc);
            query = query.Where(s => s.Order != null && !s.Order.Deleted);
            query = query.OrderByDescending(s => s.CreatedOnUtc);

            var shipments = new PagedList<Shipment>(query, pageIndex, pageSize);
            return shipments;
        }

        /// <summary>
        /// Get shipment by identifiers
        /// </summary>
        /// <param name="shipmentIds">Shipment identifiers</param>
        /// <returns>Shipments</returns>
        public virtual IList<Shipment> GetShipmentsByIds(int[] shipmentIds)
        {
            if (shipmentIds == null || shipmentIds.Length == 0)
                return new List<Shipment>();

            var query = from o in _shipmentRepository.Table.Expand(x => x.Order)
                        where shipmentIds.Contains(o.Id)
                        select o;

            var shipments = query.ToList();

            // sort by passed identifier sequence
            return shipments.OrderBySequence(shipmentIds).ToList();
        }

        public virtual Multimap<int, Shipment> GetShipmentsByOrderIds(int[] orderIds)
        {
            Guard.NotNull(orderIds, nameof(orderIds));

            var query =
                from x in _shipmentRepository.TableUntracked.Expand(x => x.ShipmentItems)
                where orderIds.Contains(x.OrderId)
                select x;

            var map = query
                .OrderBy(x => x.OrderId)
                .ThenBy(x => x.CreatedOnUtc)
                .ToList()
                .ToMultimap(x => x.OrderId, x => x);

            return map;
        }

        /// <summary>
        /// Gets a shipment
        /// </summary>
        /// <param name="shipmentId">Shipment identifier</param>
        /// <returns>Shipment</returns>
        public virtual Shipment GetShipmentById(int shipmentId)
        {
            if (shipmentId == 0)
                return null;

            var shipment = _shipmentRepository.GetById(shipmentId);
            return shipment;
        }

        /// <summary>
        /// Inserts a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual void InsertShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            _shipmentRepository.Insert(shipment);

            //event notification
            _eventPublisher.PublishOrderUpdated(shipment.Order);
        }

        /// <summary>
        /// Updates the shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual void UpdateShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            _shipmentRepository.Update(shipment);

            //event notification
            _eventPublisher.PublishOrderUpdated(shipment.Order);
        }



        /// <summary>
        /// Deletes a shipment item
        /// </summary>
        /// <param name="shipmentItem">Shipment item</param>
        public virtual void DeleteShipmentItem(ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");

            int orderId = shipmentItem.Shipment.OrderId;

            _siRepository.Delete(shipmentItem);

            if (orderId != 0)
            {
                var order = _orderRepository.GetById(orderId);
                _eventPublisher.PublishOrderUpdated(order);
            }
        }

        /// <summary>
        /// Gets a shipment item
        /// </summary>
        /// <param name="shipmentItemId">shipment item identifier</param>
        /// <returns>shipment item</returns>
        public virtual ShipmentItem GetShipmentItemById(int shipmentItemId)
        {
            if (shipmentItemId == 0)
                return null;

            var si = _siRepository.GetById(shipmentItemId);
            return si;
        }

        /// <summary>
        /// Inserts a shipment item
        /// </summary>
        /// <param name="shipmentItem">shipment item</param>
        public virtual void InsertShipmentItem(ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");

            _siRepository.Insert(shipmentItem);

            if (shipmentItem.Shipment != null && shipmentItem.Shipment.Order != null)
            {
                _eventPublisher.PublishOrderUpdated(shipmentItem.Shipment.Order);
            }
            else
            {
                var shipment = _shipmentRepository.Table
                    .Expand(x => x.Order)
                    .FirstOrDefault(x => x.Id == shipmentItem.ShipmentId);

                if (shipment != null)
                    _eventPublisher.PublishOrderUpdated(shipment.Order);
            }
        }

        /// <summary>
        /// Updates the shipment item
        /// </summary>
        /// <param name="shipmentItem">shipment item</param>
        public virtual void UpdateShipmentItem(ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");

            _siRepository.Update(shipmentItem);

            if (shipmentItem.Shipment != null && shipmentItem.Shipment.Order != null)
            {
                _eventPublisher.PublishOrderUpdated(shipmentItem.Shipment.Order);
            }
            else
            {
                var shipment = _shipmentRepository.Table
                    .Expand(x => x.Order)
                    .FirstOrDefault(x => x.Id == shipmentItem.ShipmentId);

                if (shipment != null)
                    _eventPublisher.PublishOrderUpdated(shipment.Order);
            }
        }

        #endregion
    }
}
