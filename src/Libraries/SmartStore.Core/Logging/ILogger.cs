using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
    /// <summary>
    /// Logger interface
    /// </summary>
    public partial interface ILogger
    {
        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        bool IsEnabled(LogLevel level);

		/// <summary>
		/// Inserts a log item
		/// </summary>
		/// <param name="context">The log context</param>
		/// <returns>Always return <c>null</c></returns>
		void InsertLog(LogContext context);

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
		/// <returns>Always return <c>null</c></returns>
        void InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null);

		/// <summary>
		/// Commits log entries to the data store
		/// </summary>
		void Flush();
    }
}
