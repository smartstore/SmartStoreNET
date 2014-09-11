using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core.Caching;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services;

namespace SmartStore.Web.Framework.Plugins
{
	
	public partial class ProviderManager : IProviderManager
	{
		private readonly IComponentContext _ctx;
		private readonly ICommonServices _services;

		public ProviderManager(IComponentContext ctx, ICacheManager cacheManager, ICommonServices services)
		{
			this._ctx = ctx;
			this._services = services;
		}

		public Provider<TProvider> GetProvider<TProvider>(string systemName, int storeId = 0) where TProvider : IProvider
		{
			Guard.ArgumentNotEmpty(() => systemName);

			var provider = _ctx.ResolveOptionalNamed<Lazy<TProvider, ProviderMetadata>>(systemName);
			if (provider != null)
			{
				if (storeId > 0)
				{
					var d = provider.Metadata.PluginDescriptor;
					if (d != null && _services.Settings.GetSettingByKey<string>(d.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true))
					{
						return null;
					}
				}
				return new Provider<TProvider>(provider);
			}
			return null;
		}

		public Provider<IProvider> GetProvider(string systemName, int storeId = 0)
		{
			Guard.ArgumentNotEmpty(() => systemName);

			var provider = _ctx.ResolveOptionalNamed<Lazy<IProvider, ProviderMetadata>>(systemName);
			if (provider != null)
			{
				if (storeId > 0)
				{
					var d = provider.Metadata.PluginDescriptor;
					if (d != null && _services.Settings.GetSettingByKey<string>(d.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true))
					{
						return null;
					}
				}
				return new Provider<IProvider>(provider);
			}
			return null;
		}

		public IEnumerable<Provider<TProvider>> GetAllProviders<TProvider>(int storeId = 0) where TProvider : IProvider
		{
			var providers = _ctx.Resolve<IEnumerable<Lazy<TProvider, ProviderMetadata>>>();
			if (storeId > 0)
			{
				providers = from p in providers
							let d = p.Metadata.PluginDescriptor
							where d == null || _services.Settings.GetSettingByKey<string>(d.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true)
							select p;
			}
			return SortProviders(providers.Select(x => new Provider<TProvider>(x)));
		}

		public IEnumerable<Provider<IProvider>> GetAllProviders(int storeId = 0)
		{
			var providers = _ctx.Resolve<IEnumerable<Lazy<IProvider, ProviderMetadata>>>();
			if (storeId > 0)
			{
				providers = from p in providers
							let d = p.Metadata.PluginDescriptor
							where d == null || _services.Settings.GetSettingByKey<string>(d.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true)
							select p;
			}
			return providers.Select(x => new Provider<IProvider>(x));
		}

		protected virtual IEnumerable<Provider<TProvider>> SortProviders<TProvider>(IEnumerable<Provider<TProvider>> providers, int storeId = 0) where TProvider : IProvider
		{
			string cacheKey = "sm.providers.displayorder." + typeof(TProvider).Name;
			_services.Cache.Get(cacheKey, () => {
				// cache serves just as a sort of static initializer
				foreach (var m in providers.Select(x => x.Metadata))
				{
					var mediator = _ctx.Resolve<PluginMediator>();
					var userDisplayOrder = mediator.GetSetting<int?>(m, "DisplayOrder");
					if (userDisplayOrder.HasValue)
					{
						m.DisplayOrder = userDisplayOrder.Value;
					}
				}
				return true;
			});

			return providers.OrderBy(x => x.Metadata.DisplayOrder);
		}

	}

}
