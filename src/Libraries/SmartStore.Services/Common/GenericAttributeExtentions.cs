using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Data;

namespace SmartStore.Services.Common
{
    public static class GenericAttributeExtentions
    {
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
		/// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
		public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, int storeId = 0)
        {
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            return GetAttribute<TPropType>(entity, key, genericAttributeService, storeId);
        }
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="genericAttributeService">GenericAttributeService</param>
		/// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, IGenericAttributeService genericAttributeService, int storeId = 0)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (genericAttributeService == null)
                genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

            string keyGroup = entity.GetUnproxiedType().Name;

            return genericAttributeService.GetAttribute<TPropType>(keyGroup, entity.Id, key, storeId);

            #region Old
            //var props = genericAttributeService.GetAttributesForEntity(entity.Id, keyGroup);
            ////little hack here (only for unit testing). we should write expect-return rules in unit tests for such cases
            //if (props == null)
            //    return default(TPropType);
            //props = props.Where(x => x.StoreId == storeId).ToList();
            //if (props.Count == 0)
            //    return default(TPropType);

            //var prop = props.FirstOrDefault(ga =>
            //    ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            //if (prop == null || string.IsNullOrEmpty(prop.Value))
            //    return default(TPropType);

            //return CommonHelper.To<TPropType>(prop.Value);
            #endregion
        }

        public static TPropType GetAttribute<TPropType>(this IGenericAttributeService genericAttributeService, string entityName, int entityId, string key, int storeId = 0)
        {
            Guard.NotNull(genericAttributeService, nameof(genericAttributeService));
            Guard.NotEmpty(entityName, nameof(entityName));

            var props = genericAttributeService.GetAttributesForEntity(entityId, entityName);

            // little hack here (only for unit testing). we should write expect-return rules in unit tests for such cases
            if (props == null)
            {
                return default(TPropType);
            }

            if (!props.Any(x => x.StoreId == storeId))
            {
                return default(TPropType);
            }

            var prop = props.FirstOrDefault(ga =>
                ga.StoreId == storeId && ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop == null || prop.Value.IsEmpty())
            {
                return default(TPropType);
            }

			return prop.Value.Convert<TPropType>();
        }
    }
}
