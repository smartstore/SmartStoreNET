using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services;

namespace SmartStore.Services.Events
{
	public class ConsumerResolver : IConsumerResolver
	{
		private readonly ICommonServices _services;

		public ConsumerResolver(ICommonServices services)
		{
			_services = services;
		}

		public virtual IConsumer Resolve(ConsumerDescriptor descriptor)
		{
			if (descriptor.PluginDescriptor == null || IsActiveForStore(descriptor.PluginDescriptor))
			{
				return _services.Container.ResolveKeyed<IConsumer>(descriptor.ContainerType);
			}

			return null;
		}

		private bool IsActiveForStore(PluginDescriptor plugin)
		{
			int storeId = 0;
			if (EngineContext.Current.IsFullyInitialized)
			{
				storeId = _services.StoreContext.CurrentStore.Id;
			}

			if (storeId == 0)
			{
				return true;
			}

			var limitedToStoresSetting = _services.Settings.GetSettingByKey<string>(plugin.GetSettingKey("LimitedToStores"));
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
