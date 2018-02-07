using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAvatarModel : ModelBase
    {
		public string MaxFileSize { get; set; }
		public string PictureFallbackUrl { get; set; }
		public string AvatarUrl { get; set; }
	}
}