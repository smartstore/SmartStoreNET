using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Data.Hooks
{
	public class DefaultHookHandler : IHookHandler
	{
		private readonly IEnumerable<Lazy<IPreActionHook, HookMetadata>> _preHooks;
		private readonly IEnumerable<Lazy<IPostActionHook, HookMetadata>> _postHooks;

		private readonly Multimap<Type, IPreActionHook> _preHooksRequestCache = new Multimap<Type, IPreActionHook>();
		private readonly Multimap<Type, IPostActionHook> _postHooksRequestCache = new Multimap<Type, IPostActionHook>();

		private static HashSet<Type> _importantPreHookTypes;
		private static HashSet<Type> _importantPostHookTypes;
		private readonly static object _lock = new object();

		public DefaultHookHandler(
			IEnumerable<Lazy<IPreActionHook, HookMetadata>> preHooks,
			IEnumerable<Lazy<IPostActionHook, HookMetadata>> postHooks)
		{
			_preHooks = preHooks;
			_postHooks = postHooks;
		}

		public ILogger Logger
		{
			get;
			set;
		}

		public bool HasImportantPreHooks()
		{
			if (_importantPreHookTypes == null)
			{
				lock (_lock)
				{
					if (_importantPreHookTypes == null)
					{
						_importantPreHookTypes = new HashSet<Type>();
						_importantPreHookTypes.AddRange(_preHooks.Where(x => x.Metadata.Important).Select(x => x.Metadata.ImplType));
					}
				}
			}

			return _importantPreHookTypes.Any();
		}

		public bool HasImportantPostHooks()
		{
			if (_importantPostHookTypes == null)
			{
				lock (_lock)
				{
					if (_importantPostHookTypes == null)
					{
						_importantPostHookTypes = new HashSet<Type>();
						_importantPostHookTypes.AddRange(_postHooks.Where(x => x.Metadata.Important).Select(x => x.Metadata.ImplType));
					}
				}
			}

			return _importantPostHookTypes.Any();
		}

		public bool TriggerPreActionHooks(IEnumerable<HookedEntityEntry> entries, bool requiresValidation, bool importantHooksOnly)
		{
			bool anyStateChanged = false;

			if (entries == null || !_preHooks.Any() || (importantHooksOnly && !this.HasImportantPreHooks()))
				return false;

			var processedHooks = new HashSet<IPreActionHook>();

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var preHooks = GetPreHookInstancesFor(e.Entry.Entity as BaseEntity, importantHooksOnly);
				foreach (var hook in preHooks)
				{
					if (hook.CanProcess(e.PreSaveState) && hook.RequiresValidation == requiresValidation)
					{
						var metadata = new HookEntityMetadata(e.PreSaveState);

						// call hook
						try
						{
							hook.HookObject(e.Entry.Entity, metadata);
						}
						catch (Exception ex)
						{
							Logger.ErrorFormat(ex, "PreHook exception ({0})", hook.GetType().FullName);
						}

						processedHooks.Add(hook);

						// change state if applicable
						if (metadata.HasStateChanged)
						{
							e.PreSaveState = metadata.State;
							e.Entry.State = (System.Data.Entity.EntityState)((int)metadata.State);
							anyStateChanged = true;
						}
					}
				}
			}

			processedHooks.Each(x => x.OnCompleted());

			return anyStateChanged;
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private IEnumerable<IPreActionHook> GetPreHookInstancesFor(BaseEntity entity, bool importantOnly)
		{
			if (entity == null)
				return Enumerable.Empty<IPreActionHook>();

			IEnumerable<IPreActionHook> hooks;

			var hookedType = entity.GetUnproxiedType();

			if (_preHooksRequestCache.ContainsKey(hookedType))
			{
				hooks = _preHooksRequestCache[hookedType];
			}
			else
			{
				hooks = _preHooks.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => x.Value);
				_preHooksRequestCache.AddRange(hookedType, hooks);
			}

			if (importantOnly && hooks.Any())
			{
				hooks = hooks.Where(x => _importantPreHookTypes.Contains(x.GetType()));
			}

			return hooks;
		}


		public void TriggerPostActionHooks(IEnumerable<HookedEntityEntry> entries, bool importantHooksOnly)
		{
			if (entries == null || !_postHooks.Any() || (importantHooksOnly && !this.HasImportantPostHooks()))
				return;

			var processedHooks = new HashSet<IPostActionHook>();

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var postHooks = GetPostHookInstancesFor(e.Entry.Entity as BaseEntity, importantHooksOnly);
				foreach (var hook in postHooks)
				{
					if (hook.CanProcess(e.PreSaveState))
					{
						var metadata = new HookEntityMetadata(e.PreSaveState);

						// call hook
						try
						{
							hook.HookObject(e.Entry.Entity, metadata);
						}
						catch (Exception ex)
						{
							Logger.ErrorFormat(ex, "PostHook exception ({0})", hook.GetType().FullName);
						}

						processedHooks.Add(hook);
					}
				}
			}

			processedHooks.Each(x => x.OnCompleted());
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private IEnumerable<IPostActionHook> GetPostHookInstancesFor(BaseEntity entity, bool importantOnly)
		{
			if (entity == null)
				return Enumerable.Empty<IPostActionHook>();

			IEnumerable<IPostActionHook> hooks;

			var hookedType = entity.GetUnproxiedType();

			if (_postHooksRequestCache.ContainsKey(hookedType))
			{
				hooks = _postHooksRequestCache[hookedType];
			}
			else
			{
				hooks = _postHooks.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => x.Value);
				_postHooksRequestCache.AddRange(hookedType, hooks);
			}

			if (importantOnly && hooks.Any())
			{
				hooks = hooks.Where(x => _importantPostHookTypes.Contains(x.GetType()));
			}

			return hooks;
		}
	}
}
