using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.ComponentModel;
using SmartStore.Core.Logging;
using SmartStore.Services;

namespace SmartStore.Core.Events
{
	public interface IEventPublisher
	{
		void Publish<T>(T message) where T : class;
	}
}
