using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Forums
{
    /// <summary>
    /// Represents a vote for a forum post.
    /// </summary>
    public partial class ForumPostVote : CustomerContent
    {
        /// <summary>
        /// Forum post identifier.
        /// </summary>
        public int ForumPostId { get; set; }

        /// <summary>
        /// A value indicating whether the customer voted for or against a forum post.
        /// </summary>
        public bool Vote { get; set; }

        /// <summary>
        /// Forum post entity.
        /// </summary>
        public virtual ForumPost ForumPost { get; set; }
    }
}
