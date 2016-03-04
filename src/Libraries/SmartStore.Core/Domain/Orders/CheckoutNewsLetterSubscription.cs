namespace SmartStore.Core.Domain.Orders
{
	/// <summary>
	/// Setting for newsletter subscription in checkout
	/// </summary>
	public enum CheckoutNewsLetterSubscription
	{
		/// <summary>
		/// No newsletter subscription checkbox
		/// </summary>
		None = 0,

		/// <summary>
		/// Deactivated newsletter subscription checkbox
		/// </summary>
		Deactivated,

		/// <summary>
		/// Activated newsletter subscription checkbox
		/// </summary>
		Activated
	}
}
