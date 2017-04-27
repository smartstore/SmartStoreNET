using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class MenuModel : ModelBase
    {
		public bool NewsEnabled { get; set; }
        public bool BlogEnabled { get; set; }
        public bool RecentlyAddedProductsEnabled { get; set; }
        public bool ForumEnabled { get; set; }
        public bool IsAuthenticated { get; set; }
		public bool DisplayLoginLink { get; set; }
		public bool DisplayAdminLink { get; set; }
        public bool IsCustomerImpersonated { get; set; }
        public string CustomerEmailUsername { get; set; }

		public bool HasContactUsPage { get; set; }
	}
}