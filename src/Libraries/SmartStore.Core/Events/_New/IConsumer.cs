using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SmartStore.Core.Events
{
	public interface IConsumer
	{
	}

	public class TestConsumer : IConsumer
	{
		[FireForget]
		public void Handle(AppStartedEvent e)
		{
			Thread.Sleep(300);
		}

		public void Consume(ConsumeContext<AppInitScheduledTasksEvent> e)
		{
			//throw new NotImplementedException();
		}

		public async Task HandleEvent(AppRegisterGlobalFiltersEvent e)
		{
			var ctx = HttpContext.Current;
			await Task.Delay(750);
			ctx = HttpContext.Current;
			//throw new NotSupportedException();
		}
	}
}
