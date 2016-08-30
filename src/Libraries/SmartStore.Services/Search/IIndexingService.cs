using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Search
{
	public interface IIndexingService
	{
		IEnumerable<string> EnumerateScopes();
		void RebuildIndex(string scope, TaskExecutionContext context);
		void UpdateIndex(string scope, TaskExecutionContext context);
		void DeleteIndex(string scope);
		IndexInfo GetIndexInfo(string scope);
	}
}
