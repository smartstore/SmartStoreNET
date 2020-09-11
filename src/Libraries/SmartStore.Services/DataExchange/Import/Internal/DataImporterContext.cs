using System.Collections.Generic;
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

            ExecuteContext = new ImportExecuteContext(CancellationToken, Request.ProgressValueSetter, progressInfo)
            {
                Request = request
            };

            ColumnMap = new ColumnMapConverter().ConvertFrom<ColumnMap>(Request.Profile.ColumnMapping) ?? new ColumnMap();
            Results = new Dictionary<string, ImportResult>();
        }

        public DataImportRequest Request { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public TraceLogger Log { get; set; }
        public ImportExecuteContext ExecuteContext { get; set; }
        public IEntityImporter Importer { get; set; }
        public ColumnMap ColumnMap { get; private set; }
        public Dictionary<string, ImportResult> Results { get; private set; }
    }
}
