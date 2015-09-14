using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange
{
	public interface IExportExecuteContext
	{
		/// <summary>
		/// Provides the data to be exported
		/// </summary>
		IExportSegmenter Data { get; }

		/// <summary>
		/// The store context to be used for the export
		/// </summary>
		dynamic Store { get; }

		/// <summary>
		/// The customer context to be used for the export
		/// </summary>
		dynamic Customer { get; }

		/// <summary>
		/// The currency context to be used for the export
		/// </summary>
		dynamic Currency { get; }

		/// <summary>
		/// Projection data
		/// </summary>
		ExportProjection Projection { get; }

		/// <summary>
		/// To log information into the export log file
		/// </summary>
		ILogger Log { get; }

		/// <summary>
		/// Indicates whether and how to abort the export
		/// </summary>
		ExportAbortion Abort { get; set; }

		/// <summary>
		/// The maximum allowed file name length
		/// </summary>
		int MaxFileNameLength { get; }

		/// <summary>
		/// The path of the export folder
		/// </summary>
		string Folder { get; }

		/// <summary>
		/// The name of the current export file
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// The public URL of the export file (accessible through the internet)
		/// </summary>
		string FileUrl { get; }

		/// <summary>
		/// The path of the current export file
		/// </summary>
		string FilePath { get; }

		/// <summary>
		/// Provider specific configuration data
		/// </summary>
		object ConfigurationData { get; }

		/// <summary>
		/// Use this dictionary for any custom data required along the export
		/// </summary>
		Dictionary<string, object> CustomProperties { get; set; }

		/// <summary>
		/// Number of successful exported records. Should be incremented by the provider. Will be logged.
		/// </summary>
		int RecordsSucceeded { get; set; }

		/// <summary>
		/// Number of failed records. Should be incremented by the provider. Will be logged.
		/// </summary>
		int RecordsFailed { get; set; }
	}


	public class ExportExecuteContext : IExportExecuteContext
	{
		private CancellationToken _cancellation;
		private IExportSegmenter _segmenter;
		private ExportAbortion _providerAbort;

		internal ExportExecuteContext(CancellationToken cancellation, string folder)
		{
			_cancellation = cancellation;
			Folder = folder;

			CustomProperties = new Dictionary<string, object>();
		}

		public IExportSegmenter Data
		{
			get
			{
				return _segmenter;
			}
			internal set
			{
				if (_segmenter != null)
					(_segmenter as IExportExecuter).Dispose();

				_segmenter = value;
			}
		}

		public dynamic Store { get; internal set; }
		public dynamic Customer { get; internal set; }
		public dynamic Currency { get; internal set; }
		public ExportProjection Projection { get; internal set; }

		public ILogger Log { get; internal set; }

		public ExportAbortion Abort
		{
			get
			{
				if (_cancellation.IsCancellationRequested)
					return ExportAbortion.Hard;
				
				if (IsMaxFailures)
					return ExportAbortion.Soft;

				return _providerAbort;
			}
			set
			{
				_providerAbort = value;
			}
		}

		public bool IsMaxFailures
		{
			get { return RecordsFailed > 11; }
		}

		public int MaxFileNameLength { get; internal set; }

		public string Folder { get; private set; }
		public string FileNamePattern { get; internal set; }
		public string FileExtension { get; internal set; }
		public string FileName
		{
			get
			{
				var finallyResolvedPattern = FileNamePattern
					.Replace("%Misc.FileNumber%", (Data.FileIndex + 1).ToString("D4"))
					.ToValidFileName("")
					.Truncate(MaxFileNameLength);

				return string.Concat(finallyResolvedPattern, FileExtension);
			}
		}
		public string FilePath
		{
			get { return Path.Combine(Folder, FileName); }
		}
		public string FileUrl
		{
			get
			{
				var url = string.Concat(((string)Store.Url).EnsureEndsWith("/"), ExportProfileTask.PublicFolder, "/", FileName);
				return url;
			}
		}

		public object ConfigurationData { get; internal set; }

		public Dictionary<string, object> CustomProperties { get; set; }

		public int RecordsSucceeded { get; set; }
		public int RecordsFailed { get; set; }
	}
}
