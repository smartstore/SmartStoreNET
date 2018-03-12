namespace SmartStore.Core.Domain.Customers
{
	/// <summary>
	/// Represents the reason for posting a wallet history entry.
	/// </summary>
	public enum WalletPostingReason
	{
		Admin = 0,
		Purchase,
		Refill,
		Refund
	}
}
