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
    /// QuantityUnit service
    /// </summary>
    public partial class QuantityUnitService : IQuantityUnitService
    {
        #region Constants
        private const string MEASUREUNITS_ALL_KEY = "SMN.quantityunits.all";
        private const string MEASUREUNITS_PATTERN_KEY = "SMN.quantityunit.";
        #endregion

        #region Fields

        private readonly IRepository<QuantityUnit> _quantityUnitRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
		private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="currencyRepository">QuantityUnit repository</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="currencySettings">Currency settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="eventPublisher">Event published</param>
        public QuantityUnitService(ICacheManager cacheManager,
            IRepository<QuantityUnit> quantityUnitRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
			CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._quantityUnitRepository = quantityUnitRepository;
            this._eventPublisher = eventPublisher;
            this._productRepository = productRepository;
			this._catalogSettings = catalogSettings;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Deletes QuantityUnit
        /// </summary>
        /// <param name="quantityUnit">QuantityUnit</param>
        public virtual void DeleteQuantityUnit(QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
                throw new ArgumentNullException("quantityUnit");

            if (this.IsAssociated(quantityUnit.Id))
                throw new SmartException("The quantity unit cannot be deleted. It has associated product variants");

            _quantityUnitRepository.Delete(quantityUnit);

            _cacheManager.RemoveByPattern(MEASUREUNITS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(quantityUnit);
        }

        public virtual bool IsAssociated(int quantityUnitId)
        {
            if (quantityUnitId == 0)
                return false;

            var query = 
				from p in _productRepository.Table
                where p.QuantityUnitId == quantityUnitId || p.ProductVariantAttributeCombinations.Any(c => c.QuantityUnitId == quantityUnitId)
				select p.Id;

            return query.Count() > 0;
        }

        /// <summary>
        /// Gets a QuantityUnit
        /// </summary>
        /// <param name="quantityUnitId">QuantityUnit identifier</param>
        /// <returns>QuantityUnit</returns>
        public virtual QuantityUnit GetQuantityUnitById(int? quantityUnitId)
        {
			if (quantityUnitId == null || quantityUnitId == 0)
            {
                if(_catalogSettings.ShowDefaultQuantityUnit)
                {
                    return GetAllQuantityUnits().Where(x => x.IsDefault == true).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
            
            return _quantityUnitRepository.GetById(quantityUnitId);
        }

		/// <summary>
		/// Gets the measure unit for a product
		/// </summary>
		/// <param name="product">The product</param>
        /// <returns>QuantityUnit</returns>
        public virtual QuantityUnit GetQuantityUnit(Product product)
		{
			if (product == null)
				return null;

            return GetQuantityUnitById(product.QuantityUnitId ?? 0);
		}

        /// <summary>
        /// Gets all measure units
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>QuantityUnit collection</returns>
        public virtual IList<QuantityUnit> GetAllQuantityUnits()
        {
            string key = string.Format(MEASUREUNITS_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var query = _quantityUnitRepository.Table;
                query = query.OrderBy(c => c.DisplayOrder);
                var quantityUnits = query.ToList();
                return quantityUnits;
            });
        }

        /// <summary>
        /// Inserts a QuantityUnit
        /// </summary>
        /// <param name="quantityUnit">QuantityUnit</param>
        public virtual void InsertQuantityUnit(QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
                throw new ArgumentNullException("quantityUnit");

            _quantityUnitRepository.Insert(quantityUnit);

            _cacheManager.RemoveByPattern(MEASUREUNITS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(quantityUnit);
        }

        /// <summary>
        /// Updates the QuantityUnit
        /// </summary>
        /// <param name="quantityUnit">QuantityUnit</param>
        public virtual void UpdateQuantityUnit(QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
                throw new ArgumentNullException("quantityUnit");

            if (quantityUnit.IsDefault == true) {

                var temp = new List<QuantityUnit>();
                temp.Add(quantityUnit);

                var query = GetAllQuantityUnits()
                    .Where(x => x.IsDefault == true)
                    .Except(temp);
                
                foreach(var qu in query) 
                {
                    qu.IsDefault = false;
                    _quantityUnitRepository.Update(qu);
                }
            }

            _quantityUnitRepository.Update(quantityUnit);

            _cacheManager.RemoveByPattern(MEASUREUNITS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(quantityUnit);
        }

        #endregion

    }
}