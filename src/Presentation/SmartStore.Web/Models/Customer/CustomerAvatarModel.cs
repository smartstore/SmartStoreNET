using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAvatarModel : ModelBase
    {
        public bool Large { get; set; }
        public string PictureUrl { get; set; }
        public string LinkUrl { get; set; }
        public string AvatarColor { get; set; }
        public char AvatarLetter { get; set; }
        public string UserName { get; set; }
    }

    public partial class CustomerAvatarEditModel : ModelBase
    {
		public string MaxFileSize { get; set; }
        public CustomerAvatarModel Avatar { get; set; }
    }
}