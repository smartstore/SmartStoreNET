using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public interface IIndexManager
	{
		bool HasAnyProvider();
		IIndexProvider GetProvider();
	}
}
