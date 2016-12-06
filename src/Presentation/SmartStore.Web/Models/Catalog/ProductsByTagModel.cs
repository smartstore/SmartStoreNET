using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductsByTagModel : EntityModelBase
    {
        public string TagName { get; set; }
        public ProductSummaryModel Products { get; set; }
    }
}