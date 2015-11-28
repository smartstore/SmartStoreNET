using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Data
{
	
	public class ImportMessage
	{
		public ImportMessage(string message, ImportMessageType messageType = ImportMessageType.Info)
		{
			Guard.ArgumentNotEmpty(() => message);

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

		public override string ToString()
		{
			return "{0} - {1}".FormatCurrent(MessageType.ToString().ToUpper(), Message.EmptyNull());
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
