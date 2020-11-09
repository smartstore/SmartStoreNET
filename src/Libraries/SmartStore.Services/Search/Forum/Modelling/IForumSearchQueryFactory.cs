namespace SmartStore.Services.Search.Modelling
{
    public interface IForumSearchQueryFactory
    {
        /// <summary>
        /// Creates a <see cref="ForumSearchQuery"/> instance from the current <see cref="HttpContextBase"/> 
        /// by looking up corresponding keys in posted form and/or query string
        /// </summary>
        /// <returns>The query object</returns>
        ForumSearchQuery CreateFromQuery();

        /// <summary>
        /// The last created query instance. The MVC model binder uses this property to avoid repeated binding.
        /// </summary>
        ForumSearchQuery Current { get; }
    }
}
