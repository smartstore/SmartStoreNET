using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Messages
{
    /// <summary>
    /// Represents a message template
    /// </summary>
	public partial class MessageTemplate : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        [StringLength(500), Required]
        public string To { get; set; }

        [StringLength(500)]
        public string ReplyTo { get; set; }

        /// <summary>
        /// A comma separated list of required model types (e.g.: Product, Order, Customer, GiftCard)
        /// </summary>
        [StringLength(500)]
        public string ModelTypes { get; set; }

        [MaxLength]
        public string LastModelTree { get; set; }

        /// <summary>
        /// Gets or sets the BCC Email addresses
        /// </summary>
        public string BccEmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
		[AllowHtml]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the used email account identifier
        /// </summary>
        public int EmailAccountId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether emails derived from the template are only send manually
        /// </summary>
        public bool SendManually { get; set; }

        /// <summary>
        /// Gets or sets the attachment 1 file identifier
        /// </summary>
        public int? Attachment1FileId { get; set; }

        /// <summary>
        /// Gets or sets the attachment 2 file identifier
        /// </summary>
        public int? Attachment2FileId { get; set; }

        /// <summary>
        /// Gets or sets the attachment 3 file identifier
        /// </summary>
        public int? Attachment3FileId { get; set; }
    }
}
