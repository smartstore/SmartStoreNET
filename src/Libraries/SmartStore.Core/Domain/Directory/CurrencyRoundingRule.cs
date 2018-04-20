namespace SmartStore.Core.Domain.Directory
{
    public enum CurrencyRoundingRule
    {
        /// <summary>
        /// E.g. denomination 0.05: 9.225 will round to 9.20
        /// </summary>
        RoundMidpointDown = 0,

        /// <summary>
        /// E.g. denomination 0.05: 9.225 will round to 9.25
        /// </summary>
        RoundMidpointUp,

        /// <summary>
        /// E.g. denomination 0.05: 9.24 will round to 9.20
        /// </summary>
        AlwaysRoundDown,

        /// <summary>
        /// E.g. denomination 0.05: 9.26 will round to 9.30
        /// </summary>
        AlwaysRoundUp
    }
}
