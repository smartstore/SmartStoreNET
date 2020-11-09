using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    /// <summary>
    /// ACL service inerface
    /// </summary>
    public partial interface IAclService
    {
        /// <summary>
        /// Gets a value indicating whether at least one ACL record is in active state system-wide
        /// </summary>
        bool HasActiveAcl { get; }

        /// <summary>
        /// Deletes an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        void DeleteAclRecord(AclRecord aclRecord);

        /// <summary>
        /// Gets an ACL record
        /// </summary>
        /// <param name="aclRecordId">ACL record identifier</param>
        /// <returns>ACL record</returns>
        AclRecord GetAclRecordById(int aclRecordId);

        /// <summary>
        /// Gets ACL records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>ACL records</returns>
        IList<AclRecord> GetAclRecords<T>(T entity) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Gets ACL records
        /// </summary>
        /// <param name="entityName">Name of entity</param>
        /// <param name="entityId">Id of entity</param>
        /// <returns>ACL records</returns>
        IList<AclRecord> GetAclRecordsFor(string entityName, int entityId);

        /// <summary>
        /// Save the ACL mappings for an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">The entity</param>
        /// <param name="selectedCustomerRoleIds">Array of selected customer role ids with access to the passed entity</param>
        void SaveAclMappings<T>(T entity, params int[] selectedCustomerRoleIds) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        void InsertAclRecord(AclRecord aclRecord);

        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="customerRoleId">Customer role id</param>
        void InsertAclRecord<T>(T entity, int customerRoleId) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Updates the ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        void UpdateAclRecord(AclRecord aclRecord);

        /// <summary>
        /// Find customer role identifiers with granted access
        /// </summary>
        /// <param name="entityName">Entity name to check permission for</param>
        /// <param name="entityId">Entity id to check permission for</param>
        /// <returns>Customer role identifiers</returns>
        int[] GetCustomerRoleIdsWithAccessTo(string entityName, int entityId);

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entityName">Entity name to check permission for</param>
        /// <param name="entityId">Entity id to check permission for</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(string entityName, int entityId);

        ///// <summary>
        ///// Authorize ACL permission
        ///// </summary>
        ///// <typeparam name="T">Type</typeparam>
        ///// <param name="entityName">Entity name to check permission for</param>
        ///// <param name="entityId">Entity id to check permission for</param>
        ///// <param name="customer">Customer</param>
        ///// <returns>true - authorized; otherwise, false</returns>
        //bool Authorize(string entityName, int entityId, Customer customer);

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <param name="entityName">Entity name to check permission for</param>
        /// <param name="entityId">Entity id to check permission for</param>
        /// <param name="roles">Roles to check access permission for. Inactive roles will be skipped.</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(string entityName, int entityId, IEnumerable<CustomerRole> roles);
    }

    public static class IAclServiceExtensions
    {
        /// <summary>
        /// Find customer role identifiers with granted access
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Customer role identifiers</returns>
        public static int[] GetCustomerRoleIdsWithAccessTo<T>(this IAclService aclService, T entity) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
                return new int[0];

            return aclService.GetCustomerRoleIdsWithAccessTo(entity.GetEntityName(), entity.Id);
        }

        public static bool Authorize(this IAclService aclService, string entityName, int entityId, Customer customer)
        {
            return aclService.Authorize(entityName, entityId, customer?.CustomerRoleMappings?.Select(x => x.CustomerRole));
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public static bool Authorize<T>(this IAclService aclService, T entity) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
                return false;

            if (!entity.SubjectToAcl)
                return true;

            return aclService.Authorize(entity.GetEntityName(), entity.Id);
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public static bool Authorize<T>(this IAclService aclService, T entity, Customer customer) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
                return false;

            if (!entity.SubjectToAcl)
                return true;

            return aclService.Authorize(entity.GetEntityName(), entity.Id, customer?.CustomerRoleMappings?.Select(x => x.CustomerRole));
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="roles">Roles to check access permission for. Inactive roles will be skipped.</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public static bool Authorize<T>(this IAclService aclService, T entity, IEnumerable<CustomerRole> roles) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
                return false;

            if (!entity.SubjectToAcl)
                return true;

            return aclService.Authorize(entity.GetEntityName(), entity.Id, roles);
        }
    }
}