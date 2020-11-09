using SmartStore.Core.Configuration;

namespace SmartStore.Web.Framework.Security
{
    public class CaptchaSettings : ISettings
    {
        public bool Enabled { get; set; }
        public bool ShowOnLoginPage { get; set; }
        public bool ShowOnRegistrationPage { get; set; }
        public bool ShowOnContactUsPage { get; set; }
        public bool ShowOnEmailWishlistToFriendPage { get; set; }
        public bool ShowOnEmailProductToFriendPage { get; set; }
        public bool ShowOnAskQuestionPage { get; set; }
        public bool ShowOnBlogCommentPage { get; set; }
        public bool ShowOnNewsCommentPage { get; set; }
        public bool ShowOnForumPage { get; set; }
        public bool ShowOnProductReviewPage { get; set; }
        public string ReCaptchaPublicKey { get; set; }
        public string ReCaptchaPrivateKey { get; set; }
        public bool UseInvisibleReCaptcha { get; set; }

        public bool CanDisplayCaptcha => Enabled && ReCaptchaPublicKey.HasValue() && ReCaptchaPrivateKey.HasValue();
    }
}