using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Shipping
{
    public class ShippingSettings : ISettings
    {
        public ShippingSettings()
        {
			ActiveShippingRateComputationMethodSystemNames = new List<string>() { "Shipping.FixedRate" };
			EstimateShippingEnabled = true;
			ReturnValidOptionsIfThereAreAny = true;
        }

        /// <summary>
        /// Gets or sets an system names of active shipping rate computation methods
        /// </summary>
        public List<string> ActiveShippingRateComputationMethodSystemNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Free shipping over X' is enabled
        /// </summary>
        public bool FreeShippingOverXEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value of 'Free shipping over X' option
        /// </summary>
        public decimal FreeShippingOverXValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Free shipping over X' option
        /// should be evaluated over 'X' value including tax or not
        /// </summary>
        public bool FreeShippingOverXIncludingTax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Estimate shipping' option is enabled
        /// </summary>
        public bool EstimateShippingEnabled { get; set; }

        /// <summary>
        /// A value indicating whether customers should see shipment events on their order details pages
        /// </summary>
        public bool DisplayShipmentEventsToCustomers { get; set; }

        /// <summary>
        /// Gets or sets shipping origin address
        /// </summary>
        public int ShippingOriginAddressId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should return valid options if there are any (no matter of the errors returned by other shipping rate compuation methods).
        /// </summary>
        public bool ReturnValidOptionsIfThereAreAny { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display shipping options during checkout process only if more then one option is available
        /// </summary>
        public bool SkipShippingIfSingleOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to charge only the highest shipping surcharge of products
        /// </summary>
        public bool ChargeOnlyHighestProductShippingSurcharge { get; set; }
    }
}