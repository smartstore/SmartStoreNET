using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(EmailAccountValidator))]
    public class EmailAccountModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.DisplayName")]
        [AllowHtml]
        public string DisplayName { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Host")]
        [AllowHtml]
        public string Host { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Port")]
        public int Port { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Password")]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.EnableSsl")]
        public bool EnableSsl { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.UseDefaultCredentials")]
        public bool UseDefaultCredentials { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.IsDefaultEmailAccount")]
        public bool IsDefaultEmailAccount { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.SendTestEmailTo")]
        [AllowHtml]
        public string SendTestEmailTo { get; set; }

        public string TestEmailShortErrorMessage { get; set; }
        public string TestEmailFullErrorMessage { get; set; }
    }

    public partial class EmailAccountValidator : AbstractValidator<EmailAccountModel>
    {
        public EmailAccountValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.DisplayName).NotEmpty();
        }
    }
}