using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Forums;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Boards
{
    [Validator(typeof(EditForumPostValidator))]
    public partial class EditForumPostModel : EntityModelBase
    {
        public int ForumTopicId { get; set; }
        public bool IsEdit { get; set; }
        public bool Published { get; set; }
        public bool DisplayCaptcha { get; set; }
        public bool IsFirstPost { get; set; }

        [SanitizeHtml]
        public string Text { get; set; }
        public EditorType ForumEditor { get; set; }

        public LocalizedValue<string> ForumName { get; set; }
        public string ForumTopicSubject { get; set; }
        public string ForumTopicSeName { get; set; }

        public bool IsModerator { get; set; }
        public bool IsCustomerAllowedToSubscribe { get; set; }
        public bool Subscribed { get; set; }

        public bool IsCustomerAllowedToEdit { get; set; }
        public int CustomerId { get; set; }
    }

    public class EditForumPostValidator : AbstractValidator<EditForumPostModel>
    {
        public EditForumPostValidator()
        {
            RuleFor(x => x.Text).NotEmpty();
        }
    }
}