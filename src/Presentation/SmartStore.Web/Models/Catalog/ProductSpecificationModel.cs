using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductSpecificationModel : ModelBase
    {
        public int SpecificationAttributeId { get; set; }

        public string SpecificationAttributeName { get; set; }

        public string SpecificationAttributeOption { get; set; }
    }
}