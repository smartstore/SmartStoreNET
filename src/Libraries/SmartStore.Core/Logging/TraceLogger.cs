using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Customers;
using System.Diagnostics;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
	public class TraceLogger : DisposableObject, ILogger
	{
		private readonly TraceSource _traceSource;

		public TraceLogger() : this(CommonHelper.MapPath("~/App_Data/SmartStore.log"))
		{
		}

		public TraceLogger(string fileName)
		{
			Guard.ArgumentNotEmpty(() => fileName);

			_traceSource = new TraceSource("SmartStore");
			_traceSource.Switch = new SourceSwitch("LogSwitch", "Error");
			_traceSource.Listeners.Remove("Default");
			
			var console = new ConsoleTraceListener(false);
			console.Filter = new EventTypeFilter(SourceLevels.All);
			console.Name = "console";

			var textListener = new TextWriterTraceListener(fileName);
			textListener.Filter = new EventTypeFilter(SourceLevels.All);
			textListener.TraceOutputOptions = TraceOptions.DateTime;

			_traceSource.Listeners.Add(console);
			_traceSource.Listeners.Add(textListener);
			
			// Allow the trace source to send messages to  
			// listeners for all event types. Currently only  
			// error messages or higher go to the listeners. 
			// Messages must get past the source switch to  
			// get to the listeners, regardless of the settings  
			// for the listeners.
			_traceSource.Switch.Level = SourceLevels.All;
		}

		public bool IsEnabled(LogLevel level)
		{
			return true;
		}

		public void DeleteLog(Log log)
		{
			// not supported
		}

		public void ClearLog()
		{
			// not supported
		}

		public void ClearLog(DateTime toUtc, LogLevel logLevel)
		{
			// not supported
		}

		public IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc, string message, LogLevel? logLevel, int pageIndex, int pageSize, int minFrequency)
		{
			// not supported
			return null;
		}

		public Log GetLogById(int logId)
		{
			// not supported
			return null;
		}

		public IList<Log> GetLogByIds(int[] logIds)
		{
			// not supported
			return null;
		}

		public void InsertLog(LogContext context)
		{
			var type = LogLevelToEventType(context.LogLevel);
			var msg = context.ShortMessage;
			if (context.FullMessage.HasValue())
			{
				msg += "{0}{1}".FormatCurrent(Environment.NewLine, context.FullMessage);
			}
			_traceSource.TraceEvent(type, (int)type, "{0}: {1}".FormatCurrent(type.ToString().ToUpper(), msg));
		}

		public void InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
		{
			var context = new LogContext
			{
				LogLevel = logLevel,
				ShortMessage = shortMessage,
				FullMessage = fullMessage,
				Customer = customer
			};

			InsertLog(context);
		}

		private TraceEventType LogLevelToEventType(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug:
					return TraceEventType.Verbose;
				case LogLevel.Error:
					return TraceEventType.Error;
				case LogLevel.Fatal:
					return TraceEventType.Critical;
				case LogLevel.Warning:
					return TraceEventType.Warning;
				default:
					return TraceEventType.Information;
			}
		}

		public void Flush()
		{
			_traceSource.Flush();
		}

		protected override void OnDispose(bool disposing)
		{
			_traceSource.Flush();
			_traceSource.Close();
		}
	}
}
