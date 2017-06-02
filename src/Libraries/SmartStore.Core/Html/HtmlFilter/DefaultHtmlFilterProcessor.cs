using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Html
{
	public partial class DefaultHtmlFilterProcessor : IHtmlFilterProcessor
	{
		private readonly IEnumerable<IHtmlFilter> _filters;

		public DefaultHtmlFilterProcessor(IEnumerable<IHtmlFilter> filters)
		{
			_filters = filters;
		}

		public string ProcessFilters(string input, string flavor, IDictionary<string, object> parameters)
		{
			Guard.NotEmpty(flavor, nameof(flavor));

			parameters = parameters ?? new Dictionary<string, object>();

			var filters = _filters
				.Where(x => x.Flavor.IsEmpty() || x.Flavor.IsCaseInsensitiveEqual(flavor))
				.OrderBy(x => x.Ordinal);

			var result = input;

			foreach (var filter in filters)
			{
				result = filter.Process(input, parameters);
			}

			return result;
		}
	}
}
