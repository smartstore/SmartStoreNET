using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Localization;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.DataExchange
{
    [Validator(typeof(CsvConfigurationValidator))]
    public class CsvConfigurationModel : ModelBase, ICloneable<CsvConfiguration>
    {
        public CsvConfigurationModel()
        {
        }

        public CsvConfigurationModel(CsvConfiguration config)
        {
            QuoteAllFields = config.QuoteAllFields;
            TrimValues = config.TrimValues;
            SupportsMultiline = config.SupportsMultiline;
            Delimiter = Regex.Escape(config.Delimiter.ToString());
            Quote = Regex.Escape(config.Quote.ToString());
            Escape = Regex.Escape(config.Escape.ToString());
        }

        [SmartResourceDisplayName("Admin.DataExchange.Csv.QuoteAllFields")]
        public bool QuoteAllFields { get; set; }

        [SmartResourceDisplayName("Admin.DataExchange.Csv.TrimValues")]
        public bool TrimValues { get; set; }

        [SmartResourceDisplayName("Admin.DataExchange.Csv.SupportsMultiline")]
        public bool SupportsMultiline { get; set; }

        [SmartResourceDisplayName("Admin.DataExchange.Csv.Delimiter")]
        public string Delimiter { get; set; }

        [SmartResourceDisplayName("Admin.DataExchange.Csv.Quote")]
        public string Quote { get; set; }

        [SmartResourceDisplayName("Admin.DataExchange.Csv.Escape")]
        public string Escape { get; set; }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public CsvConfiguration Clone()
        {
            var config = new CsvConfiguration();
            config.QuoteAllFields = QuoteAllFields;
            config.TrimValues = TrimValues;
            config.SupportsMultiline = SupportsMultiline;
            config.Delimiter = Delimiter.ToChar(true);
            config.Quote = Quote.ToChar(true);
            config.Escape = Escape.ToChar(true);

            return config;
        }
    }

    public partial class CsvConfigurationValidator : AbstractValidator<CsvConfigurationModel>
    {
        public CsvConfigurationValidator(Localizer T)
        {
            RuleFor(x => x.Delimiter)
                .Must(x => !CsvConfiguration.PresetCharacters.Contains(x.ToChar(true)))
                .WithMessage(T("Admin.DataExchange.Csv.Delimiter.Validation"));

            RuleFor(x => x.Quote)
                .Must(x => !CsvConfiguration.PresetCharacters.Contains(x.ToChar(true)))
                .WithMessage(T("Admin.DataExchange.Csv.Quote.Validation"));

            RuleFor(x => x.Escape)
                .Must(x => !CsvConfiguration.PresetCharacters.Contains(x.ToChar(true)))
                .WithMessage(T("Admin.DataExchange.Csv.Escape.Validation"));

            RuleFor(x => x.Escape)
                .Must((model, x) => x != model.Delimiter)
                .WithMessage(T("Admin.DataExchange.Csv.EscapeDelimiter.Validation"));

            RuleFor(x => x.Quote)
                .Must((model, x) => x != model.Delimiter)
                .WithMessage(T("Admin.DataExchange.Csv.QuoteDelimiter.Validation"));
        }
    }
}