using System;
using System.Collections.Generic;

namespace SmartStore.Data.Caching2
{
	public class CachedDbResult
	{
		public ColumnMetadata[] TableMetadata { get; set; }
		public List<object[]> Records { get; set; }
		public int RecordsAffected { get; set; }
	}
}
