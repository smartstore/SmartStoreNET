using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
    /// <summary>
    /// Null logger
    /// </summary>
    public partial class NullLogger : ILogger
    {
		private static readonly ILogger s_instance = new NullLogger();

		public static ILogger Instance
		{
			get
			{
				return s_instance;
			}
		}
		
		/// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        public bool IsEnabled(LogLevel level)
        {
            return false;
        }

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        public void DeleteLog(Log log)
        {
        }

        /// <summary>
        /// Clears a log
        /// </summary>
        public void ClearLog()
        {
        }

		public void ClearLog(DateTime toUtc, LogLevel logLevel)
		{
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
        public IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc,
			string message, LogLevel? logLevel, int pageIndex, int pageSize, int minFrequency)
        {
            return new PagedList<Log>(new List<Log>(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        public Log GetLogById(int logId)
        {
            return null;
        }

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        public virtual IList<Log> GetLogByIds(int[] logIds)
        {
            return new List<Log>();
        }

		public void InsertLog(LogContext context)
		{
		}

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        public void InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
        }

		public void Flush()
		{
		}
	}
}
