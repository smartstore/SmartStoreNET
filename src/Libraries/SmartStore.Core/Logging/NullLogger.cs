using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
    public partial class NullLogger : ILogger
    {
		private static readonly ILogger s_instance = new NullLogger();

		public static ILogger Instance
		{
			get
			{
				return s_instance;
			}
		}
		
        public bool IsEnabled(LogLevel level)
        {
            return false;
        }

		public void InsertLog(LogContext context)
		{
		}

        public void InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
        }

		public void Flush()
		{
		}
	}
}
