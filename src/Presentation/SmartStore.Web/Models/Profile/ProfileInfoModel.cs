using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Customer;

namespace SmartStore.Web.Models.Profile
{
    public partial class ProfileInfoModel : EntityModelBase
    {
        public CustomerAvatarModel Avatar { get; set; }

        public bool LocationEnabled { get; set; }
        public string Location { get; set; }

        public bool PMEnabled { get; set; }

        public bool TotalPostsEnabled { get; set; }
        public int TotalPosts { get; set; }

        public bool JoinDateEnabled { get; set; }
        public string JoinDate { get; set; }

        public bool DateOfBirthEnabled { get; set; }
        public string DateOfBirth { get; set; }
    }
}