using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Rules.Domain
{
    public partial class RuleSetEntity : BaseEntity, IAuditable
    {
        private ICollection<RuleEntity> _rules;
        private ICollection<Discount> _discounts;
        private ICollection<ShippingMethod> _shippingMethods;
        private ICollection<PaymentMethod> _paymentMethods;
        private ICollection<CustomerRole> _customerRoles;
        private ICollection<Category> _categories;

        [DataMember]
        [StringLength(200)]
        public string Name { get; set; }

        [DataMember]
        [StringLength(400)]
        public string Description { get; set; }

        [Index("IX_RuleSetEntity_Scope", Order = 0)]
        public bool IsActive { get; set; } = true;

        [Required]
        [Index("IX_RuleSetEntity_Scope", Order = 1)]
        public RuleScope Scope { get; set; }


        /// <summary>
        /// True when this set is an internal composite container for rules within another ruleset.
        /// </summary>
        [Index]
        public bool IsSubGroup { get; set; }

        public LogicalRuleOperator LogicalOperator { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public DateTime? LastProcessedOnUtc { get; set; }

        public virtual ICollection<RuleEntity> Rules
        {
            get => _rules ?? (_rules = new HashSet<RuleEntity>());
            protected internal set => _rules = value;
        }

        /// <summary>
        /// Gets or sets assigned discounts.
        /// </summary>
        public virtual ICollection<Discount> Discounts
        {
            get => _discounts ?? (_discounts = new HashSet<Discount>());
            protected set => _discounts = value;
        }

        /// <summary>
        /// Gets or sets assigned shipping methods.
        /// </summary>
        public virtual ICollection<ShippingMethod> ShippingMethods
        {
            get => _shippingMethods ?? (_shippingMethods = new HashSet<ShippingMethod>());
            protected set => _shippingMethods = value;
        }

        /// <summary>
        /// Gets or sets assigned payment methods.
        /// </summary>
        public virtual ICollection<PaymentMethod> PaymentMethods
        {
            get => _paymentMethods ?? (_paymentMethods = new HashSet<PaymentMethod>());
            protected set => _paymentMethods = value;
        }

        /// <summary>
        /// Gets or sets assigned customer roles.
        /// </summary>
        public virtual ICollection<CustomerRole> CustomerRoles
        {
            get => _customerRoles ?? (_customerRoles = new HashSet<CustomerRole>());
            protected set => _customerRoles = value;
        }

        /// <summary>
        /// Gets or sets assigned categories.
        /// </summary>
        public virtual ICollection<Category> Categories
        {
            get => _categories ?? (_categories = new HashSet<Category>());
            protected set => _categories = value;
        }
    }
}
