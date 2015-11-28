using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Linq;
using EFCache;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Data.Setup;
using SmartStore.Data.Caching;

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

				// prepare EntityFramework 2nd level cache
				ICache cache = null;
				try
				{
					var innerCache = EngineContext.Current.Resolve<Func<Type, SmartStore.Core.Caching.ICache>>();
					cache = new EfCacheImpl(innerCache(typeof(SmartStore.Core.Caching.StaticCache)));
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
