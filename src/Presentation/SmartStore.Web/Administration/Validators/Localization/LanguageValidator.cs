using System.Globalization;
using FluentValidation;
using SmartStore.Admin.Models.Localization;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Localization
{
	public partial class LanguageValidator : AbstractValidator<LanguageModel>
    {
        public LanguageValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Fields.Name.Required"));

            RuleFor(x => x.LanguageCulture)
                .Must(x =>
                {
                    try
                    {
                        var culture = new CultureInfo(x);
                        return culture != null;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Fields.LanguageCulture.Validation"));

            RuleFor(x => x.UniqueSeoCode)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Fields.UniqueSeoCode.Required"));

			RuleFor(x => x.UniqueSeoCode)
				//.Length(2)	// Never validates.
				.Length(x => 2)
				.WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Fields.UniqueSeoCode.Length"));
		}
	}
}