using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Plugins
{
	public interface IProviderManager
	{
		Provider<TProvider> GetProvider<TProvider>(string systemName, int storeId = 0) where TProvider : IProvider;

		Provider<IProvider> GetProvider(string systemName, int storeId = 0);

		IEnumerable<Provider<TProvider>> GetAllProviders<TProvider>(int storeId = 0) where TProvider : IProvider;

		IEnumerable<Provider<IProvider>> GetAllProviders(int storeId = 0);

		void SetDisplayOrder(ProviderMetadata metadata, int displayOrder, int storeId = 0);

	}

	public static class IProviderManagerExtensions
	{
		public static void SetDisplayOrder(this IProviderManager manager, string systemName, int displayOrder, int storeId = 0)
		{
			Guard.ArgumentNotEmpty(() => systemName);

			var provider = manager.GetProvider(systemName, storeId);
			if (provider != null)
			{
				manager.SetDisplayOrder(provider.Metadata, displayOrder, storeId);
			}
		}
	}
}
