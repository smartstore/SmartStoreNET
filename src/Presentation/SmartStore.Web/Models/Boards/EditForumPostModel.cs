using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Forums;
using SmartStore.Services.Localization;
using System.Web.Mvc;

namespace SmartStore.Web.Models.Boards
{
    [Validator(typeof(EditForumPostValidator))]
    public partial class EditForumPostModel
    {
        public int Id { get; set; }
        public int ForumTopicId { get; set; }

        public bool IsEdit { get; set; }

        [AllowHtml]
        public string Text { get; set; }
        public EditorType ForumEditor { get; set; }

        public LocalizedValue<string> ForumName { get; set; }
        public string ForumTopicSubject { get; set; }
        public string ForumTopicSeName { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }
        public bool Subscribed { get; set; }
    }

    public class EditForumPostValidator : AbstractValidator<EditForumPostModel>
    {
        public EditForumPostValidator()
        {
            RuleFor(x => x.Text).NotEmpty();
        }
    }
}