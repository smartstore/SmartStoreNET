using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Search
{
	public interface IIndexingService
	{
		IEnumerable<string> EnumerateScopes();
		void RebuildIndex(string scope);
		void UpdateIndex(string scope);
		void DeleteIndex(string scope);
		IndexInfo GetIndexInfo(string scope);
	}
}
