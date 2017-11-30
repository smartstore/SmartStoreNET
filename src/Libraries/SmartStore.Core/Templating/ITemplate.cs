using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Templating
{
	public interface ITemplate
	{
		DateTime TimeStamp { get; }
		string Render(object data, IFormatProvider formatProvider);
	}
}
