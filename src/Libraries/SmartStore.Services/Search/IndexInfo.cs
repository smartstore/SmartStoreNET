using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public class IndexInfo
	{
		public string Scope { get; set; }
		public int DocumentCount { get; set; }
		public DateTime LastUpdatedOnUtc { get; set; }
		public IEnumerable<string> Fields { get; set; }
		public IndexingStatus Status { get; set; }
	}
}
