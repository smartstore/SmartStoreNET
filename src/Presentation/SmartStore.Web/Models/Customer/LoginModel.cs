using System.ComponentModel.DataAnnotations;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class LoginModel : ModelBase
    {
        public bool CheckoutAsGuest { get; set; }

        public CustomerLoginType CustomerLoginType { get; set; }

        [SmartResourceDisplayName("Account.Login.Fields.Email")]
        public string Email { get; set; }

        [SmartResourceDisplayName("Account.Login.Fields.UserName")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Account.Login.Fields.UsernameOrEmail")]
        public string UsernameOrEmail { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.Login.Fields.Password")]
        public string Password { get; set; }

        [SmartResourceDisplayName("Account.Login.Fields.RememberMe")]
        public bool RememberMe { get; set; }

        public bool DisplayCaptcha { get; set; }

    }
}