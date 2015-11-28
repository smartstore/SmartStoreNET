using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
    public static class LoggingExtensions
    {
        
		public static void Debug(this ILogger logger, string message, Exception exception = null, Customer customer = null)
        {
            FilteredLog(logger, LogLevel.Debug, message, exception, customer);
        }

        public static void Information(this ILogger logger, string message, Exception exception = null, Customer customer = null)
        {
            FilteredLog(logger, LogLevel.Information, message, exception, customer);
        }

        public static void Warning(this ILogger logger, string message, Exception exception = null, Customer customer = null)
        {
            FilteredLog(logger, LogLevel.Warning, message, exception, customer);
        }

        public static void Error(this ILogger logger, string message, Exception exception = null, Customer customer = null)
        {
            FilteredLog(logger, LogLevel.Error, message, exception, customer);
        }

        public static void Fatal(this ILogger logger, string message, Exception exception = null, Customer customer = null)
        {
            FilteredLog(logger, LogLevel.Fatal, message, exception, customer);
        }

		public static void Error(this ILogger logger, Exception exception, Customer customer = null)
		{
			FilteredLog(logger, LogLevel.Error, exception.Message, exception, customer);
		}

        private static void FilteredLog(ILogger logger, LogLevel level, string message, Exception exception = null, Customer customer = null)
        {
            // don't log thread abort exception
            if ((exception != null) && (exception is System.Threading.ThreadAbortException))
                return;

            if (logger.IsEnabled(level))
            {
                string fullMessage = exception == null ? string.Empty : exception.ToString();
                logger.InsertLog(level, message, fullMessage, customer);
            }
        }
    }
}
