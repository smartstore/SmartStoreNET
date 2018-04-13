using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductTagModel : EntityModelBase
    {
        public LocalizedValue<string> Name { get; set; }

        public string SeName { get; set; }

        public int ProductCount { get; set; }
    }
}