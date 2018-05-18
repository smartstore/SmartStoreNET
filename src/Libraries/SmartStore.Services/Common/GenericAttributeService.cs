using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Services.Orders;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Common
{
    public partial class GenericAttributeService : IGenericAttributeService
    {
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IEventPublisher _eventPublisher;
		private readonly IRepository<Order> _orderRepository;

        public GenericAttributeService(
            IRepository<GenericAttribute> genericAttributeRepository,
            IEventPublisher eventPublisher,
			IRepository<Order> orderRepository)
        {
            _genericAttributeRepository = genericAttributeRepository;
            _eventPublisher = eventPublisher;
			_orderRepository = orderRepository;
        }

        public virtual void DeleteAttribute(GenericAttribute attribute)
        {
			Guard.NotNull(attribute, nameof(attribute));

			int entityId = attribute.EntityId;
			string keyGroup = attribute.KeyGroup;

            _genericAttributeRepository.Delete(attribute);

			if (keyGroup.IsCaseInsensitiveEqual("Order") && entityId != 0)
			{
				var order = _orderRepository.GetById(entityId);
				_eventPublisher.PublishOrderUpdated(order);
			}
        }

        public virtual GenericAttribute GetAttributeById(int attributeId)
        {
            if (attributeId == 0)
                return null;

            var attribute = _genericAttributeRepository.GetById(attributeId);
            return attribute;
        }

        public virtual void InsertAttribute(GenericAttribute attribute)
        {
			Guard.NotNull(attribute, nameof(attribute));

			_genericAttributeRepository.Insert(attribute);

			if (attribute.KeyGroup.IsCaseInsensitiveEqual("Order") && attribute.EntityId != 0)
			{
				var order = _orderRepository.GetById(attribute.EntityId);
				_eventPublisher.PublishOrderUpdated(order);
			}
        }

        public virtual void UpdateAttribute(GenericAttribute attribute)
        {
			Guard.NotNull(attribute, nameof(attribute));

			_genericAttributeRepository.Update(attribute);

			if (attribute.KeyGroup.IsCaseInsensitiveEqual("Order") && attribute.EntityId != 0)
			{
				var order = _orderRepository.GetById(attribute.EntityId);
				_eventPublisher.PublishOrderUpdated(order);
			}
        }

		public virtual IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup)
        {
			var query = from ga in _genericAttributeRepository.Table
						where ga.EntityId == entityId &&
						ga.KeyGroup == keyGroup
						select ga;

			var attributes = query.ToListCached("db.ga.{0}-{1}".FormatInvariant(entityId, keyGroup));
			return attributes;
		}

		public virtual Multimap<int, GenericAttribute> GetAttributesForEntity(int[] entityIds, string keyGroup)
		{
			Guard.NotNull(entityIds, nameof(entityIds));

			var query = _genericAttributeRepository.TableUntracked
				.Where(x => entityIds.Contains(x.EntityId) && x.KeyGroup == keyGroup);

			var map = query
				.ToList()
				.ToMultimap(x => x.EntityId, x => x);

			return map;
		}

		public virtual IQueryable<GenericAttribute> GetAttributes(string key, string keyGroup)
		{
			var query =
				from ga in _genericAttributeRepository.Table
				where ga.Key == key && ga.KeyGroup == keyGroup
				select ga;

			return query;
		}

		public virtual void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, int storeId = 0)
        {
			Guard.NotNull(entity, nameof(entity));

			SaveAttribute(entity.Id, key, entity.GetUnproxiedType().Name, value, storeId);
        }

		public virtual void SaveAttribute<TPropType>(int entityId, string key, string keyGroup, TPropType value, int storeId = 0)
		{
			Guard.NotZero(entityId, nameof(entityId));

			var valueStr = value.Convert<string>();
			var props = GetAttributesForEntity(entityId, keyGroup);

			// should be culture invariant
			var prop = props.FirstOrDefault(ga => ga.StoreId == storeId && ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

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
	}
}