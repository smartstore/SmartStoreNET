using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Search.Facets
{
    public static class FacetExtensions
    {
        /// <summary>
        /// Orders a facet sequence
        /// </summary>
        /// <param name="source">Facets</param>
        /// <param name="descriptor">Facet descriptor</param>
        /// <returns>Ordered facets</returns>
        public static IOrderedEnumerable<Facet> OrderBy(this IEnumerable<Facet> source, FacetDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            return source.OrderBy(descriptor.OrderBy, descriptor.IsMultiSelect);
        }

        /// <summary>
        /// Orders a facet sequence
        /// </summary>
        /// <param name="source">Facets</param>
        /// <param name="sorting">Type of sorting</param>
        /// <param name="selectedFirst">Whether to display selected facets first</param>
        /// <returns>Ordered facets</returns>
        public static IOrderedEnumerable<Facet> OrderBy(this IEnumerable<Facet> source, FacetSorting sorting, bool selectedFirst = true)
        {
            Guard.NotNull(source, nameof(source));

            switch (sorting)
            {
                case FacetSorting.ValueAsc:
                    if (selectedFirst)
                        return source
                            .OrderByDescending(x => x.Value.IsSelected)
                            .ThenBy(x => x.Value.Value);
                    else
                        return source.OrderBy(x => x.Value.Value);

                case FacetSorting.LabelAsc:
                    if (selectedFirst)
                        return source
                            .OrderByDescending(x => x.Value.IsSelected)
                            .ThenBy(x => x.Value.Label);
                    else
                        return source.OrderBy(x => x.Value.Label);

                case FacetSorting.DisplayOrder:
                    if (selectedFirst)
                        return source
                            .OrderByDescending(x => x.Value.IsSelected)
                            .ThenBy(x => x.Value.DisplayOrder);
                    else
                        return source.OrderBy(x => x.Value.DisplayOrder);

                default:
                    if (selectedFirst)
                        return source
                            .OrderByDescending(x => x.Value.IsSelected)
                            .ThenByDescending(x => x.HitCount)
                            .ThenBy(x => x.Value.Label);
                    else
                        return source
                            .OrderByDescending(x => x.HitCount)
                            .ThenBy(x => x.Value.Label);
            }
        }

        /// <summary>
        /// Removes a facet
        /// </summary>
        /// <param name="facets">List of facets</param>
        /// <param name="value">Facet value</param>
        /// <param name="upperValue">Whether to compare the upper value</param>
        public static void RemoveFacet(this IList<Facet> facets, object value, bool upperValue)
        {
            Facet facet = null;

            if (upperValue)
            {
                facet = facets.FirstOrDefault(x => x.Value.UpperValue != null && x.Value.UpperValue.Equals(value));
            }
            else
            {
                facet = facets.FirstOrDefault(x => x.Value.Value != null && x.Value.Value.Equals(value));
            }

            if (facet != null)
            {
                facets.Remove(facet);
            }
        }
    }
}
