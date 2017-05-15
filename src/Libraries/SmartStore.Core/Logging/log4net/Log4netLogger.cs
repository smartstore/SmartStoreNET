using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using log4net;
using log4net.Core;
using log4net.Util;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
	[DebuggerDisplay("Logger: {Name}")]
	public class Log4netLogger : ILogger
	{
		private static readonly Type _declaringType = typeof(Log4netLogger);
		private static readonly ContextState<Dictionary<string, object>> _state;

		private readonly log4net.Core.ILogger _logger;

		static Log4netLogger()
		{
			_state = new ContextState<Dictionary<string, object>>("Log4netLogger.LogicalThreadContext", () => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
		}

		public Log4netLogger(log4net.Core.ILogger logger)
		{
			Guard.NotNull(logger, nameof(logger));

			_logger = logger;
		}

		public string Name
		{
			get { return _logger.Name; }
		}

		public bool IsEnabledFor(LogLevel level)
		{
			return _logger.IsEnabledFor(ConvertLevel(level));
		}

		public void Log(LogLevel level, Exception exception, string message, object[] args)
		{
			var logLevel = ConvertLevel(level);

			if (message == null && exception != null)
			{
				message = exception.Message;
			}

			object messageObj = message;

			if (args != null && args.Length > 0)
			{
				messageObj = new SystemStringFormat(CultureInfo.CurrentUICulture, message, args);
			}

			var loggingEvent = new LoggingEvent(
				_declaringType,
				_logger.Repository,
				_logger.Name,
				logLevel,
				messageObj,
				exception);

			loggingEvent.Properties["LevelId"] = (int)level;
			TryAddExtendedThreadInfo(loggingEvent);

			_logger.Log(loggingEvent);
		}

		protected internal void TryAddExtendedThreadInfo(LoggingEvent loggingEvent)
		{
			HttpRequest httpRequest = null;

			try
			{
				httpRequest = HttpContext.Current.Request;
			}
			catch
			{
				loggingEvent.Properties["CustomerId"] = DBNull.Value;
				loggingEvent.Properties["Url"] = DBNull.Value;
				loggingEvent.Properties["Referrer"] = DBNull.Value;
				loggingEvent.Properties["HttpMethod"] = DBNull.Value;
				loggingEvent.Properties["Ip"] = DBNull.Value;

				return;
			}

			var props = _state.GetState();

			// Load the log4net thread with additional properties if they are available
			var threadInfoMissing = !props.ContainsKey("sm:ThreadInfoAdded");

			if (threadInfoMissing)
			{
				using (new ActionDisposable(() => props["sm:ThreadInfoAdded"] = true))
				{
					if (DataSettings.DatabaseIsInstalled() && EngineContext.Current.IsFullyInitialized)
					{
						var container = EngineContext.Current.ContainerManager;

						IWorkContext workContext;

						// CustomerId
						if (container.TryResolve<IWorkContext>(null, out workContext))
						{
							try
							{
								props["CustomerId"] = workContext.CurrentCustomer.Id;
							}
							catch
							{
								props["CustomerId"] = DBNull.Value;
							}
						}

						IWebHelper webHelper;

						// Url & stuff
						if (container.TryResolve<IWebHelper>(null, out webHelper))
						{
							try
							{
								props["Url"] = webHelper.GetThisPageUrl(true);
								props["Referrer"] = webHelper.GetUrlReferrer();
								props["HttpMethod"] = httpRequest?.HttpMethod;
								props["Ip"] = webHelper.GetCurrentIpAddress();
							}
							catch { }
						}
					}
				}
			}

			loggingEvent.Properties["CustomerId"] = props.Get("CustomerId") ?? DBNull.Value;
			loggingEvent.Properties["Url"] = props.Get("Url") ?? DBNull.Value;
			loggingEvent.Properties["Referrer"] = props.Get("Referrer") ?? DBNull.Value;
			loggingEvent.Properties["HttpMethod"] = props.Get("HttpMethod") ?? DBNull.Value;
			loggingEvent.Properties["Ip"] = props.Get("Ip") ?? DBNull.Value;
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
