using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Templating
{
	public interface ITemplateEngine
	{
		ITemplate Compile(string template);
		string Render(string template, object data, IFormatProvider formatProvider);
	}
}
