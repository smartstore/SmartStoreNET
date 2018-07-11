using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(MeasureWeightValidator))]
    public class MeasureWeightModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.SystemKeyword")]
        [AllowHtml]
        public string SystemKeyword { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.Ratio")]
        [UIHint("Decimal8")]
        public decimal Ratio { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.IsPrimaryWeight")]
        public bool IsPrimaryWeight { get; set; }
    }

    public partial class MeasureWeightValidator : AbstractValidator<MeasureWeightModel>
    {
        public MeasureWeightValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.SystemKeyword).NotEmpty();
        }
    }
}