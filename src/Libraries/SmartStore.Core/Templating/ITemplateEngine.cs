using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;

namespace SmartStore.Templating
{
	public interface ITemplateEngine
	{
		ITemplate Compile(string template);
		string Render(string source, object data, IFormatProvider formatProvider);
		object CreateTestModelFor(BaseEntity entity, string modelPrefix);
	}
}
