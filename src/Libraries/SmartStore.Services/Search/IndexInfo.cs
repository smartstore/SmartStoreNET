using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public enum IndexingStatus
	{
		Unavailable = -1,
		Idle = 0,
		Rebuilding = 1,
		Updating = 2
	}

	public class IndexInfo
	{
		public string Scope { get; set; }
		public int DocumentCount { get; set; }
		public IEnumerable<string> Fields { get; set; }

		// loaded from status file
		public DateTime? LastIndexedUtc { get; set; }
		public IndexingStatus Status { get; set; }
	}
}
