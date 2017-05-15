using System;
using NuGet;
using SmartStore.Core.Logging;
using Log = SmartStore.Core.Logging;

namespace SmartStore.Core.Packaging
{
	internal class NugetLogger : NuGet.ILogger
	{
		private readonly Log.ILogger _logger;

		public NugetLogger(Log.ILogger logger)
		{
			_logger = logger;
		}

		public void Log(MessageLevel level, string message, params object[] args)
		{
			switch (level)
			{
				case MessageLevel.Debug:
					_logger.Debug(String.Format(message, args));
					break;
				case MessageLevel.Error:
					_logger.Error(String.Format(message, args));
					break;
				case MessageLevel.Info:
					_logger.Info(String.Format(message, args));
					break;
				case MessageLevel.Warning:
					_logger.Warn(String.Format(message, args));
					break;
			}
		}

		public FileConflictResolution ResolveFileConflict(string message)
		{
			return FileConflictResolution.OverwriteAll;
		}
	}
}
