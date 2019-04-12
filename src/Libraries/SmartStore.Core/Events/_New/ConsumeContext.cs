using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Events
{
	//public class ConsumeContext
	//{
	//	protected ConsumeContext(object message)
	//	{
	//		Message = message;
	//	}

	//	public object Message { get; private set; }
	//}

	public class ConsumeContext<TMessage> //: ConsumeContext
	{
		public ConsumeContext(TMessage message)
			//: base(message)
		{
			Message = message;
		}

		public TMessage Message
		{
			get;
			private set;
		}
	}
}
