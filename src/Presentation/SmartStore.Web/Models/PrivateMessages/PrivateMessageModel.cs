using System;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.PrivateMessages
{
    [Validator(typeof(SendPrivateMessageValidator))]
    public partial class PrivateMessageModel : EntityModelBase
    {
        public int FromCustomerId { get; set; }
        public string CustomerFromName { get; set; }
        public bool AllowViewingFromProfile { get; set; }

        public int ToCustomerId { get; set; }
        public string CustomerToName { get; set; }
        public bool AllowViewingToProfile { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsRead { get; set; }
    }
}