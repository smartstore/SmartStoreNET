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
		/// <param name="sorting">Type of sorting</param>
		/// <returns>Ordered facets</returns>
		public static IOrderedEnumerable<Facet> OrderBy(this IEnumerable<Facet> source, FacetSorting sorting)
		{
			Guard.NotNull(source, nameof(source));

			switch (sorting)
			{
				case FacetSorting.ValueAsc:
					return source.OrderBy(x => x.Label);
				case FacetSorting.DisplayOrder:
					return source.OrderBy(x => x.DisplayOrder);
				default:
					return source.OrderByDescending(x => x.HitCount);
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
