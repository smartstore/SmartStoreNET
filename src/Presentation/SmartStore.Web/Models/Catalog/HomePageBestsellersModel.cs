using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class HomePageBestsellersModel : ModelBase
    {
        public HomePageBestsellersModel()
        {
            Products = new List<ProductOverviewModel>();
        }

        public bool UseSmallProductBox { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }
    }
}