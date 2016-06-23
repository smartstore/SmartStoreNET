using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;

namespace SmartStore.DevTools.OutputCache
{
	public class OutputCacheInvalidationHook : PostActionHook<BaseEntity>
	{
		private readonly MemoryOutputCacheProvider _outputCache;
		private readonly IDisplayedEntities _displayedEntities;
		private readonly OutputCacheSettings _settings;

		public OutputCacheInvalidationHook(IDisplayedEntities displayedEntities)
		{
			_outputCache = new MemoryOutputCacheProvider();
			_displayedEntities = displayedEntities;
			_settings = new OutputCacheSettings();
		}

		public override EntityState HookStates
		{
			get
			{
				return EntityState.Modified; // TODO
			}
		}

		public override void Hook(BaseEntity entity, HookEntityMetadata metadata)
		{
			if (!_settings.AutomaticInvalidationEnabled)
				return;

			var tag = _displayedEntities.GetCacheControlTagFor(entity);
			if (tag != null)
			{
				_outputCache.InvalidateByTag(tag);
			}
		}
	}
}