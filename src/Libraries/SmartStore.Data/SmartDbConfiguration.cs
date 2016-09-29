using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Linq;
//using EFCache;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
//using SmartStore.Data.Caching;
using SmartStore.Data.Caching2;
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
					IDbCache cache = null;
					try
					{
						var innerCache = EngineContext.Current.Resolve<ICacheManager>();
						cache = new DbCache(innerCache);
					}
					catch
					{
						cache = new NullDbCache();
					}

					var cacheInterceptor = new CacheTransactionInterceptor(cache);
					AddInterceptor(cacheInterceptor);

					Loaded +=
					  (sender, args) => args.ReplaceService<DbProviderServices>(
						(s, _) => new CachingProviderServices(s, cacheInterceptor));
				}
			}
		}
	}
}
