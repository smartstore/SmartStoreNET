using System;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
	public class PerformanceSettings : ISettings
	{
		/// <summary>
		/// The number of entries in a single cache segment
		/// when greedy loading is disabled.
		/// </summary>
		/// <remarks>
		/// The cache has to be cleared after changing this setting.
		/// </remarks>
		public int CacheSegmentSize { get; set; } = 500;
	}
}
