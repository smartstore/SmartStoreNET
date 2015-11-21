using SmartStore.ComponentModel;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.DataExchange;
using SmartStore.Services.DataExchange.Csv;

namespace SmartStore.Admin.Infrastructure
{
    public class AdminStartupTask : IStartupTask
    {
        public void Execute()
        {
			TypeConverterFactory.RegisterConverter<CsvConfiguration>(new CsvConfigurationConverter());
			TypeConverterFactory.RegisterConverter<ColumnMapConverter>(new ColumnMapConverter());
		}

        public int Order
        {
            get { return 100; }
        }
    }
}