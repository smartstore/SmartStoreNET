using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    [Validator(typeof(PasswordRecoveryConfirmValidator))]
    public partial class PasswordRecoveryConfirmModel : ModelBase
    {
        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.PasswordRecovery.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.PasswordRecovery.ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }

        public bool SuccessfullyChanged { get; set; }
        public string Result { get; set; }
    }

    public class PasswordRecoveryConfirmValidator : AbstractValidator<PasswordRecoveryConfirmModel>
    {
        public PasswordRecoveryConfirmValidator(Localizer T, CustomerSettings customerSettings)
        {
            RuleFor(x => x.NewPassword).NotEmpty();
            RuleFor(x => x.NewPassword).Length(customerSettings.PasswordMinLength, 999);
            RuleFor(x => x.ConfirmNewPassword).NotEmpty();
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage(T("Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch"));
        }
    }
}