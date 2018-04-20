using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Topics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.DevTools.Examples
{
	public static class OutputCacheInvalidationObserver
	{
		public static void Execute()
		{
			var observer = EngineContext.Current.Resolve<IOutputCacheInvalidationObserver>();
			observer.ObserveEntity(Observe);
		}

		private static void Observe(ObserveEntityContext ctx)
		{
			// If your plugin renders dynamic content to the frontend which won't change often once it is configured, you should add a corresponding route 
			// to the ouput cache to add the rendered output of the plugin to the cache. For more information on this see the Install/Uninstall method of this plugin

			// In the example below we assume the plugin has own entities which depend on system entities like Product, Category or Topic.
			// In this case you must invalidate the cache items for the system entities as soon as the corresponding plugin entity is changed by the shop admin 
			// in order to remove the cached output of the affected pages in the frontend and thus be rebuilt

			var myRecord = ctx?.Entity as MyRecord;

			if (myRecord != null)
			{
				IEnumerable<string> tags = null;

				var outputCacheProvider = ctx.OutputCacheProvider;
				var displayControl = ctx.ServiceContainer.Resolve<ICommonServices>().DisplayControl;

				// We assume the domain record from the plugin stores information about the type of the entity in EntityName and the corresponding Id in EntityId
				var entityname = myRecord.EntityName;

				// Collect the tags for the entities which must be invalidated due to changes of the plugin domain record
				switch (entityname.ToLower())
				{
					case "product":
						var product = ctx.ServiceContainer.Resolve<IProductService>().GetProductById(myRecord.EntityId);
						if (product != null) tags = displayControl.GetCacheControlTagsFor(product);
						break;
					case "category":
						var category = ctx.ServiceContainer.Resolve<ICategoryService>().GetCategoryById(myRecord.EntityId);
						if (category != null) tags = displayControl.GetCacheControlTagsFor(category);
						break;
					case "topic":
						var topic = ctx.ServiceContainer.Resolve<ITopicService>().GetTopicById(myRecord.EntityId);
						if (topic != null) tags = displayControl.GetCacheControlTagsFor(topic);
						break;
				}

				// Invalidate cache items by the collected tags
				if (tags != null && tags.Any())
				{
					outputCacheProvider.InvalidateByTag(tags.ToArray());
					ctx.Handled = true;
				}
			}
		}

		internal class MyRecord : BaseEntity
		{
			public int EntityId
			{
				get { return 0; }
			}

			public string EntityName
			{
				get { return String.Empty; }
			}
		}
	}
}