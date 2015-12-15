namespace SmartStore.Core.Domain.DataExchange
{
	/// <summary>
	/// Supported entity types
	/// </summary>
	public enum ImportEntityType
	{
		Product = 0,
		Customer,
		NewsLetterSubscription
	}

	public enum ImportFileType
	{
		CSV = 0,
		XLSX
	}
}
