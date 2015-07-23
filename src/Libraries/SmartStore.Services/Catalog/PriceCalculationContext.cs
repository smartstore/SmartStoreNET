using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
	public class PriceCalculationContext
	{
		public LazyMultimap<ProductVariantAttribute> Attributes { get; set; }
		public LazyMultimap<ProductVariantAttributeCombination> AttributeCombinations { get; set; }
	}
}
