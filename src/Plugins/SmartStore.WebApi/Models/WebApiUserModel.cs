using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.WebApi.Models
{
    public class WebApiUserModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.AdminComment")]
        public string AdminComment { get; set; }

        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public bool Enabled { get; set; }
        public string EnabledFriendly { get; set; }
        public string LastRequestDateFriendly { get; set; }

        public string DisplayApiInfo => (SecretKey.HasValue() ? "table" : "none");
        public string ButtonDisplayEnable => (PublicKey.HasValue() && !Enabled) ? "inline-block" : "none";
        public string ButtonDisplayDisable => Enabled ? "inline-block" : "none";
        public string ButtonDisplayRemoveKeys => PublicKey.HasValue() ? "inline-block" : "none";
        public string ButtonDisplayCreateKeys => !PublicKey.HasValue() ? "inline-block" : "none";
    }
}
