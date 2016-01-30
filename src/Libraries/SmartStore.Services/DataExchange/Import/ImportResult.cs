using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SmartStore.Services.DataExchange.Import
{
	public class ImportResult : ICloneable<SerializableImportResult>
	{
		public ImportResult()
		{
			this.Messages = new List<ImportMessage>();
			Clear();
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

		public int SkippedRecords
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

		public void Clear()
		{
			Messages.Clear();
			StartDateUtc = EndDateUtc = DateTime.UtcNow;
			TotalRecords = 0;
			SkippedRecords = 0;
			NewRecords = 0;
			ModifiedRecords = 0;
			Cancelled = false;
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

			return this.AddMessage(msg, ImportMessageType.Error, fullMessage: exception.StackTrace);
		}

		public ImportMessage AddError(Exception exception, string message)
		{
			return AddMessage(
				message ?? exception.ToAllMessages(),
				ImportMessageType.Error,
				null,
				null,
				exception.StackTrace);
		}

		public ImportMessage AddMessage(string message, ImportMessageType severity, ImportRowInfo affectedRow = null, string affectedField = null, string fullMessage = null)
		{
			var msg = new ImportMessage(message, severity);

			msg.AffectedItem = affectedRow;
			msg.AffectedField = affectedField;
			msg.FullMessage = fullMessage;

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

		public int Warnings
		{
			get { return Messages.Count(x => x.MessageType == ImportMessageType.Warning); }
		}

		public bool HasErrors
		{
			get { return this.Messages.Any(x => x.MessageType == ImportMessageType.Error); }
		}

		public int Errors
		{
			get { return Messages.Count(x => x.MessageType == ImportMessageType.Error); }
		}

		public string LastError
		{
			get
			{
				var lastError = Messages.LastOrDefault(x => x.MessageType == ImportMessageType.Error);
				if (lastError != null)
					return lastError.Message;

				return null;
			}
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		public SerializableImportResult Clone()
		{
			var result = new SerializableImportResult();
			result.StartDateUtc = StartDateUtc;
			result.EndDateUtc = EndDateUtc;
			result.TotalRecords = TotalRecords;
			result.SkippedRecords = SkippedRecords;
			result.NewRecords = NewRecords;
			result.ModifiedRecords = ModifiedRecords;
			result.AffectedRecords = AffectedRecords;
			result.Cancelled = Cancelled;
			result.Warnings = Warnings;
			result.Errors = Errors;
			result.LastError = LastError;

			return result;
		}
	}


	[Serializable]
	public partial class SerializableImportResult
	{
		public DateTime StartDateUtc { get; set; }
		public DateTime EndDateUtc { get; set; }
		public int TotalRecords { get; set; }
		public int SkippedRecords { get; set; }
		public int NewRecords { get; set; }
		public int ModifiedRecords { get; set; }
		public int AffectedRecords { get; set; }
		public bool Cancelled { get; set; }
		public int Warnings { get; set; }
		public int Errors { get; set; }
		public string LastError { get; set; }
	}
}
