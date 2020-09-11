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
        private readonly IDictionary<string, Dictionary<string, object>> _valueCache;

        public GenericAttributeService(
            IRepository<GenericAttribute> genericAttributeRepository,
            IEventPublisher eventPublisher,
            IRepository<Order> orderRepository)
        {
            _genericAttributeRepository = genericAttributeRepository;
            _eventPublisher = eventPublisher;
            _orderRepository = orderRepository;
            _valueCache = new Dictionary<string, Dictionary<string, object>>();
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

        public virtual IList<GenericAttribute> GetAttributesForEntity(int entityId, string entityName)
        {
            var query = from ga in _genericAttributeRepository.Table
                        where ga.EntityId == entityId && ga.KeyGroup == entityName
                        select ga;

            return query.ToListCached("db.ga.{0}-{1}".FormatInvariant(entityId, entityName));
        }

        public virtual Multimap<int, GenericAttribute> GetAttributesForEntity(int[] entityIds, string entityName)
        {
            Guard.NotNull(entityIds, nameof(entityIds));

            var query = _genericAttributeRepository.TableUntracked
                .Where(x => entityIds.Contains(x.EntityId) && x.KeyGroup == entityName);

            var map = query
                .ToList()
                .ToMultimap(x => x.EntityId, x => x);

            return map;
        }

        public virtual TProp GetAttribute<TProp>(string entityName, int entityId, string key, int storeId = 0)
        {
            if (entityName == null)
                throw new ArgumentNullException(nameof(entityName));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var entityKey = entityName.ToLowerInvariant() + entityId;
            if (!_valueCache.TryGetValue(entityKey, out var attrs))
            {
                var list = GetAttributesForEntity(entityId, entityName);
                if (list.Count > 0)
                {
                    attrs = list.ToDictionarySafe(k => k.Key.ToLowerInvariant() + k.StoreId, v => (object)v.Value);
                }
                _valueCache[entityKey] = attrs;
            }

            TProp result = default;

            if (attrs != null)
            {
                var entryKey = key.ToLowerInvariant() + storeId;
                if (attrs.TryGetValue(entryKey, out var rawValue))
                {
                    if (rawValue.GetType() == typeof(TProp))
                    {
                        result = (TProp)rawValue;
                    }
                    else
                    {
                        result = rawValue.Convert<TProp>();
                        // to skip repeated value type conversion
                        attrs[entryKey] = result;
                    }
                }
            }

            return result;
        }

        public virtual IQueryable<GenericAttribute> GetAttributes(string key, string entityName)
        {
            var query =
                from ga in _genericAttributeRepository.Table
                where ga.Key == key && ga.KeyGroup == entityName
                select ga;

            return query;
        }

        public virtual void SaveAttribute<TProp>(BaseEntity entity, string key, TProp value, int storeId = 0)
        {
            Guard.NotNull(entity, nameof(entity));

            SaveAttribute(entity.Id, key, entity.GetUnproxiedType().Name, value, storeId);
        }

        public virtual void SaveAttribute<TProp>(int entityId, string key, string entityName, TProp value, int storeId = 0)
        {
            Guard.NotZero(entityId, nameof(entityId));

            var valueStr = value.Convert<string>();
            var props = GetAttributesForEntity(entityId, entityName);

            // should be culture invariant
            var prop = props.FirstOrDefault(ga => ga.StoreId == storeId && ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    // delete
                    DeleteAttribute(prop);
                    CacheTryRemove(entityName, entityId, prop);
                }
                else
                {
                    // update
                    prop.Value = valueStr;
                    UpdateAttribute(prop);
                    CacheTryAddOrUpdate(entityName, entityId, prop);
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
                        KeyGroup = entityName,
                        Value = valueStr,
                        StoreId = storeId
                    };

                    InsertAttribute(prop);

                    CacheTryAddOrUpdate(entityName, entityId, prop);
                }
            }
        }

        private void CacheTryRemove(string entityName, int entityId, GenericAttribute attr)
        {
            var entityKey = entityName.ToLowerInvariant() + entityId;
            var entryKey = attr.Key.ToLowerInvariant() + attr.StoreId;
            _valueCache.Get(entityKey).TryRemove(entryKey, out _);
        }

        private void CacheTryAddOrUpdate(string entityName, int entityId, GenericAttribute attr)
        {
            var entityKey = entityName.ToLowerInvariant() + entityId;
            var entryKey = attr.Key.ToLowerInvariant() + attr.StoreId;
            _valueCache.Get(entityKey).TryAdd(entryKey, attr.Value, true);
        }
    }
}