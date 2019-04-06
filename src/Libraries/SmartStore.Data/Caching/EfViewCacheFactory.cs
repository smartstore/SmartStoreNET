using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Mapping;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Collections.Concurrent;

namespace SmartStore.Data.Caching
{
	public class EfViewCacheFactory : DbMappingViewCacheFactory
	{
		private readonly static ConcurrentDictionary<DbMappingViewCacheFactory, StorageMappingItemCollection> _lookup =
			new ConcurrentDictionary<DbMappingViewCacheFactory, StorageMappingItemCollection>();

		public static void SetContext<TContext>()
			where TContext : DbContext, new()
		{
			using (var ctx = new TContext())
			{
				SetContext(ctx);
			}
		}

		/// <summary>
		/// Sets a <paramref name="viewCacheFactory"/> for a mapping represented 
		/// by a <see cref="DbContext"/> derived class.
		/// </summary>
		/// <param name="context">A <see cref="DbContext"/> derived class instance containing 
		/// mapping to set the view cache factory for.</param>
		/// <param name="viewCacheFactory">View cache factory</param>
		/// <remarks>
		/// This method must be called before EntityFramework generates views for the mapping 
		/// (which typically happens on the first query).
		/// <paramref name="viewCacheFactory"/> cannot be set more than once for the same 
		/// <paramref name="context"/>.
		/// </remarks>
		public static void SetContext(DbContext ctx)
		{
			Guard.NotNull(ctx, nameof(ctx));

			var objectContext = ((IObjectContextAdapter)ctx).ObjectContext;
			var itemCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
			var factory = new EfViewCacheFactory();

			itemCollection.MappingViewCacheFactory = factory;
			_lookup.TryAdd(factory, itemCollection);
		}

		/// <summary>
		/// Returns a <see cref="StorageMappingItemCollection"/> instance for the <paramref name="viewCacheFactory" />.
		/// </summary>
		/// <param name="viewCacheFactory">View cache factory to return <see cref="StorageMappingItemCollection"/> for.</param>
		/// <returns>A <see cref="StorageMappingItemCollection"/> instance for the <paramref name="viewCacheFactory" />.</returns>
		public static StorageMappingItemCollection GetMappingItemCollection(DbMappingViewCacheFactory viewCacheFactory)
		{
			Guard.NotNull(viewCacheFactory, nameof(viewCacheFactory));

			if (!_lookup.TryGetValue(viewCacheFactory, out var mappingItemCollection))
			{
				throw new InvalidOperationException("No StorageMappingItemCollection instance found for the provided DbMappingViewCacheFactory.");
			}

			return mappingItemCollection;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DbMappingViewCache"/> class for 
		/// the given <paramref name="conceptualModelContainerName"/> and <paramref name="storeModelContainerName"/>.
		/// </summary>
		/// <param name="conceptualModelContainerName">The name of the conceptual model container.</param>
		/// <param name="storeModelContainerName">The name of the store model container.</param>
		/// <returns>A new instance of the <see cref="DbMappingViewCache"/> class.</returns>
		public override DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName)
		{
			var mappingItemCollection = GetMappingItemCollection();

			if (mappingItemCollection == null)
			{
				throw new InvalidOperationException("View cache not set for this mapping item collection");
			}

			var hash = mappingItemCollection.ComputeMappingHashValue(conceptualModelContainerName, storeModelContainerName);

			//var viewsXml = Load(conceptualModelContainerName, storeModelContainerName);
			//if (viewsXml != null)
			//{
			//	var viewsForMapping = GetViewsForMapping(viewsXml, conceptualModelContainerName, storeModelContainerName);

			//	if (viewsForMapping != null && (string)viewsForMapping.Attribute("hash") == hash)
			//	{
			//		return new MappingViewCache(viewsForMapping);
			//	}
			//}

			var views = GenerateViews(mappingItemCollection, conceptualModelContainerName, storeModelContainerName);

			//viewsXml = CreateOrUpdateViewsXml(viewsXml, hash, conceptualModelContainerName, storeModelContainerName, views);
			//Save(viewsXml);

			return new EfMappingViewCache(hash, views);
		}

		// virtual for mocking
		internal virtual StorageMappingItemCollection GetMappingItemCollection()
		{
			return GetMappingItemCollection(this);
		}

		// virtual for mocking
		internal virtual Dictionary<EntitySetBase, DbMappingView> GenerateViews(
			StorageMappingItemCollection mappingItemCollection, 
			string conceptualModelContainerName, 
			string storeModelContainerName)
		{
			var errors = new List<EdmSchemaError>();
			return mappingItemCollection.GenerateViews(conceptualModelContainerName, storeModelContainerName, errors);
		}
	}
}
