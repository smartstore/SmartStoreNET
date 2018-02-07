using SmartStore.Utilities;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAvatarModel : ModelBase
    {
		public string MaxFileSize { get; set; }
		public string PictureFallbackUrl { get; set; }
		public string AvatarUrl { get; set; }

		public string RandomAvatarUrl
		{
			get
			{
				if (AvatarUrl.HasValue())
				{
					return string.Concat(AvatarUrl, AvatarUrl.Contains("?") ? "&rnd=" : "?rnd=", CommonHelper.GenerateRandomInteger());
				}

				return null;
			}
		}
	}
}