using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Search
{
	public class DefaultIndexingService : IIndexingService
	{
		public DefaultIndexingService()
		{

		}

		public IEnumerable<string> EnumerateScopes()
		{
			throw new NotImplementedException();
		}

		public void RebuildIndex(string scope)
		{
			throw new NotImplementedException();
		}


		public void UpdateIndex(string scope)
		{
			throw new NotImplementedException();
		}

		public void DeleteIndex(string scope)
		{
			throw new NotImplementedException();
		}

		public IndexInfo GetIndexInfo(string scope)
		{
			throw new NotImplementedException();
		}
	}
}
