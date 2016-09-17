using System;
using System.IO;
using log4net;
using log4net.Config;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
	public class Log4netLoggerFactory : ILoggerFactory
	{
		static Log4netLoggerFactory()
		{
			var configFile = GetConfigFile(CommonHelper.GetAppSetting<string>("log4net.Config", @"Config\log4net.config"));
			XmlConfigurator.ConfigureAndWatch(configFile);
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

		public ILogger CreateLogger(Type type)
		{
			return CreateLogger(type.FullName);
		}

		public ILogger CreateLogger(string name)
		{
			var log = LogManager.GetLogger(name);

			//// TODO!!!!
			//var adoNetAppenders = log.Logger.Repository.GetAppenders().OfType<AdoNetAppender>();
			//foreach (var appender in adoNetAppenders)
			//{
			//	appender.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			//	appender.ActivateOptions();
			//}

			return new Log4netLogger(log.Logger);
		}
	}
}
