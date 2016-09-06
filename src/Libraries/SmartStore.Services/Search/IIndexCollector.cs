using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public interface IIndexCollector
	{
		string Scope { get; }

		IIndexDataSegmenter Collect(
			DateTime? lastIndexedUtc, 
			Func<int, IIndexDocument> newDocument);
	}
}
