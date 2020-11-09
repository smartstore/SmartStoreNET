using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ProductTagValidator))]
    public class ProductTagModel : EntityModelBase, ILocalizedModel<ProductTagLocalizedModel>
    {
        public ProductTagModel()
        {
            Locales = new List<ProductTagLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Catalog.ProductTags.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductTags.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductTags.Fields.ProductCount")]
        public int ProductCount { get; set; }

        public IList<ProductTagLocalizedModel> Locales { get; set; }
    }

    public class ProductTagLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductTags.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }

    public partial class ProductTagValidator : AbstractValidator<ProductTagModel>
    {
        public ProductTagValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}