using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Events
{
	public class ConsumerResolver : IConsumerResolver
	{
		public virtual IConsumer Resolve(ConsumerDescriptor descriptor)
		{
			if (descriptor.PluginDescriptor == null || IsActiveForStore(descriptor.PluginDescriptor))
			{
				return EngineContext.Current.ContainerManager.Scope().ResolveKeyed<IConsumer>(descriptor.ContainerType);
			}

			return null;
		}

		private bool IsActiveForStore(PluginDescriptor plugin)
		{
			var storeContext = EngineContext.Current.Resolve<IStoreContext>();

			int storeId = 0;
			if (EngineContext.Current.IsFullyInitialized)
			{
				storeId = storeContext.CurrentStore.Id;
			}

			if (storeId == 0)
			{
				return true;
			}

			var settingService = EngineContext.Current.Resolve<ISettingService>();

			var limitedToStoresSetting = settingService.GetSettingByKey<string>(plugin.GetSettingKey("LimitedToStores"));
			if (limitedToStoresSetting.IsEmpty())
			{
				return true;
			}

			var limitedToStores = limitedToStoresSetting.ToIntArray();
			if (limitedToStores.Length > 0)
			{
				var flag = limitedToStores.Contains(storeId);
				return flag;
			}

			return true;
		}
	}
}
