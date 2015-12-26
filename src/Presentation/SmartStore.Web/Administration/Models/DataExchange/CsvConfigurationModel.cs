using System;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.DataExchange;
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
			Delimiter = config.Delimiter;
			Quote = config.Quote;
			Escape = config.Escape;
		}

		[SmartResourceDisplayName("Admin.DataExchange.Csv.QuoteAllFields")]
		public bool QuoteAllFields { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Csv.TrimValues")]
		public bool TrimValues { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Csv.SupportsMultiline")]
		public bool SupportsMultiline { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Csv.Delimiter")]
		public char Delimiter { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Csv.Quote")]
		public char Quote { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Csv.Escape")]
		public char Escape { get; set; }

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
			config.Delimiter = Delimiter;
			config.Quote = Quote;
			config.Escape = Escape;

			return config;
		}
	}
}