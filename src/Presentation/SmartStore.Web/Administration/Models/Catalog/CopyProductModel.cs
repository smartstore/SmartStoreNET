using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class CopyProductModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.CopyImages")]
        public bool CopyImages { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.Published")]
        public bool Published { get; set; }
    }
}