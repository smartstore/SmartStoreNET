using FluentValidation;
using SmartStore.Admin.Models.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Directory
{
	public partial class MeasureDimensionValidator : AbstractValidator<MeasureDimensionModel>
    {
        public MeasureDimensionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Dimensions.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword.Required"));
        }
    }
}