using System.Collections.Generic;

namespace SmartStore.Core
{
	public interface IMergedData
	{
		bool MergedDataIgnore { get; set; }
		Dictionary<string, object> MergedDataValues { get; }
	}
}
