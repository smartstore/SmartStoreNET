using System;
using System.Collections.Generic;

namespace SmartStore.Core.Html
{
	public interface IHtmlFilterProcessor
	{
		string ProcessFilters(string input, string flavor, IDictionary<string, object> parameters);
	}
}
