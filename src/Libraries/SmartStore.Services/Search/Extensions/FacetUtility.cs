using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Extensions
{
    /// <summary>
    /// Contains methods that are specifically required for facet processing.
    /// </summary>
    public static class FacetUtility
    {
        private const double MAX_PRICE = 1000000000;

        private static int[,] _priceThresholds = new int[,]
        {
            { 10, 5 },
            { 25, 15 },
            { 200, 25 },
            { 500, 50 },
            { 1000, 100 },
            { 2000, 250 },
            { 5000, 500 },
            { 10000, 1000 },
            { 20000, 2500 },
            { 50000, 5000 },
            { 100000, 10000 },
            { 200000, 25000 },
            { 500000, 50000 },
            { 1000000, 100000 },
            { 2000000, 250000 },
            { 5000000, 500000 },
            { 10000000, 1000000 },
            { 20000000, 2500000 },
            { 50000000, 5000000 }
        };

        public static double GetNextPrice(double price)
        {
            for (var i = 0; i <= _priceThresholds.GetUpperBound(0); ++i)
            {
                if (price < _priceThresholds[i, 0])
                    return price + _priceThresholds[i, 1];
            }

            return MAX_PRICE;
        }

        public static double MakePriceEven(double price)
        {
            if (price == 0.0)
                return GetNextPrice(0.0);

            // Get previous threshold for price.
            var result = 0.0;
            for (var i = 1; i <= _priceThresholds.GetUpperBound(0) && result == 0.0; ++i)
            {
                if (price < _priceThresholds[i, 0])
                    result = _priceThresholds[i - 1, 0];
            }

            while (result < price && result < MAX_PRICE)
            {
                result = GetNextPrice(result);
            }

            return result;
        }

        public static List<Facet> GetLessPriceFacets(List<Facet> facets, int maxNumber)
        {
            const double expFactor = 2.0;
            const double flatten = 2.0;

            if (facets.Count <= 3)
                return facets;

            // Remove too granular facets.
            if (facets.Any(x => x.Value.UpperValue != null && (double)x.Value.UpperValue == 25.0))
            {
                facets.RemoveFacet(5.0, true);
                facets.RemoveFacet(10.0, true);
            }

            var result = new List<Facet>();
            var expIndexes = new HashSet<int>();
            var lastIndex = facets.Count - 1;

            // Get exponential distributed indexes.
            for (var i = 0.0; i < lastIndex; ++i)
            {
                var x = (int)Math.Floor(Math.Pow(expFactor, i / flatten));
                expIndexes.Add(x);
            }

            for (var index = 0; index <= lastIndex; ++index)
            {
                var facet = facets[index];

                // Always return first, last and selected facets.
                if (index == 0 || index == lastIndex || facet.Value.IsSelected)
                {
                    result.Add(facet);
                }
                else if (expIndexes.Contains(index) && result.Count < maxNumber && index < (lastIndex - 1))
                {
                    result.Add(facet);
                }
            }

            return result;
        }

        public static IEnumerable<FacetValue> GetRatings()
        {
            var count = 0;
            for (double rate = 1.0; rate <= 5.0; ++rate)
            {
                yield return new FacetValue(rate, IndexTypeCode.Double)
                {
                    DisplayOrder = ++count
                };
            }
        }

        public static IQueryable<Customer> GetCustomersByNumberOfPosts(
            IRepository<ForumPost> forumPostRepository,
            IRepository<StoreMapping> storeMappingRepository,
            int storeId,
            int minHitCount = 1)
        {
            var postQuery = forumPostRepository.TableUntracked
                .Expand(x => x.Customer)
                .Expand(x => x.Customer.BillingAddress)
                .Expand(x => x.Customer.ShippingAddress)
                .Expand(x => x.Customer.Addresses);

            if (storeId > 0)
            {
                postQuery =
                    from p in postQuery
                    join sm in storeMappingRepository.TableUntracked on new { eid = p.ForumTopic.Forum.ForumGroupId, ename = "ForumGroup" } equals new { eid = sm.EntityId, ename = sm.EntityName } into gsm
                    from sm in gsm.DefaultIfEmpty()
                    where !p.ForumTopic.Forum.ForumGroup.LimitedToStores || sm.StoreId == storeId
                    select p;
            }

            var groupQuery =
                from p in postQuery
                group p by p.CustomerId into grp
                select new
                {
                    Count = grp.Count(),
                    grp.FirstOrDefault().Customer   // Cannot be null.
                };

            groupQuery = minHitCount > 1
                ? groupQuery.Where(x => x.Count >= minHitCount)
                : groupQuery;

            var query = groupQuery
                .OrderByDescending(x => x.Count)
                .Select(x => x.Customer)
                .Where(x => x.CustomerRoleMappings.FirstOrDefault(y => y.CustomerRole.SystemName == SystemCustomerRoleNames.Guests) == null && !x.Deleted && x.Active && !x.IsSystemAccount);

            return query;
        }

        /// <summary>
        /// Gets the string resource key for a facet group kind
        /// </summary>
        /// <param name="kind">Facet group kind</param>
        /// <returns>Resource key</returns>
        public static string GetLabelResourceKey(FacetGroupKind kind)
        {
            switch (kind)
            {
                case FacetGroupKind.Category:
                    return "Search.Facet.Category";
                case FacetGroupKind.Brand:
                    return "Search.Facet.Manufacturer";
                case FacetGroupKind.Price:
                    return "Search.Facet.Price";
                case FacetGroupKind.Rating:
                    return "Search.Facet.Rating";
                case FacetGroupKind.DeliveryTime:
                    return "Search.Facet.DeliveryTime";
                case FacetGroupKind.Availability:
                    return "Search.Facet.Availability";
                case FacetGroupKind.NewArrivals:
                    return "Search.Facet.NewArrivals";
                case FacetGroupKind.Forum:
                    return "Search.Facet.Forum";
                case FacetGroupKind.Customer:
                    return "Search.Facet.Customer";
                case FacetGroupKind.Date:
                    return "Search.Facet.Date";
                default:
                    return null;
            }
        }

        public static string GetFacetAliasSettingKey(FacetGroupKind kind, int languageId, string scope = null)
        {
            if (scope.HasValue())
            {
                return $"FacetGroupKind-{kind.ToString()}-Alias-{languageId}-{scope}";
            }

            return $"FacetGroupKind-{kind.ToString()}-Alias-{languageId}";
        }
    }
}
