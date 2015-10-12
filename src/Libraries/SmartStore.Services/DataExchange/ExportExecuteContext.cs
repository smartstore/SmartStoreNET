using System;
using System.Collections.Generic;
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
		IExportSegmenterConsumer Segmenter { get; }

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
		/// The language context to be used for the export
		/// </summary>
		dynamic Language { get; }

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
		/// The path of the export content folder
		/// </summary>
		string Folder { get; }

		/// <summary>
		/// The name of the current export file
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// The path of the current export file
		/// </summary>
		string FilePath { get; }


		/// <summary>
		/// Whether the profile has a public deployment into "Exchange" folder
		/// </summary>
		bool HasPublicDeployment { get; }

		/// <summary>
		/// The local path to the public export folder "Exchange". <c>null</c> if the profile has no public deployment.
		/// </summary>
		string PublicFolderPath { get; }

		/// <summary>
		/// The public URL of the export file (accessible through the internet). <c>null</c> if the profile has no public deployment.
		/// </summary>
		string PublicFileUrl { get; }


		/// <summary>
		/// Provider specific configuration data
		/// </summary>
		object ConfigurationData { get; }

		/// <summary>
		/// Use this dictionary for any custom data required along the export
		/// </summary>
		Dictionary<string, object> CustomProperties { get; set; }

		/// <summary>
		/// Number of successful processed records
		/// </summary>
		int RecordsSucceeded { get; set; }

		/// <summary>
		/// Number of failed records
		/// </summary>
		int RecordsFailed { get; set; }

		/// <summary>
		/// Processes an exception that occurred while exporting a record
		/// </summary>
		/// <param name="exc">Exception</param>
		void RecordException(Exception exc, int entityId);
	}


	public class ExportExecuteContext : IExportExecuteContext
	{
		private ExportExecuteResult _result;
		private CancellationToken _cancellation;
		private ExportAbortion _providerAbort;

		internal ExportExecuteContext(ExportExecuteResult result, CancellationToken cancellation, string folder)
		{
			_result = result;
			_cancellation = cancellation;
			Folder = folder;

			CustomProperties = new Dictionary<string, object>();
		}

		public IExportSegmenterConsumer Segmenter { get; set; }

		public dynamic Store { get; internal set; }
		public dynamic Customer { get; internal set; }
		public dynamic Currency { get; internal set; }
		public dynamic Language { get; internal set; }
		public ExportProjection Projection { get; internal set; }

		public ILogger Log { get; internal set; }

		public ExportAbortion Abort
		{
			get
			{
				if (_cancellation.IsCancellationRequested || IsMaxFailures)
					return ExportAbortion.Hard;

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
		public string FileName { get; internal set; }
		public string FileExtension { get; internal set; }
		public string FilePath { get; internal set; }

		public bool HasPublicDeployment { get; internal set; }
		public string PublicFolderPath { get; internal set; }
		public string PublicFileUrl { get; internal set; }

		public object ConfigurationData { get; internal set; }

		public Dictionary<string, object> CustomProperties { get; set; }

		public int RecordsSucceeded { get; set; }
		public int RecordsFailed { get; set; }

		public void RecordException(Exception exc, int entityId)
		{
			++RecordsFailed;

			Log.Error("Error while processing record with id {0}: {1}".FormatInvariant(entityId, exc.ToAllMessages()), exc);

			if (IsMaxFailures)
				_result.LastError = exc.ToString();
		}
	}
}
