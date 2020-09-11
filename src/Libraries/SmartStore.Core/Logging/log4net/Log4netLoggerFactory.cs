using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;
using SmartStore.Core.Data;
using SmartStore.Utilities;

namespace SmartStore.Core.Logging
{
    public class Log4netLoggerFactory : ILoggerFactory, IRegisteredObject
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggerCache = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);

        public Log4netLoggerFactory()
        {
            if (HostingEnvironment.IsHosted)
            {
                var configFile = GetConfigFile(CommonHelper.GetAppSetting<string>("log4net.Config", @"Config\log4net.config"));

                XmlConfigurator.ConfigureAndWatch(configFile);

                var repository = LogManager.GetRepository();
                repository.ConfigurationChanged += OnConfigurationChanged;
                TryConfigureDbAppender(repository);

                HostingEnvironment.RegisterObject(this);
            }
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            _loggerCache.Clear();
            TryConfigureDbAppender(sender as ILoggerRepository);
        }

        private void TryConfigureDbAppender(ILoggerRepository repository)
        {
            if (repository == null || !DataSettings.DatabaseIsInstalled())
                return;

            var adoNetAppenders = repository.GetAppenders().OfType<AdoNetAppender>().Where(x => x.Name == "database").ToList();
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
            Guard.NotNull(type, nameof(type));

            return GetLogger(type.FullName);
        }

        public ILogger GetLogger(string name)
        {
            Guard.NotEmpty(name, nameof(name));

            var logger = _loggerCache.GetOrAdd(name, key => new Log4netLogger(LogManager.GetLogger(name).Logger));
            return logger;
        }

        public void FlushAll()
        {
            var bufferingAppenders = LogManager.GetRepository().GetAppenders().OfType<BufferingAppenderSkeleton>();
            foreach (var appender in bufferingAppenders)
            {
                appender.Flush();
            }
        }


        #region IRegisteredObject

        public void Stop(bool immediate)
        {
            RemoveEmptyLogFiles();
            HostingEnvironment.UnregisterObject(this);
        }

        internal static void RemoveEmptyLogFiles()
        {
            var fileAppenders = LogManager.GetRepository()?.GetAppenders()?.OfType<FileAppender>();
            if (fileAppenders != null)
            {
                foreach (var appender in fileAppenders)
                {
                    // Delete log file if it's empty
                    var logFile = new FileInfo(appender.File);
                    if (logFile.Exists && logFile.Length <= 0)
                    {
                        logFile.Delete();
                    }
                }
            }
        }

        #endregion
    }

    //public class DbAppender : AdoNetAppender
    //{
    //	protected override void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
    //	{
    //		try
    //		{
    //			base.SendBuffer(dbTran, events);
    //		}
    //		catch (Exception ex)
    //		{
    //			Debug.WriteLine(ex.Message);
    //		}
    //	}
    //}
}
