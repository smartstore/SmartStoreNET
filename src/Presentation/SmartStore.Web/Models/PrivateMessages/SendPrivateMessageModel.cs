using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Validators.PrivateMessages;

namespace SmartStore.Web.Models.PrivateMessages
{
    [Validator(typeof(SendPrivateMessageValidator))]
    public partial class SendPrivateMessageModel : EntityModelBase
    {
        public int ToCustomerId { get; set; }
        public string CustomerToName { get; set; }
        public bool AllowViewingToProfile { get; set; }

        public int ReplyToMessageId { get; set; }

        [AllowHtml]
        public string Subject { get; set; }

        [AllowHtml]
        public string Message { get; set; }
    }
}