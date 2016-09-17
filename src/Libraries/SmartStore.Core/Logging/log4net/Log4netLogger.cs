using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using log4net;
using log4net.Core;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Logging
{
	[DebuggerDisplay("Logger: {Name}")]
	public class Log4netLogger : ILogger
	{
		private static readonly Type _declaringType = typeof(Log4netLogger);

		private readonly log4net.Core.ILogger _logger;

		public Log4netLogger(log4net.Core.ILogger logger)
		{
			Guard.NotNull(logger, nameof(logger));

			_logger = logger;
		}

		public string Name
		{
			get { return _logger.Name;  }
		}

		public bool IsEnabledFor(LogLevel level)
		{
			return _logger.IsEnabledFor(ConvertLevel(level));
		}

		public void Log(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
		{
			var context = new LogContext
			{
				LogLevel = logLevel,
				ShortMessage = shortMessage,
				FullMessage = fullMessage,
				Customer = customer
			};

			Log(context);
		}

		public void Log(LogContext context)
		{
			var logLevel = ConvertLevel(context.LogLevel);

			object messageObj = context.ShortMessage;

			// TODO: refactor input

			//if (args != null && args.Length > 0)
			//{
			//	messageObj = new SystemStringFormat(CultureInfo.CurrentUICulture, message, args);
			//}

			TryAddExtendedThreadInfo();

			_logger.Log(_declaringType, logLevel, messageObj, null);
		}

		protected internal void TryAddExtendedThreadInfo()
		{
			// TODO: Put way more info into bag (IP, Referrer, Customer etc.)
			
			// Load the log4net thread with additional properties if they are available
			var threadInfoMissing = LogicalThreadContext.Properties["sm:ThreadInfoAdded"] == null;

			if (threadInfoMissing)
			{
				try
				{
					var request = HttpContext.Current?.Request;
					if (request != null)
					{
						LogicalThreadContext.Properties["Url"] = request.RawUrl;
						LogicalThreadContext.Properties["HttpMethod"] = request.HttpMethod;
					}

					var user = Thread.CurrentPrincipal.Identity;
					if (user != null)
					{
						LogicalThreadContext.Properties["User"] = user.Name.NullEmpty() ?? "n/a";
					}
				}
				catch
				{
					// can happen on cloud service for an unknown reason
				}
				finally
				{
					LogicalThreadContext.Properties["sm:ThreadInfoAdded"] = true;
				}
			}
		}

		public void Flush()
		{
		}

		private static Level ConvertLevel(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Information:
					return Level.Info;
				case LogLevel.Warning:
					return Level.Warn;
				case LogLevel.Error:
					return Level.Error;
				case LogLevel.Fatal:
					return Level.Fatal;
				default:
					return Level.Debug;
			}
		}
	}
}
