using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    [Validator(typeof(PasswordRecoveryValidator))]
    public partial class PasswordRecoveryModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Account.PasswordRecovery.Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string ResultMessage { get; set; }

        public PasswordRecoveryResultState ResultState { get; set; }
    }

    public enum PasswordRecoveryResultState
    {
        Success,
        Error
    }

    public class PasswordRecoveryValidator : AbstractValidator<PasswordRecoveryModel>
    {
        public PasswordRecoveryValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}