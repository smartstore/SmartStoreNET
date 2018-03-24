using System;

namespace SmartStore.Core.Search
{
	public class IndexingCompletedEvent
	{
		public IndexingCompletedEvent(IndexInfo indexInfo, bool wasRebuilt)
		{
			Guard.NotNull(indexInfo, nameof(indexInfo));

			IndexInfo = indexInfo;
			WasRebuilt = wasRebuilt;
		}

		public IndexInfo IndexInfo
		{
			get;
			private set;
		}

		public bool WasRebuilt
		{
			get;
			private set;
		}
	}
}