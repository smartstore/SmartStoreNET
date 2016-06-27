using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class AccountDropdownModel : EntityModelBase
    {
        public string Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool DisplayAdminLink { get; set; }
        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }

        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }

        public bool AllowPrivateMessages { get; set; }
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }

        [SmartResourceDisplayName("Account.Login.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }
        [SmartResourceDisplayName("Account.Login.Fields.UserName")]
        [AllowHtml]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.Login.Fields.Password")]
        [AllowHtml]
        public string Password { get; set; }

        [SmartResourceDisplayName("Account.Login.Fields.RememberMe")]
        public bool RememberMe { get; set; }
    }
}