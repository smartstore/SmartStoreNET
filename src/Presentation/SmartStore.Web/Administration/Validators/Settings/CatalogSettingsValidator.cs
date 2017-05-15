using FluentValidation;
using SmartStore.Admin.Models.Settings;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Settings
{
	public partial class CatalogSettingsValidator : AbstractValidator<CatalogSettingsModel>
    {
        public CatalogSettingsValidator(ILocalizationService localizationService)
        {
			RuleFor(x => x.LabelAsNewForMaxDays)
                .LessThan(1000)
				.WithMessage(localizationService.GetResource("Admin.Validation.ValueRange").FormatWith("0",  "1000"));
		}
    }
}