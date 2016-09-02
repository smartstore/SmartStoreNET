using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Autofac.Features.Metadata;
using SmartStore.Collections;
using SmartStore.Core.Events;

namespace SmartStore.Core.Data.Hooks
{
	public abstract class HookEventBase
	{
		public IEnumerable<HookedEntityEntry> ModifiedEntries { get; set; }
		public bool ImportantHooksOnly { get; set; }
	}

	public class PreActionHookEvent : HookEventBase
	{
		/// <summary>
		/// If set to <c>true</c>, executes hooks that require validation, otherwise executes hooks that do NOT require validation
		/// </summary>
		public bool RequiresValidation { get; set; }
	}

	public class PostActionHookEvent : HookEventBase
	{
	}

	public class HookMetadata
	{
		public Type HookedType { get; set; }
		/// <summary>
		/// Whether the hook should run in any case, even if hooking has been turned off.
		/// </summary>
		public bool Important { get; set; }
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
			var entries = eventMessage.ModifiedEntries.ToArray();

			if (entries.Length == 0 || !_preHooks.Any())
				return;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var preHooks = GetPreHookInstancesFor(e.Entity.GetType(), eventMessage.ImportantHooksOnly);
				foreach (var hook in preHooks)
				{
					if (hook.CanProcess(e.PreSaveState) && hook.RequiresValidation == eventMessage.RequiresValidation)
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

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private IEnumerable<IPreActionHook> GetPreHookInstancesFor(Type hookedType, bool importantOnly = false)
		{
			IEnumerable<IPreActionHook> hooks;

			if (_preHooksCache.ContainsKey(hookedType)) 
			{
				hooks = _preHooksCache[hookedType];
			}
			else
			{
				hooks = _preHooks.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => x.Value);
				_preHooksCache.AddRange(hookedType, hooks);
			}

			if (importantOnly)
			{
				var importantHookTypes = _preHooks.Where(x => x.Metadata.Important == true).Select(x => x.Metadata.HookedType);
				hooks = hooks.Where(x => importantHookTypes.Contains(x.GetType()));
			}

			return hooks;
		}

		public void HandleEvent(PostActionHookEvent eventMessage)
		{
			var entries = eventMessage.ModifiedEntries.ToArray();

			if (entries.Length == 0 || !_postHooks.Any())
				return;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				var postHooks = GetPostHookInstancesFor(e.Entity.GetType(), eventMessage.ImportantHooksOnly);
				foreach (var hook in postHooks)
				{
					if (hook.CanProcess(e.PreSaveState))
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

		private IEnumerable<IPostActionHook> GetPostHookInstancesFor(Type hookedType, bool importantOnly = false)
		{
			IEnumerable<IPostActionHook> hooks;

			if (_postHooksCache.ContainsKey(hookedType))
			{
				hooks = _postHooksCache[hookedType];
			}
			else
			{
				hooks = _postHooks.Where(x => x.Metadata.HookedType.IsAssignableFrom(hookedType)).Select(x => x.Value);
				_postHooksCache.AddRange(hookedType, hooks);
			}

			if (importantOnly)
			{
				var importantHookTypes = _postHooks.Where(x => x.Metadata.Important == true).Select(x => x.Metadata.HookedType);
				hooks = hooks.Where(x => importantHookTypes.Contains(x.GetType()));
			}

			return hooks;
		}

	}
}
