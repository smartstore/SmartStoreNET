namespace SmartStore.Core.Domain.Forums
{
    /// <summary>
    /// Represents the sorting of forum topics.
    /// </summary>
    public enum ForumTopicSorting
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Initial = 0,

        /// <summary>
        /// Relevance
        /// </summary>
        Relevance,

        /// <summary>
        /// Subject: A to Z
        /// </summary>
        SubjectAsc,

        /// <summary>
        /// Subject: Z to A
        /// </summary>
        SubjectDesc,

        /// <summary>
        /// User name: A to Z
        /// </summary>
        UserNameAsc,

        /// <summary>
        /// User name: Z to A
        /// </summary>
        UserNameDesc,

        /// <summary>
        /// Creation date: Oldest first
        /// </summary>
        CreatedOnAsc,

        /// <summary>
        /// Creation date: Newest first
        /// </summary>
        CreatedOnDesc,

        /// <summary>
        /// Number of posts: Low to High
        /// </summary>
        PostsAsc,

        /// <summary>
        /// Number of posts: High to Low
        /// </summary>
        PostsDesc
    }
}
