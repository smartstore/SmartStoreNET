using System;
using System.Collections.Generic;
using System.Threading;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Import
{
	public interface IEntityImporter
	{
		void Execute(ImportExecuteContext context);
	}


	public class ImportExecuteContext
	{
		private DataExchangeAbortion _abortion;
		private CancellationToken _cancellation;
		private ProgressValueSetter _progressValueSetter;
		private string _progressInfo;

		public ImportExecuteContext(
			CancellationToken cancellation,
			int customerId,
			ProgressValueSetter progressValueSetter,
			string progressInfo)
		{
			_cancellation = cancellation;
			_progressValueSetter = progressValueSetter;
			_progressInfo = progressInfo;

			CustomerId = customerId;
			CustomProperties = new Dictionary<string, object>();
		}

		public ILogger Log { get; internal set; }

		public IDataTable DataTable { get; internal set; }

		public int CustomerId { get; private set; }

		/// <summary>
		/// Use this dictionary for any custom data required along the import
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; private set; }

		public int RecordsAdded { get; set; }
		public int RecordsUpdated { get; set; }
		public int RecordsFailed { get; set; }

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
			get { return RecordsFailed > 11; }
		}

		/// <summary>
		/// Processes an exception that occurred while importing a record
		/// </summary>
		/// <param name="exception">Exception</param>
		public void RecordException(Exception exception, string id)
		{
			++RecordsFailed;

			Log.Error("Error while processing record with id {0}: {1}".FormatInvariant(id, exception.ToAllMessages()), exception);
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
