using SmartStore.Collections;

namespace SmartStore.Core.Search
{
	public enum AcquirementReason
	{
		Indexing,
		Optimizing,
		Deleting
	}

	public class AcquireWriterContext
	{

		public AcquireWriterContext(AcquirementReason reason)
		{
			Reason = reason;
			LanguageSeoCodes = new string[0];
			CurrencyCodes = new string[0];
			StoreIds = new int[0];
			StoreMappings = new Multimap<int, int>();
			CustomerRoleIds = new int[0];
			CustomerRoleMappings = new Multimap<int, int>();
		}

		/// <summary>
		/// Reason for writer acquirement
		/// </summary>
		public AcquirementReason Reason { get; private set; }

		/// <summary>
		/// Indicates whether old and new search index uses different languages
		/// </summary>
		public bool? LanguagesChangedSinceLastIndexing { get; set; }

		/// <summary>
		/// SEO codes of languages used for indexing
		/// </summary>
		public string[] LanguageSeoCodes { get; set; }

		/// <summary>
		/// Currency codes used for indexing
		/// </summary>
		public string[] CurrencyCodes { get; set; }

		/// <summary>
		/// Array of all store identifiers
		/// </summary>
		public int[] StoreIds { get; set; }

		/// <summary>
		/// Map of product to store identifiers if the product is limited to certain stores
		/// </summary>
		public Multimap<int, int> StoreMappings { get; set; }

		/// <summary>
		/// Array of all customer role identifiers
		/// </summary>
		public int[] CustomerRoleIds { get; set; }

		/// <summary>
		/// Map of product to customer role identifiers if the product is limited to certain customer roles
		/// </summary>
		public Multimap<int, int> CustomerRoleMappings { get; set; }
	}
}
