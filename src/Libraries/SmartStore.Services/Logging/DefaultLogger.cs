using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Logging
{
    /// <summary>
    /// Default logger
    /// </summary>
    public partial class DefaultLogger : ILogger
    {
        #region Fields

		private const int _deleteNumberOfEntries = 1000;

        private readonly IRepository<Log> _logRepository;
        private readonly IWebHelper _webHelper;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logRepository">Log repository</param>
        /// <param name="webHelper">Web helper</param>>
        /// <param name="dbContext">DB context</param>>
        /// <param name="dataProvider">WeData provider</param>
        /// <param name="commonSettings">Common settings</param>
        public DefaultLogger(IRepository<Log> logRepository, IWebHelper webHelper, IDbContext dbContext, IDataProvider dataProvider)
        {
            this._logRepository = logRepository;
            this._webHelper = webHelper;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        public virtual bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        public virtual void DeleteLog(Log log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            _logRepository.Delete(log);
        }

        /// <summary>
        /// Clears a log
        /// </summary>
        public virtual void ClearLog()
        {
			try
			{
				_dbContext.ExecuteSqlCommand("TRUNCATE TABLE [Log]");
			}
			catch (Exception)
			{
				try
				{
					for (int i = 0; i < 100000; ++i)
					{
						if (_dbContext.ExecuteSqlCommand("Delete Top ({0}) From [Log]", false, null, _deleteNumberOfEntries) < _deleteNumberOfEntries)
							break;
					}
				}
				catch (Exception) { }

				try
				{
					_dbContext.ExecuteSqlCommand("DBCC CHECKIDENT('Log', RESEED, 0)");
				}
				catch (Exception)
				{
					try
					{
						_dbContext.ExecuteSqlCommand("Alter Table [Log] Alter Column [Id] Identity(1,1)");
					}
					catch (Exception) { }
				}
			}
        }

		public virtual void ClearLog(DateTime toUtc, LogLevel logLevel)
		{
			try
			{
				string sqlDelete = "Delete Top ({0}) From [Log] Where LogLevelId < {1} And CreatedOnUtc <= {2}";

				for (int i = 0; i < 100000; ++i)
				{
					if (_dbContext.ExecuteSqlCommand(sqlDelete, false, null, _deleteNumberOfEntries, (int)logLevel, toUtc) < _deleteNumberOfEntries)
						break;
				}
			}
			catch (Exception) { }
		}

        /// <summary>
        /// Gets all log items
        /// </summary>
        /// <param name="fromUtc">Log item creation from; null to load all records</param>
        /// <param name="toUtc">Log item creation to; null to load all records</param>
        /// <param name="message">Message</param>
        /// <param name="logLevel">Log level; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Log item collection</returns>
        public virtual IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc,
			string message, LogLevel? logLevel, int pageIndex, int pageSize, int minFrequency)
        {
            var query = _logRepository.Table;
            
            if (fromUtc.HasValue)
                query = query.Where(l => fromUtc.Value <= l.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(l => toUtc.Value >= l.CreatedOnUtc);
            if (logLevel.HasValue)
            {
                int logLevelId = (int)logLevel.Value;
                query = query.Where(l => logLevelId == l.LogLevelId);
            }
            if (!String.IsNullOrEmpty(message))
                query = query.Where(l => l.ShortMessage.Contains(message) || l.FullMessage.Contains(message));
            query = query.OrderByDescending(l => l.CreatedOnUtc);

			if (minFrequency > 0)
				query = query.Where(l => l.Frequency >= minFrequency);

            //query = _logRepository.Expand(query, x => x.Customer);

            var log = new PagedList<Log>(query, pageIndex, pageSize);
            return log;
        }

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        public virtual Log GetLogById(int logId)
        {
            if (logId == 0)
                return null;

            var log = _logRepository.GetById(logId);
            return log;
        }

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        public virtual IList<Log> GetLogByIds(int[] logIds)
        {
            if (logIds == null || logIds.Length == 0)
                return new List<Log>();

            var query = from l in _logRepository.Table
                        where logIds.Contains(l.Id)
                        select l;
            var logItems = query.ToList();
            //sort by passed identifiers
            var sortedLogItems = new List<Log>();
            foreach (int id in logIds)
            {
                var log = logItems.Find(x => x.Id == id);
                if (log != null)
                    sortedLogItems.Add(log);
            }
            return sortedLogItems;
        }

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="context">The log context</param>
        /// <returns>A log item</returns>
        public virtual Log InsertLog(LogContext context)
        {
			if (context == null || (context.ShortMessage.IsNullOrEmpty() && context.FullMessage.IsNullOrEmpty()))
				return null;

			Log log = null;

			try
			{
				string shortMessage = context.ShortMessage.NaIfEmpty();
				string fullMessage = context.FullMessage.EmptyNull();
				string contentHash = null;
				string ipAddress = "";
				string pageUrl = "";
				string referrerUrl = "";

				try
				{
					ipAddress = _webHelper.GetCurrentIpAddress();
					pageUrl = _webHelper.GetThisPageUrl(true);
					referrerUrl = _webHelper.GetUrlReferrer();
				}
				catch { }

				if (context.HashNotFullMessage || context.HashIpAddress)
				{
					contentHash = (shortMessage
						+ (context.HashNotFullMessage ? "" : fullMessage)
						+ (context.HashIpAddress ? ipAddress.EmptyNull() : "")
					).Hash(true, true);
				}
				else
				{
					contentHash = (shortMessage + fullMessage).Hash(true, true);
				}

				log = _logRepository.Table.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault(x => x.ContentHash == contentHash);

				if (log == null)
				{
					log = new Log()
					{
						Frequency = 1,
						LogLevel = context.LogLevel,
						ShortMessage = shortMessage,
						FullMessage = fullMessage,
						IpAddress = ipAddress,
						Customer = context.Customer,
						PageUrl = pageUrl,
						ReferrerUrl = referrerUrl,
						CreatedOnUtc = DateTime.UtcNow,
						ContentHash = contentHash
					};

					_logRepository.Insert(log);
				}
				else
				{
					if (log.Frequency < 2147483647)
						log.Frequency = log.Frequency + 1;

					log.LogLevel = context.LogLevel;
					log.IpAddress = ipAddress;
					log.Customer = context.Customer;
					log.PageUrl = pageUrl;
					log.ReferrerUrl = referrerUrl;
					log.UpdatedOnUtc = DateTime.UtcNow;

					_logRepository.Update(log);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return log;
        }

		/// <summary>
		/// Inserts a log item
		/// </summary>
		/// <param name="logLevel">Log level</param>
		/// <param name="shortMessage">The short message</param>
		/// <param name="fullMessage">The full message</param>
		/// <param name="customer">The customer to associate log record with</param>
		/// <returns>A log item</returns>
		public virtual Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
		{
			var context = new LogContext()
			{
				LogLevel = logLevel,
				ShortMessage = shortMessage,
				FullMessage = fullMessage,
				Customer = customer
			};

			return InsertLog(context);
		}

        #endregion
    }
}
