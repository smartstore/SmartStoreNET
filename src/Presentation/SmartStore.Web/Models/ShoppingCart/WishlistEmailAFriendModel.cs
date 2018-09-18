using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Web.Mvc;

namespace SmartStore.Web.Models.ShoppingCart
{
    [Validator(typeof(WishlistEmailAFriendValidator))]
    public partial class WishlistEmailAFriendModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Wishlist.EmailAFriend.FriendEmail")]
        public string FriendEmail { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Wishlist.EmailAFriend.YourEmailAddress")]
        public string YourEmailAddress { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Wishlist.EmailAFriend.PersonalMessage")]
        public string PersonalMessage { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }

    public class WishlistEmailAFriendValidator : AbstractValidator<WishlistEmailAFriendModel>
    {
        public WishlistEmailAFriendValidator()
        {
            RuleFor(x => x.FriendEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.YourEmailAddress).NotEmpty().EmailAddress();
        }
    }
}