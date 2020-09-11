using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Blogs
{
    public partial class AddBlogCommentModel : EntityModelBase
    {
        [SmartResourceDisplayName("Blog.Comments.CommentText")]
        [SanitizeHtml]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}