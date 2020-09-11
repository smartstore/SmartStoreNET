using System;

namespace SmartStore.Core.Logging
{
    public partial class NullLogger : ILogger
    {
        private static readonly ILogger s_instance = new NullLogger();

        public static ILogger Instance => s_instance;

        public bool IsEnabledFor(LogLevel level)
        {
            return false;
        }

        public void Log(LogLevel level, Exception exception, string message, object[] args)
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

        public void FlushAll() { }
    }
}
