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

			return source.OrderBy(descriptor.OrderBy, descriptor.Key);
		}

		/// <summary>
		/// Orders a facet sequence
		/// </summary>
		/// <param name="source">Facets</param>
		/// <param name="sorting">Type of sorting</param>
		/// <returns>Ordered facets</returns>
		public static IOrderedEnumerable<Facet> OrderBy(this IEnumerable<Facet> source, FacetSorting sorting, string key = null)
		{
			Guard.NotNull(source, nameof(source));

			switch (sorting)
			{
				case FacetSorting.ValueAsc:
					return source.OrderByDescending(x => x.Value.IsSelected).ThenBy(x => x.Label);
				case FacetSorting.DisplayOrder:
					if (key.IsCaseInsensitiveEqual("price"))
						return source.OrderBy(x => x.DisplayOrder);

					return source.OrderByDescending(x => x.Value.IsSelected).ThenBy(x => x.DisplayOrder);
				default:
					return source.OrderByDescending(x => x.Value.IsSelected).ThenByDescending(x => x.HitCount);
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

		/// <summary>
		/// Get the facet value as a string
		/// </summary>
		/// <param name="value">Facet value</param>
		/// <returns>Facet value string</returns>
		public static string GetStringValue(this FacetValue value)
		{
			Guard.NotNull(value, nameof(value));

			var result = string.Empty;
			var valueString = value.Value == null ? string.Empty : value.Value.ToString().EmptyNull();

			if (value.IsRange)
			{
				var upperValueString = value.UpperValue == null ? string.Empty : value.UpperValue.ToString().EmptyNull();

				if (value.IncludesLower && value.IncludesUpper)
				{
					result = $"[{valueString} - {upperValueString}]";
				}
				else if (value.IncludesLower)
				{
					result = valueString;
				}
				else if (value.IncludesUpper)
				{
					result = upperValueString;
				}
			}
			else
			{
				result = valueString;
			}

			return result;
		}
	}
}
