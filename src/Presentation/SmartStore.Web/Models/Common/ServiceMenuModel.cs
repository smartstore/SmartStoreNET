using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class ServiceMenuModel : ModelBase
    {
        public bool RecentlyAddedProductsEnabled { get; set; }
        public bool RecentlyViewedProductsEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }
        public bool BlogEnabled { get; set; }
        public bool ForumEnabled { get; set; }
        public bool ManufacturerEnabled { get; set; }
        public bool AllowPrivateMessages { get; set; }
	}
}