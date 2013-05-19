using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Filter
{
	/// <remarks>codehint: sm-add</remarks>
	public partial interface IFilterService
	{
		bool IncludeFeatured { get; }

		List<FilterCriteria> Deserialize(string jsonData);
		string Serialize(List<FilterCriteria> criteria);

		bool ToWhereClause(FilterSql context);
		bool ToWhereClause(FilterSql context, List<FilterCriteria> findIn, Predicate<FilterCriteria> match);

		IQueryable<Product> ProductFilter(FilterProductContext context);

		void ProductFilterable(FilterProductContext context);
		void ProductFilterableMultiSelect(FilterProductContext context, string filterMultiSelect);
	}
}
