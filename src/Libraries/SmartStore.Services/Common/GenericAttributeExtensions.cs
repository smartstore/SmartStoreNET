using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Data;

namespace SmartStore.Services.Common
{
    public static class GenericAttributeExtensions
    {
        /// <summary>
        /// Gets an entity generic attribute value
        /// </summary>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
		/// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TProp GetAttribute<TProp>(this BaseEntity entity, string key, int storeId = 0)
        {
            return GetAttribute<TProp>(
                entity,
                key,
                EngineContext.Current.Resolve<IGenericAttributeService>(),
                storeId);
        }

        /// <summary>
        /// Gets an entity generic attribute value
        /// </summary>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="genericAttributeService">GenericAttributeService</param>
        /// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute value</returns>
        public static TProp GetAttribute<TProp>(this BaseEntity entity, string key, IGenericAttributeService genericAttributeService, int storeId = 0)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return (genericAttributeService ?? EngineContext.Current.Resolve<IGenericAttributeService>()).GetAttribute<TProp>(
                entity.GetUnproxiedType().Name,
                entity.Id,
                key,
                storeId);
        }
    }
}
