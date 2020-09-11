using System;
using System.Diagnostics;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents an exchange rate
    /// </summary>
    [DebuggerDisplay("{CurrencyCode} {Rate}")]
    public partial class ExchangeRate
    {
        /// <summary>
        /// Name of the currrency
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The three letter ISO code for the Exchange Rate, e.g. USD
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the currency is available in the store
        /// </summary>
        public bool IsStoreCurrency { get; set; }

        /// <summary>
        /// The conversion rate of this currency from the base currency
        /// </summary>
        public decimal Rate { get; set; } = 1.0m;

        /// <summary>
        /// When was this exchange rate updated from the data source (the internet data xml feed)
        /// </summary>
        public DateTime UpdatedOn { get; set; }
    }
}
