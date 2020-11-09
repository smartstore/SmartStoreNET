using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Rules.Domain;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a category
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Id}: {Name} (Parent: {ParentCategoryId})")]
    public partial class Category : BaseEntity, ICategoryNode, IAuditable, ISoftDeletable, IPagingOptions, IRulesContainer
    {
        private ICollection<RuleSetEntity> _ruleSets;
        private ICollection<Discount> _appliedDiscounts;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full name (category page title)
        /// </summary>
        [DataMember]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a description displayed at the bottom of the category page
        /// </summary>
        [DataMember]
        public string BottomDescription { get; set; }

        /// <summary>
        /// Gets or sets the external link expression. If set, any category menu item will navigate to the specified link.
        /// </summary>
        [DataMember]
        public string ExternalLink { get; set; }

        /// <summary>
		/// Gets or sets a text displayed in a badge next to the category within menus
		/// </summary>
        [DataMember]
        public string BadgeText { get; set; }

        /// <summary>
		/// Gets or sets the type of the badge within menus
		/// </summary>
        [DataMember]
        public int BadgeStyle { get; set; }

        /// <summary>
        /// Gets or sets the category alias 
        /// (an optional key for advanced customization)
        /// </summary>
        [DataMember]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets a value of used category template identifier
        /// </summary>
        [DataMember]
        public int CategoryTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        [DataMember]
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        [DataMember]
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        [DataMember]
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the parent category identifier
        /// </summary>
        [DataMember]
        public int ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the media file identifier
        /// </summary>
        [DataMember]
        public int? MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the media file
        /// </summary>
        [DataMember]
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
		[DataMember]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size
        /// </summary>
		[DataMember]
        public bool? AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
		[DataMember]
        public string PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available price ranges
        /// </summary>
        [Obsolete("Price ranges are calculated automatically since version 3")]
        [StringLength(400)]
        public string PriceRanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the category on home page
        /// </summary>
        [DataMember]
        public bool ShowOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this category has discounts applied
        /// <remarks>The same as if we run category.AppliedDiscounts.Count > 0
        /// We use this property for performance optimization:
        /// if this property is set to false, then we do not need to load Applied Discounts navigation property
        /// </remarks>
        /// </summary>
		[DataMember]
        public bool HasDiscountsApplied { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
		[DataMember]
        public bool SubjectToAcl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        [DataMember]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        [DataMember]
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
		[Index]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        [DataMember]
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        [DataMember]
        public string DefaultViewMode { get; set; }

        /// <summary>
        /// Gets or sets assigned rule sets.
        /// </summary>
        public virtual ICollection<RuleSetEntity> RuleSets
        {
            get => _ruleSets ?? (_ruleSets = new HashSet<RuleSetEntity>());
            protected set => _ruleSets = value;
        }

        /// <summary>
        /// Gets or sets the collection of applied discounts
        /// </summary>
		[DataMember]
        public virtual ICollection<Discount> AppliedDiscounts
        {
            get => _appliedDiscounts ?? (_appliedDiscounts = new HashSet<Discount>());
            protected set => _appliedDiscounts = value;
        }
    }
}
