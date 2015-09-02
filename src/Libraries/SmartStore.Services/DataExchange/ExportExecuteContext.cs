using System.Collections.Generic;
using System.Dynamic;
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

		ExpandoObject Store { get; }

		object ConfigurationData { get; }

		Dictionary<string, object> CustomProperties { get; set; }

		int SuccessfulExportedRecords { get; set; }

		int MaxFileNameLength { get; }

		string Folder { get; }
		string FileName { get; }
		string FilePath { get; }
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

		public ExpandoObject Store { get; internal set; }

		public object ConfigurationData { get; internal set; }

		public Dictionary<string, object> CustomProperties { get; set; }

		public int SuccessfulExportedRecords { get; set; }

		public int MaxFileNameLength { get; internal set; }

		public string Folder { get; private set; }
		public string FileNamePattern { get; internal set; }
		public string FileExtension { get; internal set; }
		public string FileName
		{
			get
			{
				var finallyResolvedPattern = FileNamePattern
					.Replace("%Misc.FileNumber%", (Data.FileIndex + 1).ToString("D5"))
					.ToValidFileName("")
					.Truncate(MaxFileNameLength);

				return string.Concat(finallyResolvedPattern, FileExtension);
			}
		}
		public string FilePath
		{
			get { return Path.Combine(Folder, FileName); }
		}
	}
}
