using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Export
{
	public interface IDataExporter
	{
		DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken);

		IList<dynamic> Preview(DataExportRequest request, int pageIndex, int? totalRecords = null);

		int GetDataCount(DataExportRequest request);
	}


	public class DataExportRequest
	{
		private readonly static ProgressValueSetter _voidProgressValueSetter = DataExportRequest.SetProgress;

		public DataExportRequest(ExportProfile profile, Provider<IExportProvider> provider)
		{
			Guard.NotNull(profile, nameof(profile));
			Guard.NotNull(provider, nameof(provider));

			Profile = profile;
			Provider = provider;
			ProgressValueSetter = _voidProgressValueSetter;

			EntitiesToExport = new List<int>();
			CustomData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public ExportProfile Profile { get; private set; }

		public Provider<IExportProvider> Provider { get; private set; }

		public ProgressValueSetter ProgressValueSetter { get; set; }

		public bool HasPermission { get; set; }

		public IList<int> EntitiesToExport { get; set; }

		public IDictionary<string, object> CustomData { get; private set; }

		public IQueryable<Product> ProductQuery { get; set; }


		private static void SetProgress(int val, int max, string msg)
		{
			// do nothing
		}
	}
}
