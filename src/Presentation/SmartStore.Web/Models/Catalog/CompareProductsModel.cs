using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class CompareProductsModel : EntityModelBase
    {
        public CompareProductsModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        public IList<ProductOverviewModel> Products { get; set; }

        public bool IncludeShortDescriptionInCompareProducts { get; set; }
        public bool IncludeFullDescriptionInCompareProducts { get; set; }
    }
}