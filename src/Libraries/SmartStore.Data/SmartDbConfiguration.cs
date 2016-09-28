using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Linq;
using EFCache;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Data.Caching;
using SmartStore.Core.Caching;
using System.Web.Hosting;

namespace SmartStore.Data
{
	public class SmartDbConfiguration : DbConfiguration
	{
		public SmartDbConfiguration()
		{
			IEfDataProvider provider = null;
			try
			{
				provider = (new EfDataProviderFactory(DataSettings.Current).LoadDataProvider()) as IEfDataProvider;
			}
			catch { /* SmartStore is not installed yet! */ }

			if (provider != null)
			{
				base.SetDefaultConnectionFactory(provider.GetConnectionFactory());

				if (HostingEnvironment.IsHosted)
				{
					// prepare EntityFramework 2nd level cache
					ICache cache = null;
					try
					{
						var innerCache = EngineContext.Current.Resolve<ICacheManager>();
						if (innerCache.IsDistributedCache)
						{
							// fuckin' EfCache puts internal, unserializable objects to the cache!!!
							innerCache = EngineContext.Current.Resolve<ICacheManager>("memory");
						}
						cache = new EfCacheImpl(innerCache);
					}
					catch
					{
						cache = new InMemoryCache();
					}

					var transactionHandler = new CacheTransactionHandler(cache);
					AddInterceptor(transactionHandler);

					Loaded +=
					  (sender, args) => args.ReplaceService<DbProviderServices>(
						(s, _) => new CachingProviderServices(s, transactionHandler,
						  new EfCachingPolicy()));
				}
			}
		}
	}
}
