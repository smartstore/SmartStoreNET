using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ManufacturerOverviewModel : EntityModelBase
    {
        public LocalizedValue<string> Name { get; set; }
        public LocalizedValue<string> Description { get; set; }
		public string SeName { get; set; }
		public PictureModel Picture { get; set; }
    }
}