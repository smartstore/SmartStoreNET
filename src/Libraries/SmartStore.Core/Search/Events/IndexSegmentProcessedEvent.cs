using System;
using System.Collections.Generic;

namespace SmartStore.Core.Search
{
	public class IndexSegmentProcessedEvent
	{
		public IndexSegmentProcessedEvent(string scope, IEnumerable<IIndexOperation> documents, bool isRebuild)
		{
			Guard.NotEmpty(scope, nameof(scope));
			Guard.NotNull(documents, nameof(documents));

			Scope = scope;
			IsRebuild = isRebuild;
			Documents = documents;
			Metadata = new Dictionary<string, object>();
		}

		public string Scope
		{
			get;
			private set;
		}

		public bool IsRebuild
		{
			get;
			private set;
		}

		public IEnumerable<IIndexOperation> Documents
		{
			get;
			private set;
		}

		public IDictionary<string, object> Metadata
		{
			get;
			private set;
		}
	}
}
