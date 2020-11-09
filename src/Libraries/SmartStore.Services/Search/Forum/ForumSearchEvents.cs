namespace SmartStore.Services.Search
{
    public class ForumSearchingEvent
    {
        public ForumSearchingEvent(ForumSearchQuery query)
        {
            Guard.NotNull(query, nameof(query));

            Query = query;
        }

        public ForumSearchQuery Query { get; private set; }
    }

    public class ForumSearchedEvent
    {
        public ForumSearchedEvent(ForumSearchQuery query, ForumSearchResult result)
        {
            Guard.NotNull(query, nameof(query));
            Guard.NotNull(result, nameof(result));

            Query = query;
            Result = result;
        }

        public ForumSearchQuery Query { get; private set; }
        public ForumSearchResult Result { get; private set; }
    }
}
