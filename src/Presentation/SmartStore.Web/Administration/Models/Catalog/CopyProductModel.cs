using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(CopyProductValidator))]
    public class CopyProductModel : EntityModelBase
    {
        public CopyProductModel()
        {
            NumberOfCopies = 1;
        }

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.NumberOfCopies")]
        public int NumberOfCopies { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.CopyImages")]
        public bool CopyImages { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.Published")]
        public bool Published { get; set; }
    }
}