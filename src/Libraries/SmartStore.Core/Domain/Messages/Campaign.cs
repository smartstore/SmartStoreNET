using System;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Messages
{
    /// <summary>
    /// Represents a campaign
    /// </summary>
	public partial class Campaign : BaseEntity, IStoreMappingSupported
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
		/// </summary>
		public bool LimitedToStores { get; set; }
    }
}
