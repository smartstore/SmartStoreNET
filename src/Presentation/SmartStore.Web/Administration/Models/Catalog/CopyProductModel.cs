using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
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

        [SmartResourceDisplayName("Admin.Catalog.Products.Copy.Published")]
        public bool Published { get; set; }
    }

    public partial class CopyProductValidator : AbstractValidator<CopyProductModel>
    {
        public CopyProductValidator()
        {
            RuleFor(x => x.NumberOfCopies).NotEmpty().GreaterThan(0);
        }
    }
}