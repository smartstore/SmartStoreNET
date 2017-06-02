using System;
using System.Collections.Generic;

namespace SmartStore.Core.Html
{
	public interface IHtmlFilter : IOrdered
	{
		string Process(string input, IDictionary<string, object> parameters);
		string Flavor { get; }
	}
}
