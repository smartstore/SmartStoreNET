using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Forums
{
    /// <summary>
    /// Represents a forum topic
    /// </summary>
    public partial class ForumTopic : BaseEntity, IAuditable
    {
        /// <summary>
        /// Gets or sets the forum identifier
        /// </summary>
        [Index("IX_ForumId_Published", Order = 0)]
        public int ForumId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the topic type identifier
        /// </summary>
        [Index("IX_TopicTypeId_LastPostTime", Order = 0)]
        public int TopicTypeId { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        [Index]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the number of posts
        /// </summary>
        [Index]
        public int NumPosts { get; set; }

        /// <summary>
        /// Gets or sets the number of views
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// Gets or sets the first post identifier, for example of the first search hit.
        /// This property is not a data member.
        /// </summary>
        public int FirstPostId { get; set; }

        /// <summary>
        /// Gets or sets the last post identifier
        /// </summary>
        public int LastPostId { get; set; }

        /// <summary>
        /// Gets or sets the last post customer identifier
        /// </summary>
        public int LastPostCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the last post date and time
        /// </summary>
        [Index("IX_TopicTypeId_LastPostTime", Order = 1)]
        public DateTime? LastPostTime { get; set; }

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
        [Index("IX_ForumId_Published", Order = 1)]
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the forum topic type
        /// </summary>
        public ForumTopicType ForumTopicType
        {
            get => (ForumTopicType)this.TopicTypeId;
            set => this.TopicTypeId = (int)value;
        }

        /// <summary>
        /// Gets the forum
        /// </summary>
        public virtual Forum Forum { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets the number of replies
        /// </summary>
        public int NumReplies
        {
            get
            {
                if (NumPosts > 0)
                {
                    return NumPosts - 1;
                }

                return 0;
            }
        }
    }
}
