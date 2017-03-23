using System.IO;
using FluentValidation;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

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
				.Must(x =>
				{
					var isValidPath = 
						x.HasValue() &&
						!x.IsCaseInsensitiveEqual("con") &&
						x != "~/" &&
						x != "~" &&
						x.IndexOfAny(Path.GetInvalidPathChars()) == -1;

					if (isValidPath)
					{
						try
						{
							var unused = CommonHelper.MapPath(x);
							return true;
						}
						catch {	}
					}

					return false;
				})
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