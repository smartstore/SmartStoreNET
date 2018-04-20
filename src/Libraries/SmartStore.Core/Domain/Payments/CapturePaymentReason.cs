namespace SmartStore.Core.Domain.Payments
{
	/// <summary>
	/// The reason for automatic capturing of the payment amount.
	/// </summary>
	public enum CapturePaymentReason
	{
		/// <summary>
		/// Capture payment because the order has been marked as shipped.
		/// </summary>
		OrderShipped = 0,

		/// <summary>
		/// Capture payment because the order has been marked as delivered.
		/// </summary>
		OrderDelivered
	}
}
