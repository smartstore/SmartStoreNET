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
		
        public bool IsEnabledFor(LogLevel level)
        {
            return false;
        }

        public void Log(LogLevel logLevel, string shortMessage, string fullMessage = "")
        {
        }

		public void Flush()
		{
		}
	}

	public class NullLoggerFactory : ILoggerFactory
	{
		public ILogger GetLogger(Type type)
		{
			return NullLogger.Instance;
		}

		public ILogger GetLogger(string name)
		{
			return NullLogger.Instance;
		}
	}
}
