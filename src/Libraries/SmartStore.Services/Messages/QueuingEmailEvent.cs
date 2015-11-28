using System.Collections.Generic;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
	/// <summary>
	/// An event message, which gets published just before a new instance
	/// of <see cref="QueuedEmail"/> is persisted to the database
	/// </summary>
	public class QueuingEmailEvent
	{
		public QueuedEmail QueuedEmail 
		{ 
			get; 
			set; 
		}

		public MessageTemplate MessageTemplate
		{
			get;
			set;
		}

		public EmailAccount EmailAccount
		{
			get;
			set;
		}

		public IList<Token> Tokens
		{
			get;
			set;
		}

		public int LanguageId
		{
			get;
			set;
		}
	}
}
