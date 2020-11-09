using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum post
    /// </summary>
    public partial class ForumPost : BaseEntity, IAuditable
    {
        private ICollection<ForumPostVote> _forumPostVotes;

        /// <summary>
        /// Gets or sets the forum topic identifier
        /// </summary>
        public int TopicId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        [Index]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        [Index]
        public bool Published { get; set; }

        /// <summary>
        /// Gets the topic
        /// </summary>
        public virtual ForumTopic ForumTopic { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Forum post votes.
        /// </summary>
        public virtual ICollection<ForumPostVote> ForumPostVotes
        {
            get => _forumPostVotes ?? (_forumPostVotes = new HashSet<ForumPostVote>());
            protected set => _forumPostVotes = value;
        }
    }
}
