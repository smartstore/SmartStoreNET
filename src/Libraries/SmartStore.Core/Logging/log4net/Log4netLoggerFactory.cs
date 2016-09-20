using System;
using System.Data;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Util;
using SmartStore.Core.Data;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
	public class Log4netLoggerFactory : ILoggerFactory
	{
		public Log4netLoggerFactory()
		{
			var configFile = GetConfigFile(CommonHelper.GetAppSetting<string>("log4net.Config", @"Config\log4net.config"));

			XmlConfigurator.ConfigureAndWatch(configFile);

			var repository = LogManager.GetRepository();
			repository.ConfigurationChanged += (sender, e) => TryConfigureDbAppender(sender as ILoggerRepository);
			TryConfigureDbAppender(repository);
		}

		private void TryConfigureDbAppender(ILoggerRepository repository)
		{
			if (repository == null || !DataSettings.DatabaseIsInstalled())
				return;

			var adoNetAppenders = repository.GetAppenders().OfType<AdoNetAppender>().Where(x => x.Name == "db").ToList();
			foreach (var appender in adoNetAppenders)
			{
				appender.ConnectionString = DataSettings.Current.DataConnectionString;
				appender.ConnectionType = DataSettings.Current.DataConnectionType;
				appender.ActivateOptions();
			}
		}

		private static FileInfo GetConfigFile(string fileName)
		{
			FileInfo result;

			if (Path.IsPathRooted(fileName))
			{
				result = new FileInfo(fileName);
			}
			else
			{
				string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				result = new FileInfo(Path.Combine(baseDirectory, fileName));
			}

			return result;
		}

		public ILogger GetLogger(Type type)
		{
			return GetLogger(type.FullName);
		}

		public ILogger GetLogger(string name)
		{
			var log = LogManager.GetLogger(name);
			return new Log4netLogger(log.Logger);
		}
	}
}
