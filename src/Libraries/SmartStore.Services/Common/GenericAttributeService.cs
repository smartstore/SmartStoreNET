using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Data;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Generic attribute service
    /// </summary>
    public partial class GenericAttributeService : IGenericAttributeService
    {
        #region Constants
        
        private const string GENERICATTRIBUTE_KEY = "SmartStore.genericattribute.{0}-{1}";
        private const string GENERICATTRIBUTE_PATTERN_KEY = "SmartStore.genericattribute.";
        #endregion

        #region Fields

        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRequestCache _requestCache;
        private readonly IEventPublisher _eventPublisher;
		private readonly IRepository<Order> _orderRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="requestCache">Cache manager</param>
        /// <param name="genericAttributeRepository">Generic attribute repository</param>
        /// <param name="eventPublisher">Event published</param>
		/// <param name="orderRepository">Order repository</param>
        public GenericAttributeService(IRequestCache requestCache,
            IRepository<GenericAttribute> genericAttributeRepository,
            IEventPublisher eventPublisher,
			IRepository<Order> orderRepository)
        {
            this._requestCache = requestCache;
            this._genericAttributeRepository = genericAttributeRepository;
            this._eventPublisher = eventPublisher;
			this._orderRepository = orderRepository;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Deletes an attribute
        /// </summary>
        /// <param name="attribute">Attribute</param>
        public virtual void DeleteAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute");

			int entityId = attribute.EntityId;
			string keyGroup = attribute.KeyGroup;

            _genericAttributeRepository.Delete(attribute);

            //cache
            _requestCache.RemoveByPattern(GENERICATTRIBUTE_PATTERN_KEY);

            //event notifications
            _eventPublisher.EntityDeleted(attribute);

			if (keyGroup.IsCaseInsensitiveEqual("Order") && entityId != 0)
			{
				var order = _orderRepository.GetById(entityId);
				_eventPublisher.PublishOrderUpdated(order);
			}
        }

        /// <summary>
        /// Gets an attribute
        /// </summary>
        /// <param name="attributeId">Attribute identifier</param>
        /// <returns>An attribute</returns>
        public virtual GenericAttribute GetAttributeById(int attributeId)
        {
            if (attributeId == 0)
                return null;

            var attribute = _genericAttributeRepository.GetById(attributeId);
            return attribute;
        }

        /// <summary>
        /// Inserts an attribute
        /// </summary>
        /// <param name="attribute">attribute</param>
        public virtual void InsertAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute");

            _genericAttributeRepository.Insert(attribute);
            
            //cache
            _requestCache.RemoveByPattern(GENERICATTRIBUTE_PATTERN_KEY);

            //event notifications
            _eventPublisher.EntityInserted(attribute);

			if (attribute.KeyGroup.IsCaseInsensitiveEqual("Order") && attribute.EntityId != 0)
			{
				var order = _orderRepository.GetById(attribute.EntityId);
				_eventPublisher.PublishOrderUpdated(order);
			}
        }

        /// <summary>
        /// Updates the attribute
        /// </summary>
        /// <param name="attribute">Attribute</param>
        public virtual void UpdateAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute");

            _genericAttributeRepository.Update(attribute);

            //cache
            _requestCache.RemoveByPattern(GENERICATTRIBUTE_PATTERN_KEY);

            //event notifications
            _eventPublisher.EntityUpdated(attribute);

			if (attribute.KeyGroup.IsCaseInsensitiveEqual("Order") && attribute.EntityId != 0)
			{
				var order = _orderRepository.GetById(attribute.EntityId);
				_eventPublisher.PublishOrderUpdated(order);
			}
        }

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="keyGroup">Key group</param>
        /// <returns>Get attributes</returns>
		public virtual IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup)
        {
            string key = string.Format(GENERICATTRIBUTE_KEY, entityId, keyGroup);
            return _requestCache.Get(key, () =>
            {
                var query = from ga in _genericAttributeRepository.Table
                            where ga.EntityId == entityId &&
                            ga.KeyGroup == keyGroup
                            select ga;
                var attributes = query.ToList();
                return attributes;
            });
        }

		public virtual Multimap<int, GenericAttribute> GetAttributesForEntity(int[] entityIds, string keyGroup)
		{
			Guard.ArgumentNotNull(() => entityIds);

			var query = _genericAttributeRepository.TableUntracked
				.Where(x => entityIds.Contains(x.EntityId) && x.KeyGroup == keyGroup);

			var map = query
				.ToList()
				.ToMultimap(x => x.EntityId, x => x);

			return map;
		}

		/// <summary>
		/// Get queryable attributes
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="keyGroup">The key group</param>
		/// <returns>Queryable attributes</returns>
		public virtual IQueryable<GenericAttribute> GetAttributes(string key, string keyGroup)
		{
			var query =
				from ga in _genericAttributeRepository.Table
				where ga.Key == key && ga.KeyGroup == keyGroup
				select ga;

			return query;
		}

        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
		/// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
		public virtual void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, int storeId = 0)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

			SaveAttribute(entity.Id, key, entity.GetUnproxiedEntityType().Name, value, storeId);
        }

		public virtual void SaveAttribute<TPropType>(int entityId, string key, string keyGroup, TPropType value, int storeId = 0)
		{
			Guard.ArgumentNotZero(entityId, "entityId");

			var props = GetAttributesForEntity(entityId, keyGroup)
				 .Where(x => x.StoreId == storeId)
				 .ToList();

			var prop = props.FirstOrDefault(ga =>
				ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); // should be culture invariant

			string valueStr = value.Convert<string>();

			if (prop != null)
			{
				if (string.IsNullOrWhiteSpace(valueStr))
				{
					// delete
					DeleteAttribute(prop);
				}
				else
				{
					// update
					prop.Value = valueStr;
					UpdateAttribute(prop);
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(valueStr))
				{
					// insert
					prop = new GenericAttribute
					{
						EntityId = entityId,
						Key = key,
						KeyGroup = keyGroup,
						Value = valueStr,
						StoreId = storeId
					};
					InsertAttribute(prop);
				}
			}
		}

		#endregion
	}
}