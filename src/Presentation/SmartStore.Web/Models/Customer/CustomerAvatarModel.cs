using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAvatarModel : ModelBase
    {
        public string AvatarUrl { get; set; }
		public string MaxFileSize { get; set; }
        public CustomerNavigationModel NavigationModel { get; set; }
    }
}