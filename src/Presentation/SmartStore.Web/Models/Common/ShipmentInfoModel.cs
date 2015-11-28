using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Topics;

namespace SmartStore.Web.Models.Common
{
    public partial class ShipmentInfoModel : ModelBase
    {
        public ShipmentInfoModel()
        {
            Products = new List<ProductOverviewModel>();
            Categories = new List<CategoryModel>();
            Manufacturers = new List<ManufacturerModel>();
            Topics = new List<TopicModel>();
        }
        public IList<ProductOverviewModel> Products { get; set; }
        public IList<CategoryModel> Categories { get; set; }
        public IList<ManufacturerModel> Manufacturers { get; set; }
        public IList<TopicModel> Topics { get; set; }
    }
}