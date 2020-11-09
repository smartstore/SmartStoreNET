using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAvatarModel : EntityModelBase
    {
        public bool Large { get; set; }
        public int? FileId { get; set; }
        public string AvatarColor { get; set; }
        public char AvatarLetter { get; set; }
        public string UserName { get; set; }

        public bool AllowViewingProfiles { get; set; }
        public int AvatarPictureSize { get; set; }
    }

    public partial class CustomerAvatarEditModel : ModelBase
    {
        public string MaxFileSize { get; set; }
        public CustomerAvatarModel Avatar { get; set; }
    }
}