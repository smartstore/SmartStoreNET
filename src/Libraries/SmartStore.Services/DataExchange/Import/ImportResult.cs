using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Services.DataExchange.Import
{
	
	public class ImportResult
	{

		public ImportResult()
		{
			this.Messages = new List<ImportMessage>();
			Clear();
		}

		public void Clear()
		{
			Messages.Clear();
			StartDateUtc = EndDateUtc = DateTime.UtcNow;
			TotalRecords = 0;
			NewRecords = 0;
			ModifiedRecords = 0;
			Cancelled = false;
		}

		public DateTime StartDateUtc
		{
			get;
			set;
		}

		public DateTime EndDateUtc
		{
			get;
			set;
		}

		public int TotalRecords
		{
			get;
			set;
		}

		public int NewRecords
		{
			get;
			set;
		}

		public int ModifiedRecords
		{
			get;
			set;
		}

		public int AffectedRecords
		{
			get { return NewRecords + ModifiedRecords; }
		}

		public bool Cancelled
		{
			get;
			set;
		}

		public ImportMessage AddInfo(string message, ImportRowInfo affectedRow = null, string affectedField = null)
		{
			return this.AddMessage(message, ImportMessageType.Info, affectedRow, affectedField);
		}

		public ImportMessage AddWarning(string message, ImportRowInfo affectedRow = null, string affectedField = null)
		{
			return this.AddMessage(message, ImportMessageType.Warning, affectedRow, affectedField);
		}

		public ImportMessage AddError(string message, ImportRowInfo affectedRow = null, string affectedField = null)
		{
			return this.AddMessage(message, ImportMessageType.Error, affectedRow, affectedField);
		}

		public ImportMessage AddError(Exception exception, int? affectedBatch = null, string stage = null)
		{
			var prefix = new List<string>();
			if (affectedBatch.HasValue)
			{
				prefix.Add("Batch: " + affectedBatch.Value);
			}
			if (stage.HasValue())
			{
				prefix.Add("Stage: " + stage);
			}

			string msg = string.Empty;
			if (prefix.Any())
			{
				msg = "[{0}] ".FormatCurrent(String.Join(", ", prefix));
			}

			msg += exception.ToAllMessages();

			return this.AddMessage(msg, ImportMessageType.Error);
		}

		public ImportMessage AddMessage(string message, ImportMessageType severity, ImportRowInfo affectedRow = null, string affectedField = null)
		{
			var msg = new ImportMessage(message, severity);

			msg.AffectedItem = affectedRow;
			msg.AffectedField = affectedField;

			this.Messages.Add(msg);
			return msg;
		}

		public IList<ImportMessage> Messages
		{
			get;
			private set;
		}

		public bool HasWarnings
		{
			get { return this.Messages.Any(x => x.MessageType == ImportMessageType.Warning); }
		}

		public bool HasErrors
		{
			get { return this.Messages.Any(x => x.MessageType == ImportMessageType.Error); }
		}

	}

}
