using System.Collections.Generic;
using System.IO;
using System.Threading;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public interface IExportExecuteContext
	{
		IExportSegmenter Data { get; }

		ILogger Log { get; }

		bool IsCanceled { get; }

		string Folder { get; }

		string FileNamePattern { get; }

		Dictionary<string, object> CustomProperties { get; set; }

		string GetFilePath(int numberOfCreatedFiles, string fileNameSuffix = null);
	}

	public class ExportExecuteContext : IExportExecuteContext
	{
		private CancellationToken _cancellation;

		internal ExportExecuteContext(CancellationToken cancellation, string folder)
		{
			_cancellation = cancellation;
			Folder = folder;

			CustomProperties = new Dictionary<string, object>();
		}

		public IExportSegmenter Data { get; internal set; }

		public ILogger Log { get; internal set; }

		public bool IsCanceled
		{
			get { return _cancellation.IsCancellationRequested; }
		}

		public string Folder { get; private set; }

		public string FileNamePattern { get; internal set; }

		public Dictionary<string, object> CustomProperties { get; set; }

		public string GetFilePath(int numberOfCreatedFiles, string fileNameSuffix = null)
		{
			var fileName = FileNamePattern.FormatInvariant(
				numberOfCreatedFiles.ToString("D5"),
				SeoHelper.GetSeName(fileNameSuffix.EmptyNull(), true, false)
			);

			return Path.Combine(Folder, fileName);
		}
	}
}
