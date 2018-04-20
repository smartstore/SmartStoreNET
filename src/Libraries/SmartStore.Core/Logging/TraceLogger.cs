using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
	public class TraceLogger : DisposableObject, ILogger
	{
		private readonly TraceSource _traceSource;
		private readonly StreamWriter _streamWriter;

		public TraceLogger() : this(CommonHelper.MapPath("~/App_Data/SmartStore.log"))
		{
		}

		public TraceLogger(string fileName)
		{
			Guard.NotEmpty(fileName, nameof(fileName));

			_traceSource = new TraceSource("SmartStore");
			_traceSource.Switch = new SourceSwitch("LogSwitch", "Error");
			_traceSource.Listeners.Remove("Default");

			var console = new ConsoleTraceListener(false);
			console.Filter = new EventTypeFilter(SourceLevels.All);
			console.Name = "console";

			_traceSource.Listeners.Add(console);

			var textListener = new TextWriterTraceListener(fileName);
			textListener.Filter = new EventTypeFilter(SourceLevels.All);
			textListener.TraceOutputOptions = TraceOptions.DateTime;

			try
			{
				// force UTF-8 encoding (even if the text just contains ANSI characters)
				var append = File.Exists(fileName);
				_streamWriter = new StreamWriter(fileName, append, Encoding.UTF8);

				textListener.Writer = _streamWriter;

				_traceSource.Listeners.Add(textListener);
			}
			catch (IOException)
			{
				// file is locked by another process
			}

			// Allow the trace source to send messages to
			// listeners for all event types. Currently only
			// error messages or higher go to the listeners. 
			// Messages must get past the source switch to
			// get to the listeners, regardless of the settings
			// for the listeners.
			_traceSource.Switch.Level = SourceLevels.All;
		}

		public bool IsEnabledFor(LogLevel level)
		{
			return true;
		}

		public void Log(LogLevel level, Exception exception, string message, object[] args)
		{
			var type = LogLevelToEventType(level);

			if (exception != null && !exception.IsFatal())
			{
				message = message.Grow(exception.ToString(), Environment.NewLine);
			}

			if (message.HasValue())
			{
				var msg = args != null && args.Any()
					? message.FormatInvariant(args)
					: message;

				_traceSource.TraceEvent(type, (int)type, "{0}: {1}".FormatCurrent(type.ToString().ToUpper(), msg));
			}
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

			if (_streamWriter != null)
			{
				_streamWriter.Close();
				_streamWriter.Dispose();
			}
		}
	}
}
