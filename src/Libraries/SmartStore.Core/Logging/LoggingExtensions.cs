using System;

namespace SmartStore.Core.Logging
{
    public static class LoggingExtensions
    {
		public static bool IsDebugEnabled(this ILogger l)												=> l.IsEnabledFor(LogLevel.Debug);
		public static bool IsInfoEnabled(this ILogger l)												=> l.IsEnabledFor(LogLevel.Information);
		public static bool IsWarnEnabled(this ILogger l)												=> l.IsEnabledFor(LogLevel.Warning);
		public static bool IsErrorEnabled(this ILogger l)												=> l.IsEnabledFor(LogLevel.Error);
		public static bool IsFatalEnabled(this ILogger l)												=> l.IsEnabledFor(LogLevel.Fatal);



		public static void Debug(this ILogger l, string msg)											=> FilteredLog(l, LogLevel.Debug, null, msg, null);
		public static void Debug(this ILogger l, Func<string> msgFactory)								=> FilteredLog(l, LogLevel.Debug, null, msgFactory);
		public static void Debug(this ILogger l, Exception ex, string msg)								=> FilteredLog(l, LogLevel.Debug, ex, msg, null);
		public static void DebugFormat(this ILogger l, string msg, params object[] args)				=> FilteredLog(l, LogLevel.Debug, null, msg, args);
		public static void DebugFormat(this ILogger l, Exception ex, string msg, params object[] args)	=> FilteredLog(l, LogLevel.Debug, ex, msg, args);

		public static void Info(this ILogger l, string msg)												=> FilteredLog(l, LogLevel.Information, null, msg, null);
		public static void Info(this ILogger l, Func<string> msgFactory)								=> FilteredLog(l, LogLevel.Information, null, msgFactory);
		public static void Info(this ILogger l, Exception ex, string msg)								=> FilteredLog(l, LogLevel.Information, ex, msg, null);
		public static void InfoFormat(this ILogger l, string msg, params object[] args)					=> FilteredLog(l, LogLevel.Information, null, msg, args);
		public static void InfoFormat(this ILogger l, Exception ex, string msg, params object[] args)	=> FilteredLog(l, LogLevel.Information, ex, msg, args);

		public static void Warn(this ILogger l, string msg)												=> FilteredLog(l, LogLevel.Warning, null, msg, null);
		public static void Warn(this ILogger l, Func<string> msgFactory)								=> FilteredLog(l, LogLevel.Warning, null, msgFactory);
		public static void Warn(this ILogger l, Exception ex, string msg)								=> FilteredLog(l, LogLevel.Warning, ex, msg, null);
		public static void WarnFormat(this ILogger l, string msg, params object[] args)					=> FilteredLog(l, LogLevel.Warning, null, msg, args);
		public static void WarnFormat(this ILogger l, Exception ex, string msg, params object[] args)	=> FilteredLog(l, LogLevel.Warning, ex, msg, args);

		public static void Error(this ILogger l, string msg)											=> FilteredLog(l, LogLevel.Error, null, msg, null);
		public static void Error(this ILogger l, Func<string> msgFactory)								=> FilteredLog(l, LogLevel.Error, null, msgFactory);
		public static void Error(this ILogger l, Exception ex)											=> FilteredLog(l, LogLevel.Error, ex, null, null);
		public static void Error(this ILogger l, Exception ex, string msg)								=> FilteredLog(l, LogLevel.Error, ex, msg, null);
		public static void ErrorFormat(this ILogger l, string msg, params object[] args)				=> FilteredLog(l, LogLevel.Error, null, msg, args);
		public static void ErrorFormat(this ILogger l, Exception ex, string msg, params object[] args)	=> FilteredLog(l, LogLevel.Error, ex, msg, args);

		public static void Fatal(this ILogger l, string msg)											=> FilteredLog(l, LogLevel.Fatal, null, msg, null);
		public static void Fatal(this ILogger l, Func<string> msgFactory)								=> FilteredLog(l, LogLevel.Fatal, null, msgFactory);
		public static void Fatal(this ILogger l, Exception ex)											=> FilteredLog(l, LogLevel.Fatal, ex, null, null);
		public static void Fatal(this ILogger l, Exception ex, string msg)								=> FilteredLog(l, LogLevel.Fatal, ex, msg, null);
		public static void FatalFormat(this ILogger l, string msg, params object[] args)				=> FilteredLog(l, LogLevel.Fatal, null, msg, args);
		public static void FatalFormat(this ILogger l, Exception ex, string msg, params object[] args)	=> FilteredLog(l, LogLevel.Fatal, ex, msg, args);

		public static void ErrorsAll(this ILogger logger, Exception exception)
		{
			while (exception != null)
			{
				FilteredLog(logger, LogLevel.Error, exception, exception.Message, null);
				exception = exception.InnerException;
			}
		}

		private static void FilteredLog(ILogger logger, LogLevel level, Exception exception, string message, object[] objects)
		{
			if (logger.IsEnabledFor(level))
			{
				logger.Log(level, exception, message, objects);
			}
		}

		private static void FilteredLog(ILogger logger, LogLevel level, Exception exception, Func<string> messageFactory)
		{
			if (logger.IsEnabledFor(level))
			{
				logger.Log(level, exception, messageFactory(), null);
			}
		}
	}
}
