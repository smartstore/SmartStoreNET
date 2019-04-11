using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Events
{
	public class ConsumeContext<TMessage>  where TMessage : class
	{
		public ConsumeContext(TMessage message)
		{
			Message = message;
		}

		public TMessage Message { get; private set; }
	}
}
