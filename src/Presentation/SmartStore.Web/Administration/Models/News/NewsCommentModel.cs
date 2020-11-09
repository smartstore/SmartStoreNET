using System;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.News
{
    public class NewsCommentModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.NewsItem")]
        public int NewsItemId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.NewsItem")]
        [AllowHtml]
        public string NewsItemTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.Customer")]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.IPAddress")]
        public string IpAddress { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CommentTitle")]
        public string CommentTitle { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CommentText")]
        public string CommentText { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

    }
}