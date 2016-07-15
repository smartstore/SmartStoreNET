using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace SmartStore.Data.Caching2
{
	public static class ObjectContextExtensions
	{
		public static CachingProviderServices GetCachingProviderServices(this ObjectContext context)
		{
			Guard.NotNull(context, nameof(context));

			var providerInvariantName =
				((StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace))
					.ProviderInvariantName;
			return
				DbConfiguration.DependencyResolver.GetService(typeof(DbProviderServices), providerInvariantName) as
					CachingProviderServices;
		}
	}
}
