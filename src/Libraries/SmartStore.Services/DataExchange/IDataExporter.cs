using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Export
{
	public delegate void ProgressValueSetter(int value, int maximum, string message);
	public delegate void ProgressMessageSetter(string message);

	public interface IDataExporter
	{
		DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken);

		IList<dynamic> Preview(DataExportRequest request, int pageIndex, int? totalRecords = null);

		int GetDataCount(DataExportRequest request);
	}


	public class DataExportRequest
	{
		private readonly static ProgressValueSetter _voidProgressValueSetter = DataExportRequest.SetProgress;
		private readonly static ProgressMessageSetter _voidProgressMessageSetter = DataExportRequest.SetProgress;

		public DataExportRequest(ExportProfile profile, Provider<IExportProvider> provider)
		{
			Guard.ArgumentNotNull(() => profile);
			Guard.ArgumentNotNull(() => provider);

			Profile = profile;
			Provider = provider;

			ProgressValueSetter = _voidProgressValueSetter;
			ProgressMessageSetter = _voidProgressMessageSetter;

			EntitiesToExport = new List<int>();
			CustomData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public ExportProfile Profile { get; private set; }

		public Provider<IExportProvider> Provider { get; private set; }

		public IList<int> EntitiesToExport { get; set; }

		public ProgressValueSetter ProgressValueSetter { get; set; }
		public ProgressMessageSetter ProgressMessageSetter { get; set; }

		public IDictionary<string, object> CustomData { get; private set; }

		public IQueryable<Product> ProductQuery { get; set; }


		private static void SetProgress(int val, int max, string msg)
		{
			// do nothing
		}

		private static void SetProgress(string msg)
		{
			// do nothing
		}
	}
}
