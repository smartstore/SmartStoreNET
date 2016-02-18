using System.Collections.Generic;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Import
{
	public interface IImportExecuteContext
	{
		/// <summary>
		/// Whether to only update existing records
		/// </summary>
		bool UpdateOnly { get; }

		/// <summary>
		/// Name of key fields to identify existing records for updating
		/// </summary>
		string[] KeyFieldNames { get; }

		/// <summary>
		/// The import folder
		/// </summary>
		string ImportFolder { get; }

		/// <summary>
		/// Use this dictionary for any custom data required along the export
		/// </summary>
		Dictionary<string, object> CustomProperties { get; set; }

		/// <summary>
		/// To log information into the import log file
		/// </summary>
		ILogger Log { get; }

		/// <summary>
		/// Cancellation token
		/// </summary>
		CancellationToken CancellationToken { get; }

		/// <summary>
		/// Result of the import
		/// </summary>
		ImportResult Result { get; set; }

		/// <summary>
		/// Indicates whether and how to abort the import
		/// </summary>
		DataExchangeAbortion Abort { get; set; }

		/// <summary>
		/// Creates a segmenter instance
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		ImportDataSegmenter<TEntity> GetSegmenter<TEntity>() where TEntity : BaseEntity;

		/// <summary>
		/// Allows to set a progress message
		/// </summary>
		/// <param name="value">Progress value</param>
		/// /// <param name="maximum">Progress maximum</param>
		void SetProgress(int value, int maximum);
	}


	public class ImportExecuteContext : IImportExecuteContext
	{
		private DataExchangeAbortion _abortion;
		private ProgressValueSetter _progressValueSetter;
		private string _progressInfo;

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

		public IDataTable DataTable { get; internal set; }

		public ColumnMap ColumnMap { get; internal set; }

		public bool UpdateOnly { get; internal set; }

		public string[] KeyFieldNames { get; internal set; }

		public ILogger Log { get; internal set; }

		public CancellationToken CancellationToken { get; private set; }

		public string ImportFolder { get; internal set; }

		/// <summary>
		/// Use this dictionary for any custom data required along the import
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }

		public ImportResult Result { get; set; }

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

		public ImportDataSegmenter<TEntity> GetSegmenter<TEntity>() where TEntity : BaseEntity
		{
			return new ImportDataSegmenter<TEntity>(DataTable, ColumnMap);
		}

		public void SetProgress(int value, int maximum)
		{
			try
			{
				if (_progressValueSetter != null)
					_progressValueSetter.Invoke(value, maximum, _progressInfo.FormatInvariant(value, maximum));
			}
			catch { }
		}
	}
}
