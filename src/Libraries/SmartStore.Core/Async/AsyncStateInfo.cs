using System;

namespace SmartStore.Core.Async
{
	[Serializable]
	public class AsyncStateInfo
	{
		// used for serialization compatibility
		public static readonly string Version = "1";

		public object Progress
		{
			get;
			set;
		}

		public DateTime CreatedOnUtc
		{
			get;
			set;
		}

		public DateTime LastAccessUtc
		{
			get;
			set;
		}

		public TimeSpan Duration
		{
			get;
			set;
		}
	}
}
