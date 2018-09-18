using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumTopicPageModel : EntityModelBase
    {
        public ForumTopicPageModel()
        {
            ForumPostModels = new List<ForumPostModel>();
        }

        public string Subject { get; set; }
        public string SeName { get; set; }

        public string WatchTopicText { get; set; }

        public bool IsCustomerAllowedToEditTopic { get; set; }
        public bool IsCustomerAllowedToDeleteTopic { get; set; }
        public bool IsCustomerAllowedToMoveTopic { get; set; }
        public bool IsCustomerAllowedToSubscribe { get; set; }

        public IList<ForumPostModel> ForumPostModels { get; set; }
        public int PostsPageIndex { get; set; }
        public int PostsPageSize { get; set; }
        public int PostsTotalRecords { get; set; }
    }
}