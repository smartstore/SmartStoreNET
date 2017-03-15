﻿using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Data.Caching;
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
			catch
			{
				/* SmartStore is not installed yet! */
			}

			if (provider != null)
			{
				base.SetDefaultConnectionFactory(provider.GetConnectionFactory());

				if (HostingEnvironment.IsHosted && DataSettings.DatabaseIsInstalled())
				{
					// TODO: (mc) Either CacheTransactionInterceptor or CachingProviderServices has serious mem leaks.
					// Investigate, resolve and activate again!

					//// prepare EntityFramework 2nd level cache
					//IDbCache cache = null;
					//try
					//{
					//	cache = EngineContext.Current.Resolve<IDbCache>();
					//}
					//catch
					//{
					//	cache = new NullDbCache();
					//}

					//var cacheInterceptor = new CacheTransactionInterceptor(cache);
					//AddInterceptor(cacheInterceptor);

					//Loaded +=
					//  (sender, args) => args.ReplaceService<DbProviderServices>((s, _) => new CachingProviderServices(s, cacheInterceptor));
				}
			}
		}
	}
}
