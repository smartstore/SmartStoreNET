using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStore.Core.Domain.Messages
{
    /// <summary>
    /// Represents NewsLetterSubscription entity
    /// </summary>
    public partial class NewsLetterSubscription : BaseEntity
    {       
        /// <summary>
        /// Gets or sets the newsletter subscription GUID
        /// </summary>
        public Guid NewsLetterSubscriptionGuid { get; set; }

        /// <summary>
        /// Gets or sets the subcriber email
        /// </summary>
		[Index("IX_NewsletterSubscription_Email_StoreId", 1)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subscription is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the date and time when subscription was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the store identifier
		/// </summary>
		[Index("IX_NewsletterSubscription_Email_StoreId", 2)]
		public int StoreId { get; set; }
    }
}
