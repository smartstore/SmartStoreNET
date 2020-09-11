using System;
using System.Linq;
using System.Reflection;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Events
{
    public class ConsumerResolver : IConsumerResolver
    {
        private readonly Work<IComponentContext> _container;

        public ConsumerResolver(Work<IComponentContext> container)
        {
            _container = container;
        }

        public virtual IConsumer Resolve(ConsumerDescriptor descriptor)
        {
            if (descriptor.PluginDescriptor == null || IsActiveForStore(descriptor.PluginDescriptor))
            {
                return _container.Value.ResolveKeyed<IConsumer>(descriptor.ContainerType);
            }

            return null;
        }

        public virtual object ResolveParameter(ParameterInfo p, IComponentContext c = null)
        {
            return (c ?? _container.Value).Resolve(p.ParameterType);
        }

        private bool IsActiveForStore(PluginDescriptor plugin)
        {
            int storeId = 0;
            if (EngineContext.Current.IsFullyInitialized)
            {
                storeId = _container.Value.Resolve<IStoreContext>().CurrentStore.Id;
            }

            if (storeId == 0)
            {
                return true;
            }

            var settingService = _container.Value.Resolve<ISettingService>();

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
