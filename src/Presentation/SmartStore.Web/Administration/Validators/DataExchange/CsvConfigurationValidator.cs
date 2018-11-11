using System.Linq;
using FluentValidation;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.DataExchange
{
	public partial class CsvConfigurationValidator : AbstractValidator<CsvConfigurationModel>
	{
		public CsvConfigurationValidator(ILocalizationService localization)
		{
			RuleFor(x => x.Delimiter)
				.Must(x => !CsvConfiguration.PresetCharacters.Contains(x.ToChar(true)))
				.WithMessage(localization.GetResource("Admin.DataExchange.Csv.Delimiter.Validation"));

			RuleFor(x => x.Quote)
				.Must(x => !CsvConfiguration.PresetCharacters.Contains(x.ToChar(true)))
				.WithMessage(localization.GetResource("Admin.DataExchange.Csv.Quote.Validation"));

			RuleFor(x => x.Escape)
				.Must(x => !CsvConfiguration.PresetCharacters.Contains(x.ToChar(true)))
				.WithMessage(localization.GetResource("Admin.DataExchange.Csv.Escape.Validation"));

			
			RuleFor(x => x.Escape)
				.Must((model, x) => x != model.Delimiter)
				.WithMessage(localization.GetResource("Admin.DataExchange.Csv.EscapeDelimiter.Validation"));

			RuleFor(x => x.Quote)
				.Must((model, x) => x != model.Delimiter)
				.WithMessage(localization.GetResource("Admin.DataExchange.Csv.QuoteDelimiter.Validation"));
		}
	}
}