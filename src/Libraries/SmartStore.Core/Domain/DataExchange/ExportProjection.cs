using System;

namespace SmartStore.Core.Domain.DataExchange
{
	[Serializable]
	public class ExportProjection
	{
		/// <summary>
		/// The language to be applied to the export
		/// </summary>
		public int? LanguageId { get; set; }

		/// <summary>
		/// The currency to be applied to the export
		/// </summary>
		public int? CurrencyId { get; set; }
	}
}
