using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public interface ISearchEngine
	{
		ISearchQuery Query { get; }

		IEnumerable<ISearchHit> Search();
		ISearchHit Get(int id);
		ISearchBits GetBits();
		int Count();
	}
}
