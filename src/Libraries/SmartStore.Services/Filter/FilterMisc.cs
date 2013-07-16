using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Services.Filter;

namespace SmartStore.Services.Filter
{
	/// <remarks>codehint: sm-add</remarks>
	public class FilterSql
	{
		public List<FilterCriteria> Criteria { get; set; }
		public List<object> Values { get; set; }
		public StringBuilder WhereClause { get; set; }

		public override string ToString()
		{
			if (WhereClause != null && Values != null)
				return "{0}, {1}".FormatWith(WhereClause.ToString(), string.Join(", ", Values.ToArray()));
			return "";
		}
	}	// class


	public class FilterProductContext
	{
		public string Filter { get; set; }
		public int ParentCategoryID { get; set; }
		public List<int> CategoryIds { get; set; }
		public string Path { get; set; }
		public int PageSize { get; set; }
		public int? OrderBy { get; set; }
		public string ViewMode { get; set; }

		public List<FilterCriteria> Criteria { get; set; }
	}	// class

}
