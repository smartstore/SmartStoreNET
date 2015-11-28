using System.Collections.Generic;
using System.Text;

namespace SmartStore.Services.Filter
{
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
	}


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
	}
}
