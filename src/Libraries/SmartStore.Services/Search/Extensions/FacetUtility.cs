using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Extensions
{
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
	}
}
