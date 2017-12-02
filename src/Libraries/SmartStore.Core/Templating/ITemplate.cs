using System;

namespace SmartStore.Templating
{
	public interface ITemplate
	{
		string Source { get; }
		string Render(object data, IFormatProvider formatProvider);
	}
}
