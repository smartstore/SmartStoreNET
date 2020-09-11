using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Tests
{
    public class MockProviderManager : IProviderManager
    {
        private IDictionary<ProviderMetadata, IProvider> _providers = new Dictionary<ProviderMetadata, IProvider>();

        public void RegisterProvider(string systemName, IProvider provider)
        {
            var metadata = new ProviderMetadata
            {
                SystemName = systemName
            };
            _providers[metadata] = provider;
        }

        public Provider<TProvider> GetProvider<TProvider>(string systemName, int storeId = 0) where TProvider : IProvider
        {
            return _providers
                .Where(x => x.Key.SystemName.IsCaseInsensitiveEqual(systemName))
                .Select(x => new Provider<TProvider>(new Lazy<TProvider, ProviderMetadata>(() => (TProvider)x.Value, x.Key)))
                .FirstOrDefault();
        }

        public Provider<IProvider> GetProvider(string systemName, int storeId = 0)
        {
            return _providers
                .Where(x => x.Key.SystemName.IsCaseInsensitiveEqual(systemName))
                .Select(x => new Provider<IProvider>(new Lazy<IProvider, ProviderMetadata>(() => (IProvider)x.Value, x.Key)))
                .FirstOrDefault();
        }

        public IEnumerable<Provider<TProvider>> GetAllProviders<TProvider>(int storeId = 0) where TProvider : IProvider
        {
            return _providers
                .Where(x => typeof(TProvider).IsAssignableFrom(x.Value.GetType()))
                .Select(x => new Provider<TProvider>(new Lazy<TProvider, ProviderMetadata>(() => (TProvider)x.Value, x.Key)));
        }

        public IEnumerable<Provider<IProvider>> GetAllProviders(int storeId = 0)
        {
            return _providers.Select(x => new Provider<IProvider>(new Lazy<IProvider, ProviderMetadata>(() => (IProvider)x.Value, x.Key)));
        }

        public void SetDisplayOrder(ProviderMetadata metadata, int displayOrder, int storeId = 0)
        {
        }

        protected virtual IEnumerable<Provider<TProvider>> SortProviders<TProvider>(IEnumerable<Provider<TProvider>> providers, int storeId = 0) where TProvider : IProvider
        {
            return providers.OrderBy(x => x.Metadata.DisplayOrder);
        }
    }
}
