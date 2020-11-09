using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
    public class TraceLogger : DisposableObject, ILogger
    {
        private readonly TraceSource _traceSource;
        private readonly StreamWriter _streamWriter;

        public TraceLogger() : this(GetDefaultFileName())
        {
        }

        private static string GetDefaultFileName()
        {
            var di = new DirectoryInfo(CommonHelper.MapPath("~/App_Data/Logs"));
            if (!di.Exists)
            {
                di.Create();
            }

            var datePart = DateTime.Now.ToString("yyyy-MM");
            var fileName = "SmartStore-{0}.log".FormatInvariant(datePart);

            return Path.Combine(di.FullName, fileName);
        }

        public TraceLogger(string fileName)
        {
            Guard.NotEmpty(fileName, nameof(fileName));

            _traceSource = new TraceSource("SmartStore");
            _traceSource.Switch = new SourceSwitch("LogSwitch", "Error");
            _traceSource.Listeners.Remove("Default");

            if (CommonHelper.IsDevEnvironment)
            {
                var defaultListener = new DefaultTraceListener()
                {
                    Name = "Debugger",
                    Filter = new EventTypeFilter(SourceLevels.All),
                    TraceOutputOptions = TraceOptions.DateTime
                };
                _traceSource.Listeners.Add(defaultListener);
            }

            var textListener = new TextWriterTraceListener(fileName)
            {
                Name = "File",
                Filter = new EventTypeFilter(SourceLevels.All),
                TraceOutputOptions = TraceOptions.DateTime
            };

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
            return _traceSource.Switch.ShouldTrace(LogLevelToEventType(level));
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
                    ? message.FormatCurrent(args)
                    : message;

                _traceSource.TraceEvent(type, (int)type, msg);
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

        //private LogLevel EventTypeToLogLevel(TraceEventType eventType)
        //{
        //    switch (eventType)
        //    {
        //        case TraceEventType.Verbose:
        //            return LogLevel.Debug;
        //        case TraceEventType.Error:
        //            return LogLevel.Error;
        //        case TraceEventType.Critical:
        //            return LogLevel.Fatal;
        //        case TraceEventType.Warning:
        //            return LogLevel.Warning;
        //        default:
        //            return LogLevel.Information;
        //    }
        //}

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
