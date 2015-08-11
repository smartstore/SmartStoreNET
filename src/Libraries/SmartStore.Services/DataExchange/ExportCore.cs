using System.Collections.Generic;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange
{
	public class ExportExecuteContext
	{
		/// <summary>
		/// Record to be exported. Can be <c>null</c>.
		/// </summary>
		public ExportRecord Record { get; internal set; }

		/// <summary>
		/// Number of exported records
		/// </summary>
		public int RecordCount { get; internal set; }

		/// <summary>
		/// The file stream
		/// </summary>
		public DataExchangeStream File { get; internal set; }

		/// <summary>
		/// Entries to be written to the log file
		/// </summary>
		public IList<LogContext> Logs { get; set; }
	}

	public class ExportRecord	// TODO: whatever you are
	{
	}
}
