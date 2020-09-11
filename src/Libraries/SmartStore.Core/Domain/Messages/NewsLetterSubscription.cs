using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Messages
{
    /// <summary>
    /// Represents NewsLetterSubscription entity
    /// </summary>
    [DataContract]
    public partial class NewsLetterSubscription : BaseEntity
    {
        /// <summary>
        /// Gets or sets the newsletter subscription GUID
        /// </summary>
        [DataMember]
        public Guid NewsLetterSubscriptionGuid { get; set; }

        /// <summary>
        /// Gets or sets the subcriber email
        /// </summary>
        [DataMember]
        [Index("IX_NewsletterSubscription_Email_StoreId", 1)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subscription is active
        /// </summary>
        [DataMember]
        [Index]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the date and time when subscription was created
        /// </summary>
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        [DataMember]
        [Index("IX_NewsletterSubscription_Email_StoreId", 2)]
        public int StoreId { get; set; }

        /// <summary>
		/// Gets or sets the language identifier
		/// </summary>
        [DataMember]
        public int WorkingLanguageId { get; set; }
    }
}
