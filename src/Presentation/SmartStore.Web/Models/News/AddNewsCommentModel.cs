using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.News
{
    public partial class AddNewsCommentModel : ModelBase
    {
        [SmartResourceDisplayName("News.Comments.CommentTitle")]
        public string CommentTitle { get; set; }

        [SmartResourceDisplayName("News.Comments.CommentText")]
        [SanitizeHtml]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}