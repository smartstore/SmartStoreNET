using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core.Plugins;
using SmartStore.Services;

namespace SmartStore.Web.Framework.Plugins
{
	public partial class ProviderManager : IProviderManager
	{
		private readonly IComponentContext _ctx;
		private readonly ICommonServices _services;
		private readonly PluginMediator _pluginMediator;

		public ProviderManager(IComponentContext ctx, ICommonServices services, PluginMediator pluginMediator)
		{
			this._ctx = ctx;
			this._services = services;
			this._pluginMediator = pluginMediator;
		}

		public Provider<TProvider> GetProvider<TProvider>(string systemName, int storeId = 0) where TProvider : IProvider
		{
			if (systemName.IsEmpty())
				return null;

			var provider = _ctx.ResolveOptionalNamed<Lazy<TProvider, ProviderMetadata>>(systemName);
			if (provider != null)
			{
				if (storeId > 0)
				{
					var d = provider.Metadata.PluginDescriptor;
					if (d != null && !IsActiveForStore(d, storeId))
					{
						return null;
					}
				}
				SetUserData(provider.Metadata);
				return new Provider<TProvider>(provider);
			}

			return null;
		}

		public Provider<IProvider> GetProvider(string systemName, int storeId = 0)
		{
			Guard.NotEmpty(systemName, nameof(systemName));

			var provider = _ctx.ResolveOptionalNamed<Lazy<IProvider, ProviderMetadata>>(systemName);
			if (provider != null)
			{
				if (storeId > 0)
				{
					var d = provider.Metadata.PluginDescriptor;
					if (d != null && !IsActiveForStore(d, storeId))
					{
						return null;
					}
				}
				SetUserData(provider.Metadata);
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
							where d == null || IsActiveForStore(d, storeId)
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
							where d == null || IsActiveForStore(d, storeId)
							select p;
			}
			return SortProviders(providers.Select(x => new Provider<IProvider>(x)));
		}

		protected virtual IEnumerable<Provider<TProvider>> SortProviders<TProvider>(IEnumerable<Provider<TProvider>> providers) where TProvider : IProvider
		{
			foreach (var m in providers.Select(x => x.Metadata))
			{
				SetUserData(m);
			}

			return providers.OrderBy(x => x.Metadata.DisplayOrder).ThenBy(x => x.Metadata.FriendlyName);
		}

		protected virtual void SetUserData(ProviderMetadata metadata)
		{
			if (!metadata.IsEditable)
				return;

			var displayOrder = _pluginMediator.GetUserDisplayOrder(metadata);
			var name = _pluginMediator.GetSetting<string>(metadata, "FriendlyName");
			var description = _pluginMediator.GetSetting<string>(metadata, "Description");
			metadata.FriendlyName = name;
			metadata.Description = description;

			if (displayOrder.HasValue)
			{
				metadata.DisplayOrder = displayOrder.Value;
			}
		}

		private bool IsActiveForStore(PluginDescriptor plugin, int storeId)
		{
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
