using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
    public static class LoggingExtensions
    {     
		public static void Debug(this ILogger logger, string message, Exception exception = null)
        {
            FilteredLog(logger, LogLevel.Debug, message, null, exception);
        }

        public static void Information(this ILogger logger, string message, Exception exception = null)
        {
            FilteredLog(logger, LogLevel.Information, message, null, exception);
        }

        public static void Warning(this ILogger logger, string message, Exception exception = null)
        {
            FilteredLog(logger, LogLevel.Warning, message, null, exception);
        }

        public static void Error(this ILogger logger, string message, Exception exception = null)
        {
            FilteredLog(logger, LogLevel.Error, message, null, exception);
        }

		public static void Error(this ILogger logger, string message, string fullMessage, Exception exception = null)
		{
			FilteredLog(logger, LogLevel.Error, message, fullMessage, exception);
		}

		public static void Fatal(this ILogger logger, string message, Exception exception = null)
        {
            FilteredLog(logger, LogLevel.Fatal, message, null, exception);
        }

		public static void Error(this ILogger logger, Exception exception)
		{
			FilteredLog(logger, LogLevel.Error, exception.ToAllMessages(), null, exception);
		}

		public static void ErrorsAll(this ILogger logger, Exception exception)
		{
			while (exception != null)
			{
				FilteredLog(logger, LogLevel.Error, exception.Message, exception.StackTrace, exception);
				exception = exception.InnerException;
			}
		}

		private static void FilteredLog(ILogger logger, LogLevel level, string message, string fullMessage, Exception exception = null)
        {
            // don't log thread abort exception
            if ((exception != null) && (exception is System.Threading.ThreadAbortException))
                return;

            if (logger.IsEnabledFor(level))
            {
				if (exception != null && fullMessage.IsEmpty())
				{
					fullMessage = "{0}\n{1}\n{2}".FormatCurrent(exception.Message, new String('-', 20), exception.StackTrace);
				}

                logger.Log(level, message, fullMessage);
            }
        }
    }
}
