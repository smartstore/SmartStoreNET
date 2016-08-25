using System;

namespace SmartStore.Services.DataExchange.Import
{
	
	public class ImportMessage
	{
		public ImportMessage(string message, ImportMessageType messageType = ImportMessageType.Info)
		{
			Guard.NotEmpty(message, nameof(message));

			this.Message = message;
			this.MessageType = messageType;
		}
		
		public ImportRowInfo AffectedItem
		{
			get;
			set;
		}

		public string AffectedField
		{
			get;
			set;
		}

		public ImportMessageType MessageType
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public string FullMessage
		{
			get;
			set;
		}

		public override string ToString()
		{
			var result = Message.NaIfEmpty();

			string appendix = null;

			if (AffectedItem != null)
				appendix = appendix.Grow("Pos: " + (AffectedItem.Position + 1).ToString(), ", ");

			if (AffectedField.HasValue())
				appendix = appendix.Grow("Field: " + AffectedField, ", ");

			if (appendix.HasValue())
				result = "{0} [{1}]".FormatInvariant(result, appendix);

			return result;
		}
	}

	public class ImportRowInfo : Tuple<int, string>
	{
		public ImportRowInfo(int position, string entityName) 
			: base(position, entityName)
		{
		}

		public int Position
		{
			get { return base.Item1; }
		}

		public string EntityName
		{
			get { return base.Item2; }
		}
	}

	public enum ImportMessageType
	{
		Info = 0,
		Warning = 5,
		Error = 10
	}

}
