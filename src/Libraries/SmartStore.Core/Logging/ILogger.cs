using System;

namespace SmartStore.Core.Logging
{
	/// <summary>
	/// Interface that all loggers implement
	/// </summary>
	public partial interface ILogger
    {
		/// <summary>
		/// Checks if this logger is enabled for a given <see cref="LogLevel"/> passed as parameter. 
		/// </summary>
		/// <param name="level">true if this logger is enabled for level, otherwise false</param>
		/// <returns>Result</returns>
		bool IsEnabledFor(LogLevel level);

		/// <summary>
		/// Generates a logging event for the specified level using the message and exception
		/// </summary>
		/// <param name="logLevel">The level of the message to be logged</param>
		/// <param name="exception">The exception to log, including its stack trace. Pass null to not log an exception</param>
		/// <param name="message">The message object to log</param>
		/// <param name="args">An Object array containing zero or more objects to format. Can be null.</param>
		void Log(LogLevel level, Exception exception, string message, object[] args);
    }
}
