using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAvatarModel : ModelBase
    {
        public string AvatarUrl { get; set; }
		public string MaxFileSize { get; set; }
    }
}