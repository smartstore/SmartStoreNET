using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Logging
{
    public partial class DefaultLogger : ILogger
    {
        private readonly IRepository<Log> _logRepository;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

		private readonly IList<LogContext> _entries = new List<LogContext>();


		public DefaultLogger(IRepository<Log> logRepository, IWebHelper webHelper, IWorkContext workContext)
        {
            this._logRepository = logRepository;
            this._webHelper = webHelper;
			this._workContext = workContext;
        }

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
	}
}
