using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Data.Hooks
{
	public class DefaultDbHookHandler : IDbHookHandler
	{
		private readonly IEnumerable<Lazy<IDbHook, HookMetadata>> _hooks;
		private readonly IList<Lazy<IDbHook, HookMetadata>> _loadHooks;
		private readonly IList<Lazy<IDbHook, HookMetadata>> _saveHooks;

		private readonly Multimap<Type, IDbLoadHook> _loadHooksRequestCache = new Multimap<Type, IDbLoadHook>();
		private readonly Multimap<Type, IDbSaveHook> _saveHooksRequestCache = new Multimap<Type, IDbSaveHook>();

		// Prevents repetitive hooking of the same entity/state/[pre|post] combination within a single request
		private readonly HashSet<HookedEntityKey> _hookedEntities = new HashSet<HookedEntityKey>();

		private readonly static ConcurrentDictionary<Type, bool> _hookableEntities = new ConcurrentDictionary<Type, bool>();
		private static HashSet<Type> _importantLoadHookTypes;
		private static HashSet<Type> _importantSaveHookTypes;
		private readonly static object _lock = new object();

		public DefaultDbHookHandler(IEnumerable<Lazy<IDbHook, HookMetadata>> hooks)
		{
			_hooks = hooks;
			_loadHooks = hooks.Where(x => x.Metadata.IsLoadHook == true).ToList();
			_saveHooks = hooks.Where(x => x.Metadata.IsLoadHook == false).ToList();
		}

		public ILogger Logger
		{
			get;
			set;
		}

		public bool HasImportantLoadHooks()
		{
			if (_importantLoadHookTypes == null)
			{
				lock (_lock)
				{
					if (_importantLoadHookTypes == null)
					{
						_importantLoadHookTypes = new HashSet<Type>();
						_importantLoadHookTypes.AddRange(_loadHooks.Where(x => x.Metadata.Important).Select(x => x.Metadata.ImplType));
					}
				}
			}

			return _importantLoadHookTypes.Any();
		}

		public bool HasImportantSaveHooks()
		{
			if (_importantSaveHookTypes == null)
			{
				lock (_lock)
				{
					if (_importantSaveHookTypes == null)
					{
						_importantSaveHookTypes = new HashSet<Type>();
						_importantSaveHookTypes.AddRange(_saveHooks.Where(x => x.Metadata.Important).Select(x => x.Metadata.ImplType));
					}
				}
			}

			return _importantSaveHookTypes.Any();
		}

		public void TriggerLoadHooks(BaseEntity entity, bool importantHooksOnly)
		{
			if (!_loadHooks.Any() || (importantHooksOnly && !this.HasImportantLoadHooks()))
			{
				return;
			}

			if (entity == null || !IsHookableEntity(entity))
			{
				return;
			}				

			var loadHooks = GetLoadHookInstancesFor(entity, importantHooksOnly);
			foreach (var hook in loadHooks)
			{
				// call hook
				try
				{
					hook.OnLoaded(entity);
				}
				catch (Exception ex)
				{
					Logger.ErrorFormat(ex, "LoadHook exception ({0})", hook.GetType().FullName);
				}
			}
		}

		public bool TriggerPreSaveHooks(IEnumerable<HookedEntity> entries, bool importantHooksOnly)
		{
			bool anyStateChanged = false;

			if (entries != null)
			{
				// Skip entities explicitly marked as unhookable
				entries = entries.Where(IsHookableEntry);
			}

			if (entries == null || !entries.Any() || !_saveHooks.Any() || (importantHooksOnly && !this.HasImportantSaveHooks()))
				return false;

			var processedHooks = new HashSet<IDbSaveHook>();

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var entity = e.Entity;
				if (HandledAlready(entity, e.InitialState, false))
				{
					// Prevent repetitive hooking of the same entity/state/pre combination within a single request
					continue;
				}
				var hooks = GetSaveHookInstancesFor(entity, importantHooksOnly);
				foreach (var hook in hooks)
				{
					// call hook
					try
					{
						//Logger.DebugFormat("PRE save hook: {0}, State: {1}, Entity: {2}", hook.GetType().Name, e.InitialState, e.Entity.GetUnproxiedType().Name);
						hook.OnBeforeSave(e);
						processedHooks.Add(hook);
					}
					catch (Exception ex)
					{
						Logger.ErrorFormat(ex, "PreSaveHook exception ({0})", hook.GetType().FullName);
					}			

					// change state if applicable
					if (e.HasStateChanged)
					{
						e.InitialState = e.State;
						anyStateChanged = true;
					}
				}
			}

			processedHooks.Each(x => x.OnBeforeSaveCompleted());

			return anyStateChanged;
		}

		public void TriggerPostSaveHooks(IEnumerable<HookedEntity> entries, bool importantHooksOnly)
		{
			if (entries != null)
			{
				// Skip entities explicitly marked as unhookable
				entries = entries.Where(IsHookableEntry);
			}

			if (entries == null || !entries.Any() || !_saveHooks.Any() || (importantHooksOnly && !this.HasImportantSaveHooks()))
				return;

			var processedHooks = new HashSet<IDbSaveHook>();

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var entity = e.Entity;
				if (HandledAlready(entity, e.InitialState, true))
				{
					// Prevent repetitive hooking of the same entity/state/post combination within a single request
					continue;
				}
				var postHooks = GetSaveHookInstancesFor(entity, importantHooksOnly);
				foreach (var hook in postHooks)
				{
					// call hook
					try
					{
						//Logger.DebugFormat("POST save hook: {0}, State: {1}, Entity: {2}", hook.GetType().Name, e.InitialState, e.Entity.GetUnproxiedType().Name);
						hook.OnAfterSave(e);
						processedHooks.Add(hook);
					}
					catch (Exception ex)
					{
						Logger.ErrorFormat(ex, "PostSaveHook exception ({0})", hook.GetType().FullName);
					}
				}
			}

			processedHooks.Each(x => x.OnAfterSaveCompleted());
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private IEnumerable<IDbLoadHook> GetLoadHookInstancesFor(BaseEntity entity, bool importantOnly)
		{
			return GetHookInstancesFor<IDbLoadHook>(entity, importantOnly,
				_loadHooks,
				_loadHooksRequestCache,
				_importantLoadHookTypes);
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private IEnumerable<IDbSaveHook> GetSaveHookInstancesFor(BaseEntity entity, bool importantOnly)
		{
			return GetHookInstancesFor<IDbSaveHook>(entity, importantOnly, 
				_saveHooks, 
				_saveHooksRequestCache, 
				_importantSaveHookTypes);
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private IEnumerable<THook> GetHookInstancesFor<THook>(
			BaseEntity entity, 
			bool importantOnly,
			IList<Lazy<IDbHook, HookMetadata>> hookList,
			Multimap<Type, THook> requestCache,
			HashSet<Type> importantTypes) where THook : IDbHook
		{
			if (entity == null)
			{
				return Enumerable.Empty<THook>();
			}			

			IEnumerable<THook> hooks;

			var hookedType = entity.GetUnproxiedType();

			if (requestCache.ContainsKey(hookedType))
			{
				hooks = requestCache[hookedType];
			}
			else
			{
				hooks = hookList.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => (THook)x.Value);
				requestCache.AddRange(hookedType, hooks);
			}

			if (importantOnly && hooks.Any())
			{
				hooks = hooks.Where(x => importantTypes.Contains(x.GetType()));
			}

			return hooks;
		}

		private bool IsHookableEntry(HookedEntity entry)
		{
			var entity = entry.Entity;
			if (entity == null)
			{
				return false;
			}

			return IsHookableEntity(entity);
		}

		private bool IsHookableEntity(BaseEntity entity)
		{
			var isHookable = _hookableEntities.GetOrAdd(entity.GetUnproxiedType(), t =>
			{
				var attr = t.GetAttribute<HookableAttribute>(true);
				if (attr != null)
				{
					return attr.IsHookable;
				}

				// Entities are hookable by default
				return true;
			});

			return isHookable;
		}

		private bool HandledAlready(BaseEntity entity, EntityState initialState, bool isPostSaveHook)
		{
			if (entity.IsTransientRecord())
				return false;

			var key = new HookedEntityKey(entity.GetUnproxiedType(), entity.Id, initialState, isPostSaveHook);
			if (_hookedEntities.Contains(key))
			{
				return true;
			}

			_hookedEntities.Add(key);
			return false;
		}

		class HookedEntityKey : Tuple<Type, int, EntityState, bool>
		{
			public HookedEntityKey(Type entityType, int entityId, EntityState initialState, bool isPostSaveHook)
				: base(entityType, entityId, initialState, isPostSaveHook)
			{
			}
		}
	}
}
