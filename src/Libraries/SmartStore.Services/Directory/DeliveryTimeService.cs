using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// DeliveryTime service
    /// </summary>
    public partial class DeliveryTimeService : IDeliveryTimeService
    {
        #region Constants
        private const string DELIVERYTIMES_ALL_KEY = "SMN.deliverytime.all";
        private const string DELIVERYTIMES_PATTERN_KEY = "SMN.deliverytime.";
        #endregion

        #region Fields

        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerService _customerService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;
		private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="currencyRepository">DeliveryTime repository</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="currencySettings">Currency settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="eventPublisher">Event published</param>
        public DeliveryTimeService(ICacheManager cacheManager,
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            ICustomerService customerService,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher,
			CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._deliveryTimeRepository = deliveryTimeRepository;
            this._customerService = customerService;
            this._pluginFinder = pluginFinder;
            this._eventPublisher = eventPublisher;
            this._productRepository = productRepository;
            this._attributeCombinationRepository = attributeCombinationRepository;
			this._catalogSettings = catalogSettings;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Deletes DeliveryTime
        /// </summary>
        /// <param name="currency">DeliveryTime</param>
        public virtual void DeleteDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
                throw new ArgumentNullException("deliveryTime");

            if (this.IsAssociated(deliveryTime.Id))
                throw new SmartException("The delivery time cannot be deleted. It has associated product variants");

            _deliveryTimeRepository.Delete(deliveryTime);

            _cacheManager.RemoveByPattern(DELIVERYTIMES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(deliveryTime);
        }

        public virtual bool IsAssociated(int deliveryTimeId)
        {
            if (deliveryTimeId == 0)
                return false;

            var query = 
				from p in _productRepository.Table
				where p.DeliveryTimeId == deliveryTimeId || p.ProductVariantAttributeCombinations.Any(c => c.DeliveryTimeId == deliveryTimeId)
				select p.Id;

            return query.Count() > 0;
        }

        /// <summary>
        /// Gets a DeliveryTime
        /// </summary>
        /// <param name="currencyId">DeliveryTime identifier</param>
        /// <returns>DeliveryTime</returns>
        public virtual DeliveryTime GetDeliveryTimeById(int deliveryTimeId)
        {
            if (deliveryTimeId == 0)
                return null;

            return  _deliveryTimeRepository.GetById(deliveryTimeId);
        }

		/// <summary>
		/// Gets the delivery time for a product
		/// </summary>
		/// <param name="product">The product</param>
		/// <returns>Delivery time</returns>
		public virtual DeliveryTime GetDeliveryTime(Product product)
		{
			if (product == null)
				return null;

			if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
				&& _catalogSettings.DeliveryTimeIdForEmptyStock.HasValue && product.StockQuantity <= 0)
			{
				return GetDeliveryTimeById(_catalogSettings.DeliveryTimeIdForEmptyStock.Value);
			}

			return GetDeliveryTimeById(product.DeliveryTimeId ?? 0);
		}

        /// <summary>
        /// Gets all delivery times
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>DeliveryTime collection</returns>
        public virtual IList<DeliveryTime> GetAllDeliveryTimes()
        {
            string key = string.Format(DELIVERYTIMES_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var query = _deliveryTimeRepository.Table;
                query = query.OrderBy(c => c.DisplayOrder);
                var deliveryTimes = query.ToList();
                return deliveryTimes;
            });
        }

        /// <summary>
        /// Inserts a DeliveryTime
        /// </summary>
        /// <param name="deliveryTime">DeliveryTime</param>
        public virtual void InsertDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
                throw new ArgumentNullException("deliveryTime");

            _deliveryTimeRepository.Insert(deliveryTime);

            _cacheManager.RemoveByPattern(DELIVERYTIMES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(deliveryTime);
        }

        /// <summary>
        /// Updates the DeliveryTime
        /// </summary>
        /// <param name="deliveryTime">DeliveryTime</param>
        public virtual void UpdateDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
                throw new ArgumentNullException("deliveryTime");

            _deliveryTimeRepository.Update(deliveryTime);

            _cacheManager.RemoveByPattern(DELIVERYTIMES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(deliveryTime);
        }

        #endregion

    }
}