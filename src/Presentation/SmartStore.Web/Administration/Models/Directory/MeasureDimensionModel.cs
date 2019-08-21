using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(MeasureDimensionValidator))]
    public class MeasureDimensionModel : EntityModelBase, ILocalizedModel<MeasureDimensionLocalizedModel>
    {
        public MeasureDimensionModel()
        {
            Locales = new List<MeasureDimensionLocalizedModel>();
        }

        public IList<MeasureDimensionLocalizedModel> Locales { get; set; }

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

    public class MeasureDimensionLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [AllowHtml, SmartResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.Name")]
        public string Name { get; set; }
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