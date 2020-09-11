using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Catalog
{
    [Validator(typeof(ProductEmailAFriendValidator))]
    public partial class ProductEmailAFriendModel : ModelBase
    {
        public int ProductId { get; set; }

        public LocalizedValue<string> ProductName { get; set; }

        public string ProductSeName { get; set; }

        [SmartResourceDisplayName("Products.EmailAFriend.FriendEmail")]
        public string FriendEmail { get; set; }

        [SmartResourceDisplayName("Products.EmailAFriend.YourEmailAddress")]
        public string YourEmailAddress { get; set; }

        [SanitizeHtml]
        [SmartResourceDisplayName("Products.EmailAFriend.PersonalMessage")]
        public string PersonalMessage { get; set; }

        public bool AllowChangedCustomerEmail { get; set; }

        public bool DisplayCaptcha { get; set; }
    }

    public class ProductEmailAFriendValidator : AbstractValidator<ProductEmailAFriendModel>
    {
        public ProductEmailAFriendValidator()
        {
            RuleFor(x => x.FriendEmail).NotEmpty();
            RuleFor(x => x.FriendEmail).EmailAddress();

            RuleFor(x => x.YourEmailAddress).NotEmpty();
            RuleFor(x => x.YourEmailAddress).EmailAddress();
        }
    }
}