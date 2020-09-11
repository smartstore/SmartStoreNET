using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Forums;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Boards
{
    [Validator(typeof(EditForumTopicValidator))]
    public partial class EditForumTopicModel : EntityModelBase
    {
        public EditForumTopicModel()
        {
            TopicPriorities = new List<SelectListItem>();
        }

        public bool IsEdit { get; set; }
        public bool DisplayCaptcha { get; set; }
        public bool Published { get; set; }
        public string SeName { get; set; }

        public int ForumId { get; set; }
        public LocalizedValue<string> ForumName { get; set; }
        public string ForumSeName { get; set; }

        public int TopicTypeId { get; set; }
        public EditorType ForumEditor { get; set; }

        public string Subject { get; set; }

        [SanitizeHtml]
        public string Text { get; set; }

        public bool IsModerator { get; set; }
        public IEnumerable<SelectListItem> TopicPriorities { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }
        public bool Subscribed { get; set; }

        public bool IsCustomerAllowedToEdit { get; set; }
        public int CustomerId { get; set; }
    }

    public class EditForumTopicValidator : AbstractValidator<EditForumTopicModel>
    {
        public EditForumTopicValidator()
        {
            RuleFor(x => x.Subject).NotEmpty();
            RuleFor(x => x.Text).NotEmpty();
        }
    }
}