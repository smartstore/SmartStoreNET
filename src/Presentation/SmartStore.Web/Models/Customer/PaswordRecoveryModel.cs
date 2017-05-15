using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Validators.Customer;

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
}