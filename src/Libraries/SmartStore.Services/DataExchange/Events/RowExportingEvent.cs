using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Events
{
	public class RowExportingEvent
	{
		public dynamic Row { get; internal set; }
		public ExportEntityType EntityType { get; internal set; }
		public DataExportRequest ExportRequest { get; internal set; }
		public IExportExecuteContext ExecuteContext { get; internal set; }
	}
}
