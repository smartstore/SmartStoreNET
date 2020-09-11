using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class TierPriceExtensions
    {
        /// <summary>
        /// Filter tier prices by a store
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> FilterByStore(this IEnumerable<TierPrice> source, int storeId)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Where(x => x.StoreId == 0 || x.StoreId == storeId);
        }

        /// <summary>
        /// Filter tier prices for a customer
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <param name="customer">Customer</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> FilterForCustomer(this IEnumerable<TierPrice> source, Customer customer)
        {
            Guard.NotNull(source, nameof(source));

            foreach (var tierPrice in source)
            {
                //check customer role requirement
                if (tierPrice.CustomerRole != null)
                {
                    if (customer == null)
                        continue;

                    var customerRoles = customer.CustomerRoleMappings.Select(x => x.CustomerRole).Where(cr => cr.Active);
                    if (!customerRoles.Any())
                        continue;

                    bool roleIsFound = false;
                    foreach (var customerRole in customerRoles)
                    {
                        if (customerRole == tierPrice.CustomerRole)
                            roleIsFound = true;
                    }

                    if (!roleIsFound)
                        continue;
                }

                yield return tierPrice;
            }
        }

        /// <summary>
        /// Remove duplicated quantities (leave only a tier price with minimum price)
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <returns>Filtered tier prices</returns>
        public static ICollection<TierPrice> RemoveDuplicatedQuantities(this ICollection<TierPrice> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            // Find duplicates
            var query = from tierPrice in source
                        group tierPrice by tierPrice.Quantity into g
                        where g.Count() > 1
                        select new { Quantity = g.Key, TierPrices = g.ToList() };
            foreach (var item in query)
            {
                // Find a tier price record with minimum price (we'll not remove it)
                var minTierPrice = item.TierPrices.Aggregate((tp1, tp2) => (tp1.Price < tp2.Price ? tp1 : tp2));
                // Remove all other records
                item.TierPrices.Remove(minTierPrice);
                item.TierPrices.ForEach(x => source.Remove(x));
            }

            return source;
        }
    }
}
