﻿using System.Collections.Generic;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumPageModel
    {
        public ForumPageModel()
        {
            this.ForumTopics = new List<ForumTopicRowModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
        public string Description { get; set; }

        public string WatchForumText { get; set; }
        public bool WatchForumSubscribed { get; set; } // codehing: sm-add

        public IList<ForumTopicRowModel> ForumTopics { get; set; }
        public int TopicPageSize { get; set; }
        public int TopicTotalRecords { get; set; }
        public int TopicPageIndex { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }
        
        public bool ForumFeedsEnabled { get; set; }

        public int PostsPageSize { get; set; }
    }
}