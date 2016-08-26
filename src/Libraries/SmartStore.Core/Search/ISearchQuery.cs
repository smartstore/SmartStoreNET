using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public interface ISearchQuery
	{
		ISearchQuery Parse(string[] fields, string query, bool escape = true);

		ISearchQuery AddClause(SearchFilter clause);

		ISearchQuery Slice(int skip, int take);

		IEnumerable<ISearchHit> Search();
		ISearchHit Get(int entityId);
		ISearchBits GetBits();
		int Count();
	}

	public static class ISearchQueryExtensions
	{
		public static ISearchQuery Parse(this ISearchQuery searchQuery, string field, string query, bool escape = true)
		{
			return searchQuery.Parse(new[] { field }, query, escape);
		}
	}
}
