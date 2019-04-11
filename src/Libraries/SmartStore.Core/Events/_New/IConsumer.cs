using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Events
{
	public interface IConsumer
	{
	}

	public class TestConsumer : IConsumer
	{
		public void Handle(AppStartedEvent e)
		{
		}

		public void Consume(ConsumeContext<AppInitScheduledTasksEvent> e)
		{
		}

		public void HandleEvent(AppRegisterGlobalFiltersEvent e)
		{
		}
	}
}
