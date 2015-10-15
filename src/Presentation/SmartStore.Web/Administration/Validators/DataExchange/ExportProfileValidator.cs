using System.IO;
using System.Linq;
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
				.Must(x => x.HasValue() && !x.IsCaseInsensitiveEqual("con") && !Path.GetInvalidFileNameChars().Any(y => x.Contains(y)))
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.FolderAndFileName.Validate"));

			RuleFor(x => x.FileNamePattern)
				.NotEmpty()
				.WithMessage(localization.GetResource("Admin.DataExchange.Export.FolderAndFileName.Validate"));
		}
	}
}