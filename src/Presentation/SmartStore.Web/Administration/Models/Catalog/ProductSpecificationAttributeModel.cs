using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class ProductSpecificationAttributeModel : EntityModelBase
    {
        public ProductSpecificationAttributeModel()
        {
            SpecificationAttributeOptions = new List<SpecificationAttributeOption>();
        }

        [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
        [AllowHtml]
        public string SpecificationAttributeName { get; set; }

        public int SpecificationAttributeId { get; set; }
        public int SpecificationAttributeOptionId { get; set; }

        public string SpecificationAttributeOptionsUrl { get; set; }

        public List<SpecificationAttributeOption> SpecificationAttributeOptions { get; set; }


        [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttributeOption")]
        public string SpecificationAttributeOptionName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AllowFiltering")]
        public bool? AllowFiltering { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.ShowOnProductPage")]
        public bool? ShowOnProductPage { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #region Nested classes

        public partial class SpecificationAttributeOption : EntityModelBase
        {
            public int id { get; set; }

            public string name { get; set; }

            public string text { get; set; }
        }

        #endregion

    }
}