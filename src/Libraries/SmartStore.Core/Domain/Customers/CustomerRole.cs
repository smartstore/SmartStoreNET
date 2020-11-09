using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Security;
using SmartStore.Rules.Domain;

namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer role
    /// </summary>
    [DataContract]
    public partial class CustomerRole : BaseEntity, IRulesContainer
    {
        private ICollection<PermissionRoleMapping> _permissionRoleMappings;
        private ICollection<RuleSetEntity> _ruleSets;

        /// <summary>
        /// Gets or sets the customer role name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is marked as free shiping
        /// </summary>
        [DataMember]
        public bool FreeShipping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is marked as tax exempt
        /// </summary>
        [DataMember]
        public bool TaxExempt { get; set; }

        /// <summary>
        /// Gets or sets the tax display type
        /// </summary>
        [DataMember]
        public int? TaxDisplayType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is active
        /// </summary>
        [DataMember]
        [Index]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is system
        /// </summary>
        [DataMember]
        [Index]
        [Index("IX_CustomerRole_SystemName_IsSystemRole", 2)]
        public bool IsSystemRole { get; set; }

        /// <summary>
        /// Gets or sets the customer role system name
        /// </summary>
        [DataMember]
        [Index]
        [Index("IX_CustomerRole_SystemName_IsSystemRole", 1)]
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets a minimum order amount
        /// </summary>
        [DataMember]
        public decimal? OrderTotalMinimum { get; set; }

        /// <summary>
        /// Gets or sets a maximum order amount
        /// </summary>
        [DataMember]
        public decimal? OrderTotalMaximum { get; set; }

        /// <summary>
        /// Gets or sets the permission role mappings.
        /// </summary>
        public virtual ICollection<PermissionRoleMapping> PermissionRoleMappings
        {
            get => _permissionRoleMappings ?? (_permissionRoleMappings = new HashSet<PermissionRoleMapping>());
            protected set => _permissionRoleMappings = value;
        }

        /// <summary>
        /// Gets or sets assigned rule sets.
        /// </summary>
        public virtual ICollection<RuleSetEntity> RuleSets
        {
            get => _ruleSets ?? (_ruleSets = new HashSet<RuleSetEntity>());
            protected set => _ruleSets = value;
        }
    }
}