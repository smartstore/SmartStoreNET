﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export
{
	public class ExportExecuteContext
	{
		private DataExportResult _result;
		private CancellationToken _cancellation;
		private DataExchangeAbortion _providerAbort;

		internal ExportExecuteContext(DataExportResult result, CancellationToken cancellation, string folder)
		{
			_result = result;
			_cancellation = cancellation;
			Folder = folder;
			ExtraDataUnits = new List<ExportDataUnit>();
			CustomProperties = new Dictionary<string, object>();
		}

		/// <summary>
		/// Identifier of the export profile
		/// </summary>
		public int ProfileId { get; internal set; }

		/// <summary>
		/// Provides the data to be exported
		/// </summary>
		public IExportDataSegmenterConsumer DataSegmenter { get; set; }

		/// <summary>
		/// The store context to be used for the export
		/// </summary>
		public dynamic Store { get; internal set; }

		/// <summary>
		/// The customer context to be used for the export
		/// </summary>
		public dynamic Customer { get; internal set; }

		/// <summary>
		/// The currency context to be used for the export
		/// </summary>
		public dynamic Currency { get; internal set; }

		/// <summary>
		/// The language context to be used for the export
		/// </summary>
		public dynamic Language { get; internal set; }

		/// <summary>
		/// Filter settings
		/// </summary>
		public ExportFilter Filter { get; internal set; }

		/// <summary>
		/// Projection settings
		/// </summary>
		public ExportProjection Projection { get; internal set; }

		/// <summary>
		/// To log information into the export log file
		/// </summary>
		public ILogger Log { get; internal set; }

		/// <summary>
		/// Indicates whether and how to abort the export
		/// </summary>
		public DataExchangeAbortion Abort
		{
			get
			{
				if (_cancellation.IsCancellationRequested || IsMaxFailures)
					return DataExchangeAbortion.Hard;

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

		/// <summary>
		/// Identifier of current data stream. Can be <c>null</c>.
		/// </summary>
		public string DataStreamId { get; set; }

		/// <summary>
		/// Stream used to write data to
		/// </summary>
		public Stream DataStream { get; internal set; }

		/// <summary>
		/// List with extra data units/streams required by provider
		/// </summary>
		public List<ExportDataUnit> ExtraDataUnits { get; private set; }

		/// <summary>
		/// The maximum allowed file name length
		/// </summary>
		public int MaxFileNameLength { get; internal set; }

		/// <summary>
		/// The name of the current export file
		/// </summary>
		public string FileName { get; internal set; }

		/// <summary>
		/// The path of the export content folder
		/// </summary>
		public string Folder { get; private set; }

		/// <summary>
		/// Whether the profile has a public deployment into "Exchange" folder
		/// </summary>
		public bool HasPublicDeployment { get; internal set; }

		/// <summary>
		/// The local path to the public export folder "Exchange". <c>null</c> if the profile has no public deployment.
		/// </summary>
		public string PublicFolderPath { get; internal set; }

		/// <summary>
		/// The URL of the public export folder "Exchange". <c>null</c> if the profile has no public deployment.
		/// </summary>
		public string PublicFolderUrl { get; internal set; }

		/// <summary>
		/// Provider specific configuration data
		/// </summary>
		public object ConfigurationData { get; internal set; }

		/// <summary>
		/// Use this dictionary for any custom data required along the export
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }

		/// <summary>
		/// Number of successful processed records
		/// </summary>
		public int RecordsSucceeded { get; set; }

		/// <summary>
		/// Number of failed records
		/// </summary>
		public int RecordsFailed { get; set; }

		/// <summary>
		/// Processes an exception that occurred while exporting a record
		/// </summary>
		/// <param name="exception">Exception</param>
		public void RecordException(Exception exception, int entityId)
		{
			++RecordsFailed;

			Log.ErrorFormat(exception, "Error while processing record with id {0}", entityId);

			if (IsMaxFailures)
				_result.LastError = exception.ToString();
		}

		public ProgressValueSetter ProgressValueSetter { get; internal set; }

		/// <summary>
		/// Allows to set a progress message
		/// </summary>
		/// <param name="message">Output message</param>
		public void SetProgress(string message)
		{
			if (ProgressValueSetter != null && message.HasValue())
			{
				try
				{
					ProgressValueSetter.Invoke(0, 0, message);
				}
				catch { }
			}
		}
	}

	public class ExportDataUnit
	{
		/// <summary>
		/// Your Id to identify this stream within a list of streams
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Stream used to write data to
		/// </summary>
		public Stream DataStream { get; internal set; }

		/// <summary>
		/// The name of the file to be created
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Short optional text that describes the content of the file
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Whether to display the file in the profile file dialog
		/// </summary>
		public bool DisplayInFileDialog { get; set; }
	}
}
