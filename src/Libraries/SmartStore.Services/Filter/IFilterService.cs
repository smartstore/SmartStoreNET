using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Filter
{
	public partial interface IFilterService
	{
		List<FilterCriteria> Deserialize(string jsonData);
		string Serialize(List<FilterCriteria> criteria);

		FilterProductContext CreateFilterProductContext(string filter, int categoryID, string path, int? pagesize, int? orderby, string viewmode);

		bool ToWhereClause(FilterSql context);
		bool ToWhereClause(FilterSql context, List<FilterCriteria> findIn, Predicate<FilterCriteria> match);

		IQueryable<Product> ProductFilter(FilterProductContext context);

		void ProductFilterable(FilterProductContext context);
		void ProductFilterableMultiSelect(FilterProductContext context, string filterMultiSelect);
	}
}
