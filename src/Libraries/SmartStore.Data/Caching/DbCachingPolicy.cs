using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Domain.Topics;

namespace SmartStore.Data.Caching
{
	/* TODO: (mc)
	 * ========================
	 *		1. Let developers register custom caching policies for single entities (from plugins)
	 *		2. Caching policies should contain expiration info and cacheable rows count
	 *		3. Backend: Let users decide which entities to cache
	 *		4. Backend: Let users purge the cache
	 */

	public partial class DbCachingPolicy
	{
		private static readonly HashSet<string> _cacheableSets = new HashSet<string>
		{
			typeof(AclRecord).Name,
			typeof(ActivityLogType).Name,
			typeof(CategoryTemplate).Name,
			typeof(CheckoutAttribute).Name,
			typeof(CheckoutAttributeValue).Name,
			typeof(Country).Name,
			typeof(Currency).Name,
			typeof(CustomerRole).Name,
			typeof(DeliveryTime).Name,
			typeof(Discount).Name,
			typeof(DiscountRequirement).Name,
			typeof(EmailAccount).Name,
			typeof(Language).Name,
			typeof(ManufacturerTemplate).Name,
			typeof(MeasureDimension).Name,
			typeof(MeasureWeight).Name,
			typeof(MessageTemplate).Name,
			typeof(PaymentMethod).Name,
			typeof(PermissionRecord).Name,
			typeof(ProductTemplate).Name,
			typeof(QuantityUnit).Name,
			typeof(ShippingMethod).Name,
			typeof(StateProvince).Name,
			typeof(Store).Name,
			typeof(StoreMapping).Name,
			typeof(TaxCategory).Name,
			typeof(ThemeVariable).Name,
			typeof(Topic).Name
		};

		/// <summary>
		/// Determines whether the specified command definition can be cached.
		/// </summary>
		/// <param name="affectedEntitySets">Entity sets affected by the command.</param>
		/// <param name="sql">SQL statement for the command.</param>
		/// <param name="parameters">Command parameters.</param>
		/// <returns>
		/// <c>true</c> when the specified command definition can be cached; otherwise, <c>false</c>.
		/// </returns>
		protected internal virtual bool CanBeCached(
			ReadOnlyCollection<EntitySetBase> affectedEntitySets, 
			string sql,
			IEnumerable<KeyValuePair<string, object>> parameters)
		{
			var entitySets = affectedEntitySets.Select(x => x.Name);
			var result = entitySets.All(x => _cacheableSets.Contains(x));
			return result;
		}

		/// <summary>
		/// Gets the minimum and maximum number cacheable rows for a given command definition.
		/// </summary>
		/// <param name="affectedEntitySets">Entity sets affected by the command.</param>
		/// <param name="minCacheableRows">The minimum number of cacheable rows.</param>
		/// <param name="maxCacheableRows">The maximum number of cacheable rows.</param>
		protected internal virtual void GetCacheableRows(
			ReadOnlyCollection<EntitySetBase> affectedEntitySets,
			out int minCacheableRows, 
			out int maxCacheableRows)
		{
			minCacheableRows = 0;
			maxCacheableRows = 5000; // int.MaxValue;
		}

		/// <summary>
		/// Gets the expiration timeout for a given command definition.
		/// </summary>
		/// <param name="affectedEntitySets">Entity sets affected by the command.</param>
		/// <returns>The absolute TTL</returns>
		protected internal virtual TimeSpan? GetExpirationTimeout(ReadOnlyCollection<EntitySetBase> affectedEntitySets)
		{
			return TimeSpan.FromDays(1);
		}
	}
}
