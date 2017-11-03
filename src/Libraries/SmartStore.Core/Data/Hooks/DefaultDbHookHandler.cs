using System;
using System.Collections.Generic;
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

		private readonly Multimap<RequestHookKey, IDbHook> _hooksRequestCache = new Multimap<RequestHookKey, IDbHook>();

		// Prevents repetitive hooking of the same entity/state/[pre|post] combination within a single request
		private readonly HashSet<HookedEntityKey> _hookedEntities = new HashSet<HookedEntityKey>();

		private static HashSet<Type> _importantLoadHookTypes;
		private static HashSet<Type> _importantSaveHookTypes;
		private readonly static object _lock = new object();

		// Contains all IDbHook/EntityType/State/Stage combinations in which
		// the implementor threw either NotImplementedException or NotSupportedException.
		// This boosts performance because these VOID combinations are not processed again
		// and frees us mostly from the obligation always to detect changes.
		private readonly static HashSet<HookKey> _voidHooks = new HashSet<HookKey>();

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

		public IEnumerable<IDbLoadHook> TriggerLoadHooks(BaseEntity entity, bool importantHooksOnly)
		{
			Guard.NotNull(entity, nameof(entity));

			var processedHooks = new HashSet<IDbLoadHook>();

			if (!_loadHooks.Any() || (importantHooksOnly && !this.HasImportantLoadHooks()))
			{
				return processedHooks;
			}

			var entityType = entity.GetUnproxiedType();			

			var hooks = GetLoadHookInstancesFor(entityType, importantHooksOnly);
			foreach (var hook in hooks)
			{
				// call hook
				try
				{
					hook.OnLoaded(entity);
					processedHooks.Add(hook);
				}
				catch (Exception ex) when (ex is NotImplementedException || ex is NotSupportedException)
				{
					RegisterVoidHook(hook, entityType, EntityState.Unchanged, HookStage.Load);
				}
				catch (Exception ex)
				{
					Logger.ErrorFormat(ex, "LoadHook exception ({0})", hook.GetType().FullName);
				}
			}

			return processedHooks;
		}

		public IEnumerable<IDbSaveHook> TriggerPreSaveHooks(IEnumerable<IHookedEntity> entries, bool importantHooksOnly, out bool anyStateChanged)
		{
			Guard.NotNull(entries, nameof(entries));

			anyStateChanged = false;

			var processedHooks = new HashSet<IDbSaveHook>();

			if (!entries.Any() || !_saveHooks.Any() || (importantHooksOnly && !this.HasImportantSaveHooks()))
				return processedHooks;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure

				if (HandledAlready(e, HookStage.PreSave))
				{
					// Prevent repetitive hooking of the same entity/state/pre combination within a single request
					continue;
				}

				var hooks = GetSaveHookInstancesFor(e, HookStage.PreSave, importantHooksOnly);

				foreach (var hook in hooks)
				{
					// call hook
					try
					{
						//Logger.DebugFormat("PRE save hook: {0}, State: {1}, Entity: {2}", hook.GetType().Name, e.InitialState, e.Entity.GetUnproxiedType().Name);
						hook.OnBeforeSave(e);
						processedHooks.Add(hook);
					}
					catch (Exception ex) when (ex is NotImplementedException || ex is NotSupportedException)
					{
						RegisterVoidHook(hook, e.EntityType, e.InitialState, HookStage.PreSave);
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

			return processedHooks;
		}

		public IEnumerable<IDbSaveHook> TriggerPostSaveHooks(IEnumerable<IHookedEntity> entries, bool importantHooksOnly)
		{
			Guard.NotNull(entries, nameof(entries));

			var processedHooks = new HashSet<IDbSaveHook>();

			if (!entries.Any() || !_saveHooks.Any() || (importantHooksOnly && !this.HasImportantSaveHooks()))
				return processedHooks;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure

				if (HandledAlready(e, HookStage.PostSave))
				{
					// Prevent repetitive hooking of the same entity/state/post combination within a single request
					continue;
				}

				var hooks = GetSaveHookInstancesFor(e, HookStage.PostSave, importantHooksOnly);
				
				foreach (var hook in hooks)
				{
					// call hook
					try
					{
						//Logger.DebugFormat("POST save hook: {0}, State: {1}, Entity: {2}", hook.GetType().Name, e.InitialState, e.Entity.GetUnproxiedType().Name);
						hook.OnAfterSave(e);
						processedHooks.Add(hook);
					}
					catch (Exception ex) when (ex is NotImplementedException || ex is NotSupportedException)
					{
						RegisterVoidHook(hook, e.EntityType, e.InitialState, HookStage.PostSave);
					}
					catch (Exception ex)
					{
						Logger.ErrorFormat(ex, "PostSaveHook exception ({0})", hook.GetType().FullName);
					}
				}
			}

			processedHooks.Each(x => x.OnAfterSaveCompleted());

			return processedHooks;
		}

		private IEnumerable<IDbLoadHook> GetLoadHookInstancesFor(Type entityType, bool importantOnly)
		{
			return GetHookInstancesFor<IDbLoadHook>(
				entityType,
				EntityState.Unchanged,
				HookStage.Load,
				importantOnly, 
				_loadHooks, 
				_importantLoadHookTypes);
		}

		private IEnumerable<IDbSaveHook> GetSaveHookInstancesFor(IHookedEntity entry, HookStage stage, bool importantOnly)
		{
			return GetHookInstancesFor<IDbSaveHook>(
				entry.EntityType,
				entry.InitialState,
				stage,
				importantOnly, 
				_saveHooks,
				_importantSaveHookTypes);
		}

		private IEnumerable<THook> GetHookInstancesFor<THook>(
			Type entityType,
			EntityState entityState,
			HookStage stage,
			bool importantOnly,
			IList<Lazy<IDbHook, HookMetadata>> hookList,
			HashSet<Type> importantHookTypes) where THook : IDbHook
		{
			IEnumerable<IDbHook> hooks;

			if (entityType == null)
			{
				return Enumerable.Empty<THook>();
			}

			// For request cache lookup
			var requestKey = new RequestHookKey(entityType, entityState, stage, importantOnly);

			if (_hooksRequestCache.ContainsKey(requestKey))
			{
				hooks = _hooksRequestCache[requestKey];
			}
			else
			{
				hooks = hookList
					// Reduce by entity types which can be processed by this hook
					.Where(x => x.Metadata.HookedType.IsAssignableFrom(entityType))
					// When importantOnly, only include hook types with [ImportantAttribute]
					.Where(x => !importantOnly || importantHookTypes.Contains(x.Metadata.ImplType))
					// Exclude void hooks (hooks known to be useless for the current EntityType/State/Stage combination)
					.Where(x => !_voidHooks.Contains(new HookKey(x.Metadata.ImplType, entityType, entityState, stage)))
					.Select(x => x.Value)
					.ToArray();

				_hooksRequestCache.AddRange(requestKey, hooks);
			}

			return hooks.Cast<THook>();
		}

		private bool HandledAlready(IHookedEntity entry, HookStage stage)
		{
			var entity = entry.Entity;

			if (entity == null || entity.IsTransientRecord())
				return false;

			var key = new HookedEntityKey(entry.EntityType, entity.Id, entry.InitialState, stage);
			if (_hookedEntities.Contains(key))
			{
				return true;
			}

			_hookedEntities.Add(key);
			return false;
		}

		private void RegisterVoidHook(IDbHook hook, Type entityType, EntityState entityState, HookStage stage)
		{
			var hookType = hook.GetType();

			// Unregister from request cache (if cached)
			_hooksRequestCache.Remove(new RequestHookKey(entityType, entityState, stage, false), hook);
			_hooksRequestCache.Remove(new RequestHookKey(entityType, entityState, stage, true), hook);

			lock (_lock)
			{
				// Add to static void hooks set
				_voidHooks.Add(new HookKey(hookType, entityType, entityState, stage));
			}
		}

		enum HookStage
		{
			Load,
			PreSave,
			PostSave
		}

		class HookedEntityKey : Tuple<Type, int, EntityState, HookStage>
		{
			public HookedEntityKey(Type entityType, int entityId, EntityState initialState, HookStage stage)
				: base(entityType, entityId, initialState, stage)
			{
			}
		}

		class RequestHookKey : Tuple<Type, EntityState, HookStage, bool>
		{
			public RequestHookKey(Type entityType, EntityState entityState, HookStage stage, bool importantOnly)
				: base(entityType, entityState, stage, importantOnly)
			{
			}
		}

		class HookKey : Tuple<Type, Type, EntityState, HookStage>
		{
			public HookKey(Type hookType, Type entityType, EntityState entityState, HookStage stage)
				: base(hookType, entityType, entityState, stage)
			{
			}
		}
	}
}
