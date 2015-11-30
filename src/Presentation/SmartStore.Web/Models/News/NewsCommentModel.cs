using System;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.News
{
    public partial class NewsCommentModel : EntityModelBase
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAvatarUrl { get; set; }

        public string CommentTitle { get; set; }

        public string CommentText { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool AllowViewingProfiles { get; set; }
    }
}