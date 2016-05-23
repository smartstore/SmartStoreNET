using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Indexing
{
	public interface ISearchEngine
	{
		ISearchEngine ParseQuery(string defaultField, string query, bool escape = true);
		ISearchEngine ParseQuery(string[] defaultFields, string query, bool escape = true);

		ISearchEngine WithField(string field, bool value);
		ISearchEngine WithField(string field, DateTime value);
		ISearchEngine WithField(string field, string value);
		ISearchEngine WithField(string field, int value);
		ISearchEngine WithField(string field, double value);
		ISearchEngine WithinRange(string field, int? min, int? max, bool includeMin = true, bool includeMax = true);
		ISearchEngine WithinRange(string field, double? min, double? max, bool includeMin = true, bool includeMax = true);
		ISearchEngine WithinRange(string field, DateTime? min, DateTime? max, bool includeMin = true, bool includeMax = true);
		ISearchEngine WithinRange(string field, string min, string max, bool includeMin = true, bool includeMax = true);

		/// <summary>
		/// Mark a clause as a mandatory match. By default all clauses are optional.
		/// </summary>
		ISearchEngine Mandatory();

		/// <summary>
		/// Mark a clause as a forbidden match.
		/// </summary>
		ISearchEngine Forbidden();

		/// <summary>
		/// Applied on string clauses, the searched value will not be tokenized.
		/// </summary>
		ISearchEngine NotAnalyzed();

		/// <summary>
		/// Applied on string clauses, it removes the default Prefix mecanism. Like 'broadcast' won't
		/// return 'broadcasting'.
		/// </summary>
		ISearchEngine ExactMatch();

		/// <summary>
		/// Apply a specific boost to a clause.
		/// </summary>
		/// <param name="weight">A value greater than zero, by which the score will be multiplied. 
		/// If greater than 1, it will improve the weight of a clause</param>
		ISearchEngine Weighted(float weight);

		/// <summary>
		/// Defines a clause as a filter, so that it only affect the results of the other clauses.
		/// For instance, if the other clauses returns nothing, even if this filter has matches the
		/// end result will be empty. It's like a two-pass query
		/// </summary>
		ISearchEngine AsFilter();

		ISearchEngine SortBy(string name);
		ISearchEngine SortByInteger(string name);
		ISearchEngine SortByBoolean(string name);
		ISearchEngine SortByString(string name);
		ISearchEngine SortByDouble(string name);
		ISearchEngine SortByDateTime(string name);
		ISearchEngine Ascending();

		ISearchEngine Slice(int skip, int count);
		//IEnumerable<ISearchHit> Search();
		//ISearchHit Get(int documentId);
		//ISearchBits GetBits();
		int Count();
	}
}
