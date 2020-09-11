using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum group
    /// </summary>
	public partial class ForumGroup : BaseEntity, IAuditable, IStoreMappingSupported, IAclSupported, ILocalizedEntity, ISlugSupported
    {
        private ICollection<Forum> _forums;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [Index]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        [Index]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        [Index]
        public bool SubjectToAcl { get; set; }

        /// <summary>
        /// Gets or sets the collection of Forums
        /// </summary>
        public virtual ICollection<Forum> Forums
        {
            get => _forums ?? (_forums = new HashSet<Forum>());
            protected set => _forums = value;
        }
    }
}
