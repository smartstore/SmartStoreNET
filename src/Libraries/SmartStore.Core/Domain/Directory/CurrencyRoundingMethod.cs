namespace SmartStore.Core.Domain.Directory
{
	/// <summary>
	/// <see cref="https://en.wikipedia.org/wiki/Cash_rounding"/>
	/// </summary>
	public enum CurrencyRoundingMethod
	{
		/// <summary>
		/// Default rounding. Round to nearest 0.01 even number (following IEEE 754).
		/// </summary>
		Default = 0,

		/// <summary>
		/// Round down to nearest 0.05
		/// </summary>
		Down005,

        /// <summary>
        /// Round up to nearest 0.05
        /// </summary>
        Up005,

        /// <summary>
        /// Round down to nearest 0.10
        /// </summary>
        Down01,

        /// <summary>
        /// Round up to nearest 0.10
        /// </summary>
        Up01,

        /// <summary>
        /// 0.01 - 0.24: down to 0.00
        /// 0.25 - 0.49: up to 0.50
        /// 0.51 - 0.74: down to 0.50
        /// 0.75 - 0.99: up to next integer
        /// </summary>
        Interval05,

		/// <summary>
		/// 0.01 - 0.49: down to 0.00
		/// 0.50 - 0.99: up to next integer
		/// </summary>
		Interval1,

		/// <summary>
		/// Always round up decimals to next integer
		/// </summary>
		Up1
	}
}
