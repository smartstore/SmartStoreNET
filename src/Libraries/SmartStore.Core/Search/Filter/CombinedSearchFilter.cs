using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Search
{
	public class CombinedSearchFilter : SearchFilterBase, ICombinedSearchFilter
	{
		private readonly IList<ISearchFilter> _filters;

		public CombinedSearchFilter()
		{
			_filters = new List<ISearchFilter>();
		}

		public CombinedSearchFilter(IEnumerable<ISearchFilter> filters)
		{
			Guard.NotNull(filters, nameof(filters));

			_filters = new List<ISearchFilter>(filters);
		}

		public ICollection<ISearchFilter> Filters
		{
			get
			{
				return _filters;
			}
		}

		#region Fluent builder

		public CombinedSearchFilter Add(ISearchFilter filter)
		{
			Guard.NotNull(filter, nameof(filter));

			_filters.Add(filter);

			return this;
		}

		#endregion

		public override string ToString()
		{
			if (_filters.Any())
			{
				return string.Concat("(", string.Join(", ", _filters.Select(x => x.ToString())), ")");
			}

			return "";
		}
	}
}
