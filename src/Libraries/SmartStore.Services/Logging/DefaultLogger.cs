using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Logging;
using SmartStore.Data;

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
        private readonly IWorkContext _workContext;

		private readonly IList<LogContext> _entries = new List<LogContext>();

        #endregion

        #region Ctor

		public DefaultLogger(IRepository<Log> logRepository, IWebHelper webHelper, IDbContext dbContext, IWorkContext workContext)
        {
            this._logRepository = logRepository;
            this._webHelper = webHelper;
            this._dbContext = dbContext;
			this._workContext = workContext;
        }

        #endregion

        #region Methods

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

        public virtual void DeleteLog(Log log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            _logRepository.Delete(log);
        }

        public virtual void ClearLog()
        {
			try
			{
				_dbContext.ExecuteSqlCommand("TRUNCATE TABLE [Log]");
			}
			catch
			{
				try
				{
					for (int i = 0; i < 100000; ++i)
					{
						if (_dbContext.ExecuteSqlCommand("Delete Top ({0}) From [Log]", false, null, _deleteNumberOfEntries) < _deleteNumberOfEntries)
							break;
					}
				}
				catch { }

				try
				{
					_dbContext.ExecuteSqlCommand("DBCC CHECKIDENT('Log', RESEED, 0)");
				}
				catch
				{
					try
					{
						_dbContext.ExecuteSqlCommand("Alter Table [Log] Alter Column [Id] Identity(1,1)");
					}
					catch{ }
				}
			}

			_dbContext.ShrinkDatabase();
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

				_dbContext.ShrinkDatabase();
			}
			catch { }
		}

        public virtual IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc, string message, LogLevel? logLevel, int pageIndex, int pageSize, int minFrequency)
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

        public virtual Log GetLogById(int logId)
        {
            if (logId == 0)
                return null;

            var log = _logRepository.GetById(logId);
            return log;
        }

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

		public virtual void InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
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

		public virtual void InsertLog(LogContext context)
        {
			_entries.Add(context);
			if (_entries.Count == 50)
			{
				Flush();
			}
        }

		public void Flush()
		{
			if (_entries.Count == 0)
				return;

			string ipAddress = "";
			string pageUrl = "";
			string referrerUrl = "";
			var currentCustomer = _workContext.CurrentCustomer;

			try
			{
				ipAddress = _webHelper.GetCurrentIpAddress();
				pageUrl = _webHelper.GetThisPageUrl(true);
				referrerUrl = _webHelper.GetUrlReferrer();
			}
			catch { }

			using (var scope = new DbContextScope(autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				foreach (var context in _entries)
				{
					if (context.ShortMessage.IsEmpty() && context.FullMessage.IsEmpty())
						continue;

					Log log = null;

					try
					{
						string shortMessage = context.ShortMessage.NaIfEmpty();
						string fullMessage = context.FullMessage.EmptyNull();
						string contentHash = null;

						if (context.HashNotFullMessage || context.HashIpAddress)
						{
							contentHash = (shortMessage
								+ (context.HashNotFullMessage ? "" : fullMessage)
								+ (context.HashIpAddress ? ipAddress.EmptyNull() : "")
							).Hash(Encoding.Unicode, true);
						}
						else
						{
							contentHash = (shortMessage + fullMessage).Hash(Encoding.Unicode, true);
						}

						log = _logRepository.Table.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault(x => x.ContentHash == contentHash);

						if (log == null)
						{
							log = new Log
							{
								Frequency = 1,
								LogLevel = context.LogLevel,
								ShortMessage = shortMessage,
								FullMessage = fullMessage,
								IpAddress = ipAddress,
								Customer = context.Customer ?? currentCustomer,
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
							log.Customer = context.Customer ?? currentCustomer;
							log.PageUrl = pageUrl;
							log.ReferrerUrl = referrerUrl;
							log.UpdatedOnUtc = DateTime.UtcNow;

							_logRepository.Update(log);
						}
					}
					catch (Exception ex)
					{
						ex.Dump();
					}
				}

				try
				{
					// FIRE!
					_logRepository.Context.SaveChanges();
				}
				catch { }
			}

			_entries.Clear();
		}

        #endregion

	}
}
