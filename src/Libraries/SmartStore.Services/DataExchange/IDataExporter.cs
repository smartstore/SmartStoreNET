using System;
using System.Collections.Generic;
using System.Threading;
using SmartStore.Core.Domain;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public delegate void ProgressValueSetter(int value, int maximum, string message);
	public delegate void ProgressMessageSetter(string message);

	public interface IDataExporter
	{
		DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken);

		// Handle model conversion for grid in backend's controller
		IList<dynamic> Preview(DataExportRequest request);

		// useful for decision making whether export should
		// be processed sync or async
		long GetDataCount(DataExportRequest request);
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

			CustomData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public ExportProfile Profile { get; private set; }

		public Provider<IExportProvider> Provider { get; private set; }

		public IEnumerable<int> EntitiesToExport { get; set; }

		public ProgressValueSetter ProgressValueSetter { get; set; }
		public ProgressMessageSetter ProgressMessageSetter { get; set; }

		public IDictionary<string, object> CustomData { get; private set; }


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
