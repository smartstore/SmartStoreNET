using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Orders
{
	public partial class CheckoutAttributeService : ICheckoutAttributeService
    {
        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IRepository<CheckoutAttributeValue> _checkoutAttributeValueRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IEventPublisher _eventPublisher;

        public CheckoutAttributeService(
            IRepository<CheckoutAttribute> checkoutAttributeRepository,
            IRepository<CheckoutAttributeValue> checkoutAttributeValueRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IEventPublisher eventPublisher)
        {
            _checkoutAttributeRepository = checkoutAttributeRepository;
            _checkoutAttributeValueRepository = checkoutAttributeValueRepository;
			_storeMappingRepository = storeMappingRepository;
            _eventPublisher = eventPublisher;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		#region Checkout attributes

		public virtual void DeleteCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            _checkoutAttributeRepository.Delete(checkoutAttribute);
        }

		public virtual IQueryable<CheckoutAttribute> GetCheckoutAttributes(int storeId = 0, bool showHidden = false)
		{
			var query = _checkoutAttributeRepository.Table;

			if (!showHidden)
				query = query.Where(x => x.IsActive);

			if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
			{
				query =
					from x in query
					join sm in _storeMappingRepository.Table on new { c1 = x.Id, c2 = "CheckoutAttribute" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into x_sm
					from sm in x_sm.DefaultIfEmpty()
					where !x.LimitedToStores || storeId == sm.StoreId
					select x;

				query =
					from x in query
					group x by x.Id into grp
					orderby grp.Key
					select grp.FirstOrDefault();
			}

			query = query.OrderBy(x => x.DisplayOrder);

			return query;
		}

		public virtual IList<CheckoutAttribute> GetAllCheckoutAttributes(int storeId = 0, bool showHidden = false)
        {
			var query = GetCheckoutAttributes(storeId, showHidden);
			return query.ToListCached("db.checkoutattrs.{0}.{1}".FormatInvariant(storeId, showHidden));
		}

        /// <summary>
        /// Gets a checkout attribute 
        /// </summary>
        /// <param name="checkoutAttributeId">Checkout attribute identifier</param>
        /// <returns>Checkout attribute</returns>
        public virtual CheckoutAttribute GetCheckoutAttributeById(int checkoutAttributeId)
        {
            if (checkoutAttributeId == 0)
                return null;

			return _checkoutAttributeRepository.GetByIdCached(checkoutAttributeId, "db.checkoutattr.id-" + checkoutAttributeId);
		}

        public virtual void InsertCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            _checkoutAttributeRepository.Insert(checkoutAttribute);
        }

        public virtual void UpdateCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            _checkoutAttributeRepository.Update(checkoutAttribute);
        }

        #endregion

        #region Checkout variant attribute values

        public virtual void DeleteCheckoutAttributeValue(CheckoutAttributeValue checkoutAttributeValue)
        {
            if (checkoutAttributeValue == null)
                throw new ArgumentNullException("checkoutAttributeValue");

            _checkoutAttributeValueRepository.Delete(checkoutAttributeValue);
        }

        public virtual IList<CheckoutAttributeValue> GetCheckoutAttributeValues(int checkoutAttributeId)
        {
			var query = from cav in _checkoutAttributeValueRepository.Table
						orderby cav.DisplayOrder
						where cav.CheckoutAttributeId == checkoutAttributeId
						select cav;
			var checkoutAttributeValues = query.ToListCached();
			return checkoutAttributeValues;
		}
        
        public virtual CheckoutAttributeValue GetCheckoutAttributeValueById(int checkoutAttributeValueId)
        {
            if (checkoutAttributeValueId == 0)
                return null;

			return _checkoutAttributeValueRepository.GetByIdCached(checkoutAttributeValueId, "db.checkoutattrval.id-" + checkoutAttributeValueId);
		}

        public virtual void InsertCheckoutAttributeValue(CheckoutAttributeValue checkoutAttributeValue)
        {
            if (checkoutAttributeValue == null)
                throw new ArgumentNullException("checkoutAttributeValue");

            _checkoutAttributeValueRepository.Insert(checkoutAttributeValue);
        }

        public virtual void UpdateCheckoutAttributeValue(CheckoutAttributeValue checkoutAttributeValue)
        {
            if (checkoutAttributeValue == null)
                throw new ArgumentNullException("checkoutAttributeValue");

            _checkoutAttributeValueRepository.Update(checkoutAttributeValue);
        }
        
        #endregion
    }
}
