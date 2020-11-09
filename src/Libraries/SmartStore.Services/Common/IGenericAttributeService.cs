using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Generic attribute service interface
    /// </summary>
    public partial interface IGenericAttributeService
    {
        /// <summary>
        /// Deletes an attribute
        /// </summary>
        /// <param name="attribute">Attribute</param>
        void DeleteAttribute(GenericAttribute attribute);

        /// <summary>
        /// Gets an attribute
        /// </summary>
        /// <param name="attributeId">Attribute identifier</param>
        /// <returns>An attribute</returns>
        GenericAttribute GetAttributeById(int attributeId);

        /// <summary>
        /// Inserts an attribute
        /// </summary>
        /// <param name="attribute">attribute</param>
        void InsertAttribute(GenericAttribute attribute);

        /// <summary>
        /// Updates the attribute
        /// </summary>
        /// <param name="attribute">Attribute</param>
        void UpdateAttribute(GenericAttribute attribute);

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Key group</param>
        /// <returns>Generic attributes</returns>
		IList<GenericAttribute> GetAttributesForEntity(int entityId, string entityName);

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="entityIds">Entity identifiers</param>
        /// <param name="entityName">Key group</param>
        /// <returns>Generic attributes</returns>
        Multimap<int, GenericAttribute> GetAttributesForEntity(int[] entityIds, string entityName);

        /// <summary>
        /// Get attribute value
        /// </summary>
        /// <param name="entityName">Key group</param>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="key">Key</param>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        /// <returns>Converted generic attribute value</returns>
        TProp GetAttribute<TProp>(string entityName, int entityId, string key, int storeId = 0);

        /// <summary>
        /// Get queryable attributes
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="entityName">The key group</param>
        /// <returns>Queryable attributes</returns>
        IQueryable<GenericAttribute> GetAttributes(string key, string entityName);

        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        void SaveAttribute<TProp>(BaseEntity entity, string key, TProp value, int storeId = 0);

        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="key">The key</param>
        /// <param name="keyGroup">The key group</param>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="storeId">Store identifier; pass 0 if this attribute will be available for all stores</param>
        void SaveAttribute<TProp>(int entityId, string key, string keyGroup, TProp value, int storeId = 0);
    }
}