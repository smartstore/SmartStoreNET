using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using SmartStore.Core.Domain.DataExchange;

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
			Result = new ImportResult();
		}

		public IDataTable DataTable { get; internal set; }

		public int CustomerId { get; private set; }

		/// <summary>
		/// Use this dictionary for any custom data required along the import
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; private set; }

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
