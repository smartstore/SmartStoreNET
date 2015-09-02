using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.DataExchange
{
	public class DataExchangeSettings : ISettings
	{
		public DataExchangeSettings()
		{
			MaxFileNameLength = 50;
		}

		/// <summary>
		/// The maximum length of file names (in characters) of files created by the export framework
		/// </summary>
		public int MaxFileNameLength { get; set; }
	}
}
