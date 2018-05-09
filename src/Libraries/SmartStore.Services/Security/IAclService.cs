using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;

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
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Customer role identifiers</returns>
        int[] GetCustomerRoleIdsWithAccess<T>(T entity) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity, Customer customer) where T : BaseEntity, IAclSupported;
    }
}