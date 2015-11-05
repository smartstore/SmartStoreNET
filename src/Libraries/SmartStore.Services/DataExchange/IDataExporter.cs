using SmartStore.Core.Domain;
using SmartStore.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Services.DataExchange
{
	public delegate void ProgressSetter(int value, int maximum, string message);

	public interface IDataExporter
	{
		void Export(DataExportRequest request, CancellationToken cancellationToken);
	}

	public class DataExportRequest
	{
		private readonly static ProgressSetter _voidProgressSetter = DataExportRequest.SetProgress;

		public DataExportRequest(ExportProfile profile)
		{
			Guard.ArgumentNotNull(() => profile);

			CustomData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			ProgressSetter = _voidProgressSetter;
			Profile = profile;
        }

		public ExportProfile Profile { get; private set; }

		public Provider<IExportProvider> Provider { get; set; }

		public IEnumerable<int> EntitiesToExport { get; set; }

		public ProgressSetter ProgressSetter { get; set; }

		public IDictionary<string, object> CustomData { get; private set; }


		private static void SetProgress(int val, int max, string msg)
		{
			// do nothing
		}
	}
}
