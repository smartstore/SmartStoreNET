using FluentValidation;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.DataExchange
{
	public partial class ExportProfileValidator : AbstractValidator<ExportProfileModel>
	{
		public ExportProfileValidator(ILocalizationService localization)
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage(localization.GetResource("Admin.Validation.Name"));
		}
	}
}