using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.PrivateMessages
{
    [Validator(typeof(SendPrivateMessageValidator))]
    public partial class SendPrivateMessageModel : EntityModelBase
    {
        public int ToCustomerId { get; set; }
        public string CustomerToName { get; set; }
        public bool AllowViewingToProfile { get; set; }

        public int ReplyToMessageId { get; set; }

        public string Subject { get; set; }

        [SanitizeHtml]
        public string Message { get; set; }
    }

    public class SendPrivateMessageValidator : AbstractValidator<SendPrivateMessageModel>
    {
        public SendPrivateMessageValidator()
        {
            RuleFor(x => x.Subject).NotEmpty();
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}