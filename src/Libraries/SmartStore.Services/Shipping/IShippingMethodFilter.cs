using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Shipping
{
	public partial interface IShippingMethodFilter
	{
		/// <summary>
		/// Gets a value indicating whether a shipping method should be filtered out
		/// </summary>
		/// <param name="request">Shipping filter request</param>
		/// <returns><c>true</c> filter out method, <c>false</c> do not filter out method</returns>
		bool IsExcluded(ShippingFilterRequest request);

		/// <summary>
		/// Get URL for filter configuration
		/// </summary>
		/// <param name="shippingMethodId">Shipping method identifier</param>
		/// <returns>URL for filter configuration</returns>
		string GetConfigurationUrl(int shippingMethodId);
	}


	public partial class ShippingFilterRequest
	{
		/// <summary>
		/// The shipping method to be checked
		/// </summary>
		public ShippingMethod ShippingMethod { get; set; }

		/// <summary>
		/// Shipping method request
		/// </summary>
		public GetShippingOptionRequest Option { get; set; }
	}
}
