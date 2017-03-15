using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ManufacturerOverviewModel : EntityModelBase
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public string Description { get; set; }
        public PictureModel Picture { get; set; }
    }
}