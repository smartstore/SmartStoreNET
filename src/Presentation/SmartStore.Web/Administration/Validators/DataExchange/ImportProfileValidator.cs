using FluentValidation;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.DataExchange
{
	public partial class ImportProfileValidator : AbstractValidator<ImportProfileModel>
	{
		public ImportProfileValidator(ILocalizationService localization)
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage(localization.GetResource("Admin.Validation.Name"));

			RuleFor(x => x.KeyFieldNames)
				.NotEmpty()
				.When(x => x.Id != 0)
				.WithMessage(localization.GetResource("Admin.DataExchange.Import.Validate.OneKeyFieldRequired"));

			RuleFor(x => x.Skip)
				.GreaterThanOrEqualTo(0)
				.WithMessage(localization.GetResource("Admin.Common.SkipAndTakeGreaterThanOrEqualZero"));

			RuleFor(x => x.Take)
				.GreaterThanOrEqualTo(0)
				.WithMessage(localization.GetResource("Admin.Common.SkipAndTakeGreaterThanOrEqualZero"));
		}
	}
}