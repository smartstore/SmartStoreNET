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
    [Validator(typeof(MeasureWeightValidator))]
    public class MeasureWeightModel : EntityModelBase, ILocalizedModel<MeasureWeightLocalizedModel>
    {
        public MeasureWeightModel()
        {
            Locales = new List<MeasureWeightLocalizedModel>();
        }

        public IList<MeasureWeightLocalizedModel> Locales { get; set; }

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

    public class MeasureWeightLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [AllowHtml, SmartResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.Name")]
        public string Name { get; set; }
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