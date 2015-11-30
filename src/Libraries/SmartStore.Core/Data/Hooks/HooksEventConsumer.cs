using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Autofac.Features.Metadata;
using SmartStore.Collections;
using SmartStore.Core.Events;

namespace SmartStore.Core.Data.Hooks
{

	public class PreActionHookEvent
	{
		public IEnumerable<HookedEntityEntry> ModifiedEntries { get; set; }

		/// <summary>
		/// If set to <c>true</c>, executes hooks that require validation, otherwise executes hooks that do NOT require validation
		/// </summary>
		public bool RequiresValidation { get; set; }
	}

	public class PostActionHookEvent
	{
		public IEnumerable<HookedEntityEntry> ModifiedEntries { get; set; }
	}

	public class HookMetadata
	{
		public Type HookedType { get; set; }
	}

	public class HooksEventConsumer :
		IConsumer<PreActionHookEvent>,
		IConsumer<PostActionHookEvent>
	{
		private readonly IEnumerable<Lazy<IPreActionHook, HookMetadata>> _preHooks;
		private readonly IEnumerable<Lazy<IPostActionHook, HookMetadata>> _postHooks;

		private readonly Multimap<Type, IPreActionHook> _preHooksCache = new Multimap<Type, IPreActionHook>();
		private readonly Multimap<Type, IPostActionHook> _postHooksCache = new Multimap<Type, IPostActionHook>();

		public HooksEventConsumer(
			IEnumerable<Lazy<IPreActionHook, HookMetadata>> preHooks,
			IEnumerable<Lazy<IPostActionHook, HookMetadata>> postHooks)
		{
			this._preHooks = preHooks;
			this._postHooks = postHooks;
		}

		public void HandleEvent(PreActionHookEvent eventMessage)
		{
			var entries = eventMessage.ModifiedEntries;

			if (!entries.Any() || !_preHooks.Any())
				return;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var preHooks = GetPreHookInstancesFor(e.Entity.GetType());
				foreach (var hook in preHooks)
				{
					if (hook.HookStates == e.PreSaveState && hook.RequiresValidation == eventMessage.RequiresValidation)
					{
						var metadata = new HookEntityMetadata(e.PreSaveState);
						using (var scope = new DbContextScope(hooksEnabled: false))
						{
							// dead end: don't let hooks call hooks again
							hook.HookObject(e.Entity, metadata);
						}

						if (metadata.HasStateChanged)
						{
							e.PreSaveState = metadata.State;
						}
					}
				}
			}
		}

		private IEnumerable<IPreActionHook> GetPreHookInstancesFor(Type hookedType)
		{
			if (_preHooksCache.ContainsKey(hookedType)) 
			{
				return _preHooksCache[hookedType];
			}

			var hooks = _preHooks.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => x.Value);
			_preHooksCache.AddRange(hookedType, hooks);
			return hooks;
		}

		public void HandleEvent(PostActionHookEvent eventMessage)
		{
			var entries = eventMessage.ModifiedEntries;

			if (!entries.Any() || !_postHooks.Any())
				return;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var postHooks = GetPostHookInstancesFor(e.Entity.GetType());
				foreach (var hook in postHooks)
				{
					if (hook.HookStates == e.PreSaveState)
					{
						var metadata = new HookEntityMetadata(e.PreSaveState);
						using (var scope = new DbContextScope(hooksEnabled: false))
						{
							// dead end: don't let hooks call hooks again
							hook.HookObject(e.Entity, metadata);
						}
					}
				}
			}
		}

		private IEnumerable<IPostActionHook> GetPostHookInstancesFor(Type hookedType)
		{
			if (_postHooksCache.ContainsKey(hookedType))
			{
				return _postHooksCache[hookedType];
			}

			var hooks = _postHooks.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => x.Value);
			_postHooksCache.AddRange(hookedType, hooks);
			return hooks;
		}

	}

}
