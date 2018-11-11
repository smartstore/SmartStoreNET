using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Blogs
{
    public partial class AddBlogCommentModel : EntityModelBase
    {
        [SmartResourceDisplayName("Blog.Comments.CommentText")]
        [AllowHtml]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}