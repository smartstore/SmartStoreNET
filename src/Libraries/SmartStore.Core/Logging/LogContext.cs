using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
	public class LogContext
	{
		public string ShortMessage { get; set; }
		public string FullMessage { get; set; }
		public LogLevel LogLevel { get; set; }
		public Customer Customer { get; set; }

		public bool HashNotFullMessage { get; set; }
		public bool HashIpAddress { get; set; }
	}
}
