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

			RuleFor(x => x.FolderName)
				.Must(x => x.HasValue() && !x.IsCaseInsensitiveEqual("con"))
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.FolderName.Validate"));

			RuleFor(x => x.FileNamePattern)
				.NotEmpty()
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.FileNamePattern.Validate"));

			RuleFor(x => x.Offset)
				.GreaterThanOrEqualTo(0)
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.Partition.Validate"));

			RuleFor(x => x.Limit)
				.GreaterThanOrEqualTo(0)
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.Partition.Validate"));

			RuleFor(x => x.BatchSize)
				.GreaterThanOrEqualTo(0)
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.Partition.Validate"));
		}
	}
}