using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;

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
			Languages = new List<Language>();
			Currencies = new List<Currency>();
			StoreIds = new int[0];
			StoreMappings = new Multimap<int, int>();
			CustomerRoleIds = new int[0];
			CustomerRoleMappings = new Multimap<int, int>();
			DeliveryTimes = new Dictionary<int, DeliveryTime>();
			Manufacturers = new Dictionary<int, Manufacturer>();
			Categories = new Dictionary<int, Category>();
			CustomProperties = new Dictionary<string, object>();
		}

		/// <summary>
		/// Reason for writer acquirement
		/// </summary>
		public AcquirementReason Reason { get; private set; }

		/// <summary>
		/// All languages
		/// </summary>
		public IList<Language> Languages { get; set; }

		/// <summary>
		/// Currency codes used for indexing
		/// </summary>
		public IList<Currency> Currencies { get; set; }

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

		/// <summary>
		/// All manufacturers
		/// </summary>
		public Dictionary<int, Manufacturer> Manufacturers { get; set; }

		/// <summary>
		/// All categories
		/// </summary>
		public Dictionary<int, Category> Categories { get; set; }

		/// <summary>
		/// All delivery times
		/// </summary>
		public Dictionary<int, DeliveryTime> DeliveryTimes { get; set; }

		/// <summary>
		/// Use this dictionary for any custom data required along indexing
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }
	}
}
