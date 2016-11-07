using System.Collections.Generic;
using System.Threading;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Import
{
	public class ImportExecuteContext
	{
		private DataExchangeAbortion _abortion;
		private ProgressValueSetter _progressValueSetter;
		private string _progressInfo;

		private IDataTable _dataTable;
		private ImportDataSegmenter _segmenter;

		public ImportExecuteContext(
			CancellationToken cancellation,
			ProgressValueSetter progressValueSetter,
			string progressInfo)
		{
			_progressValueSetter = progressValueSetter;
			_progressInfo = progressInfo;

			CancellationToken = cancellation;
			CustomProperties = new Dictionary<string, object>();
			Result = new ImportResult();
		}

		public DataImportRequest Request
		{
			get;
			internal set;
		}

		/// <summary>
		/// Import settings
		/// </summary>
		public DataExchangeSettings DataExchangeSettings
		{
			get;
			internal set;
		}

		/// <summary>
		/// The data source (CSV, Excel etc.)
		/// </summary>
		public IDataTable DataTable
		{
			get
			{
				return _dataTable;
			}
			internal set
			{
				_dataTable = value;
				_segmenter = null;
			}
		}

		/// <summary>
		/// Mapping information between database and data source
		/// </summary>
		public ColumnMap ColumnMap
		{
			get;
			internal set;
		}

		/// <summary>
		/// Whether to only update existing records
		/// </summary>
		public bool UpdateOnly
		{
			get;
			internal set;
		}

		/// <summary>
		/// Name of key fields to identify existing records for updating
		/// </summary>
		public string[] KeyFieldNames
		{
			get;
			internal set;
		}

		/// <summary>
		/// All active languages
		/// </summary>
		public IList<Language> Languages
		{
			get;
			internal set;
		}

		/// <summary>
		/// To log information into the import log file
		/// </summary>
		public ILogger Log
		{
			get;
			internal set;
		}

		/// <summary>
		/// Common Services
		/// </summary>
		public ICommonServices Services
		{
			get;
			internal set;
		}

		/// <summary>
		/// Cancellation token
		/// </summary>
		public CancellationToken CancellationToken
		{
			get;
			private set;
		}

		/// <summary>
		/// The import folder
		/// </summary>
		public string ImportFolder
		{
			get;
			internal set;
		}

		/// <summary>
		/// Use this dictionary for any custom data required along the import
		/// </summary>
		public Dictionary<string, object> CustomProperties
		{
			get;
			set;
		}

		/// <summary>
		/// Result of the import
		/// </summary>
		public ImportResult Result
		{
			get;
			set;
		}

		/// <summary>
		/// Extra import configuration data
		/// </summary>
		public ImportExtraData ExtraData
		{
			get;
			internal set;
		}

		/// <summary>
		/// Indicates whether and how to abort the import
		/// </summary>
		public DataExchangeAbortion Abort
		{
			get
			{
				if (CancellationToken.IsCancellationRequested || IsMaxFailures)
					return DataExchangeAbortion.Hard;

				return _abortion;
			}
			set
			{
				_abortion = value;
			}
		}

		public bool IsMaxFailures
		{
			get
			{
				return Result.Errors > 11;
			}
		}

		public ImportDataSegmenter DataSegmenter
		{
			get
			{
				if (_segmenter == null)
				{
					if (this.DataTable == null || this.ColumnMap == null)
					{
						throw new SmartException("A DataTable and a ColumnMap must be specified before accessing the DataSegmenter property.");
					}
					_segmenter = new ImportDataSegmenter(DataTable, ColumnMap);
				}

				return _segmenter;
			}
		}

		/// <summary>
		/// Allows to set a progress message
		/// </summary>
		/// <param name="value">Progress value</param>
		/// /// <param name="maximum">Progress maximum</param>
		public void SetProgress(int value, int maximum)
		{
			try
			{
				_progressValueSetter?.Invoke(value, maximum, _progressInfo.FormatInvariant(value, maximum));
			}
			catch { }
		}

        /// <summary>
        /// Allows to set a message
        /// </summary>
        /// <param name="message">Message to display</param>
        public void SetProgress(string message)
        {
            try
            {
                if (message.HasValue())
                    _progressValueSetter?.Invoke(0, 0, message);
            }
            catch { }
        }
	}
}
