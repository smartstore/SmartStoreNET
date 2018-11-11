using System;

namespace SmartStore.Core.Domain.DataExchange
{
	/// <summary>
	/// Supported entity types
	/// </summary>
	public enum ImportEntityType
	{
		Product = 0,
		Category,
		Customer,
		NewsLetterSubscription
	}

	public enum ImportFileType
	{
		CSV = 0,
		XLSX
	}

	[Flags]
	public enum ImportModeFlags
	{
		Insert = 1,
		Update = 2
	}
}
