using System.IO;
using System.Linq;
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

			RuleFor(x => x.FolderName)
				.Must(x => x.HasValue() && !x.IsCaseInsensitiveEqual("con") && !Path.GetInvalidFileNameChars().Any(y => x.Contains(y)))
				.WithMessage(localization.GetResource("Admin.DataExchange.Import.FolderName.Validate"));
		}
	}
}