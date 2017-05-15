using SmartStore.Core.Configuration;

namespace SmartStore.ShippingByWeight
{
    public class ShippingByWeightSettings : ISettings
    {
		public ShippingByWeightSettings()
		{
			IncludeWeightOfFreeShippingProducts = true;
		}

		public bool LimitMethodsToCreated { get; set; }

        public bool CalculatePerWeightUnit { get; set; }

		/// <summary>
		/// Whether to include the weight of free shipping products in shipping calculation
		/// </summary>
		public bool IncludeWeightOfFreeShippingProducts { get; set; }
	}
}