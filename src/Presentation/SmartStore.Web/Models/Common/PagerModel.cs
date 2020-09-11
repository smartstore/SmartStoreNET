namespace SmartStore.Web.Models.Common
{
    #region Classes

    /// <summary>
    /// Interface for custom RouteValues objects
    /// </summary>
    public interface IRouteValues
    {
        int page { get; set; }
    }

    /// <summary>
    /// Class that has a slug and page for route values. Used for Topic (posts) and 
    /// Forum (topics) pagination
    /// </summary>
    public partial class RouteValues : IRouteValues
    {
        public int id { get; set; }
        public string slug { get; set; }
        public int page { get; set; }
    }

    /// <summary>
    /// Class that has search options for route values. Used for Search result pagination
    /// </summary>
    public partial class ForumSearchRouteValues : IRouteValues
    {
        public string searchterms { get; set; }
        public string adv { get; set; }
        public string forumId { get; set; }
        public string within { get; set; }
        public string limitDays { get; set; }
        public int page { get; set; }
    }

    /// <summary>
    /// Class that has a slug and page for route values. Used for Private Messages pagination
    /// </summary>
    public partial class PrivateMessageRouteValues : IRouteValues
    {
        public string tab { get; set; }
        public int page { get; set; }
    }

    /// <summary>
    /// Class that has only page for route value. Used for (My Account) Forum Subscriptions pagination
    /// </summary>
    public partial class ForumSubscriptionsRouteValues : IRouteValues
    {
        public int page { get; set; }
    }

    /// <summary>
    /// Class that has only page for route value. Used for (My Account) Back in stock subscriptions pagination
    /// </summary>
    public partial class BackInStockSubscriptionsRouteValues : IRouteValues
    {
        public int page { get; set; }
    }

    #endregion Classes
}