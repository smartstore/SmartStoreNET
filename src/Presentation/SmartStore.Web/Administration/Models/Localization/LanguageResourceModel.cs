using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Localization
{
    [Validator(typeof(LanguageResourceValidator))]
    public class LanguageResourceModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.Name")]
        [AllowHtml]
        public string ResourceName { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.Value")]
        [AllowHtml]
        public string ResourceValue { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.LanguageName")]
        [AllowHtml]
        public string LanguageName { get; set; }

        public int LanguageId { get; set; }
    }

    public partial class LanguageResourceValidator : AbstractValidator<LanguageResourceModel>
    {
        public LanguageResourceValidator()
        {
            RuleFor(x => x.ResourceName).NotEmpty();
            RuleFor(x => x.ResourceValue).NotEmpty();
        }
    }
}