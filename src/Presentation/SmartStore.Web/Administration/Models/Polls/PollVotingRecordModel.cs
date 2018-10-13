using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Polls
{
    public class PollVotingRecordModel : EntityModelBase
    {
        public int CustomerId { get; set; }
        public bool IsGuest { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Common.Answer")]
        public string AnswerName { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
        public string FullName { get; set; }

    }
}