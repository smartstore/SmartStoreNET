using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Tax
{
    [Validator(typeof(TaxCategoryValidator))]
    public class TaxCategoryModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }

    public partial class TaxCategoryValidator : AbstractValidator<TaxCategoryModel>
    {
        public TaxCategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}