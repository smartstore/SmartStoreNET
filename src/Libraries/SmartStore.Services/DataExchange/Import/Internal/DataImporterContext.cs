using System.Threading;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Import.Internal
{
	internal class DataImporterContext
	{
		public DataImporterContext(
			DataImportRequest request,
			CancellationToken cancellationToken,
			string progressInfo)
		{
			Request = request;
			CancellationToken = cancellationToken;

			ExecuteContext = new ImportExecuteContext(CancellationToken, Request.ProgressValueSetter, progressInfo) { Request = request };
		}

		public DataImportRequest Request { get; private set; }
		public CancellationToken CancellationToken { get; private set; }

		public TraceLogger Log { get; set; }

		public ImportExecuteContext ExecuteContext { get; set; }

		public IEntityImporter Importer { get; set; }
	}
}
