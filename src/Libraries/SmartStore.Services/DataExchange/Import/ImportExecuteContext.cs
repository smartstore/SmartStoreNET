using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Import
{
	public interface IImportExecuteContext
	{
		/// <summary>
		/// Context customer identifier
		/// </summary>
		int CustomerId { get; }

		/// <summary>
		/// Use this dictionary for any custom data required along the export
		/// </summary>
		Dictionary<string, object> CustomProperties { get; set; }

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
		/// Whether the data source contains a particular column
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		bool HasColumn(string name);

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
		private CancellationToken _cancellation;
		private ProgressValueSetter _progressValueSetter;
		private string _progressInfo;

		public ImportExecuteContext(
			CancellationToken cancellation,
			ProgressValueSetter progressValueSetter,
			string progressInfo)
		{
			_cancellation = cancellation;
			_progressValueSetter = progressValueSetter;
			_progressInfo = progressInfo;

			CustomProperties = new Dictionary<string, object>();
			Result = new ImportResult();
		}

		public IDataTable DataTable { get; internal set; }

		public ColumnMap ColumnMap { get; internal set; }

		public int CustomerId { get; internal set; }

		/// <summary>
		/// Use this dictionary for any custom data required along the import
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }

		public ImportResult Result { get; set; }

		public DataExchangeAbortion Abort
		{
			get
			{
				if (_cancellation.IsCancellationRequested || IsMaxFailures)
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
				return Result.Messages.Count(x => x.MessageType == ImportMessageType.Error) > 11;
			}
		}

		public ImportDataSegmenter<TEntity> GetSegmenter<TEntity>() where TEntity : BaseEntity
		{
			return new ImportDataSegmenter<TEntity>(DataTable, ColumnMap);
		}

		public bool HasColumn(string name)
		{
			return DataTable.HasColumn(name);
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
