using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(MeasureDimensionValidator))]
    public class MeasureDimensionModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword")]
        [AllowHtml]
        public string SystemKeyword { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.Ratio")]
        [UIHint("Decimal8")]
        public decimal Ratio { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.IsPrimaryWeight")]
        public bool IsPrimaryDimension { get; set; }
    }

    public partial class MeasureDimensionValidator : AbstractValidator<MeasureDimensionModel>
    {
        public MeasureDimensionValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.SystemKeyword).NotEmpty();
        }
    }
}