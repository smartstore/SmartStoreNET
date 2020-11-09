using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Export.Events
{
    // TODO: Another event message must be implemented, say 'ColumnsBuildingEvent'
    // The consumer of this event (most likely a plugin) could push a list of specific column headers
    // into the global export definition.

    public class RowExportingEvent
    {
        public dynamic Row
        {
            get;
            internal set;
        }

        public ExportEntityType EntityType
        {
            get;
            internal set;
        }

        public DataExportRequest ExportRequest
        {
            get;
            internal set;
        }

        public ExportExecuteContext ExecuteContext
        {
            get;
            internal set;
        }
    }
}
