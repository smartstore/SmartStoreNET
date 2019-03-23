using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.UI
{
	public interface IMenuResolver
	{
		bool Exists(string menuName);
		IMenu Resolve(string menuName);
	}
}
