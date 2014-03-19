using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac.Features.Metadata;
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

	public class HooksEventConsumer :
		IConsumer<PreActionHookEvent>,
		IConsumer<PostActionHookEvent>
	{
		private readonly IEnumerable<Meta<Lazy<IHook>>> _hooks;

		public HooksEventConsumer(IEnumerable<Meta<Lazy<IHook>>> hooks)
		{
			this._hooks = hooks;
		}

		public void HandleEvent(PreActionHookEvent eventMessage)
		{
			var entries = eventMessage.ModifiedEntries;

			if (!entries.Any())
				return;

			var preHooks = _hooks.Where(x => x.Metadata["stage"].ToString() == "pre").Select(x => x.Value.Value).Cast<IPreActionHook>().ToList();

			if (!preHooks.Any())
				return;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				foreach (var hook in preHooks.Where(x => x.HookStates == e.PreSaveState && x.RequiresValidation == eventMessage.RequiresValidation))
				{
					var metadata = new HookEntityMetadata(e.PreSaveState);
					hook.HookObject(e.Entity, metadata);

					if (metadata.HasStateChanged)
					{
						e.PreSaveState = metadata.State;
					}
				}
			}
		}

		public void HandleEvent(PostActionHookEvent eventMessage)
		{
			var entries = eventMessage.ModifiedEntries;

			if (!entries.Any())
				return;

			var postHooks = _hooks.Where(x => x.Metadata["stage"].ToString() == "post").Select(x => x.Value.Value).Cast<IPostActionHook>().ToList();

			if (!postHooks.Any())
				return;

			foreach (var entry in entries)
			{
				var e = entry; // Prevents access to modified closure
				if (e.Entity is SmartStore.Core.Domain.Localization.ILocalizedEntity)
				{
					Debug.WriteLine(e.Entity.ToString());
				}
				foreach (var hook in postHooks.Where(x => x.HookStates == e.PreSaveState))
				{
					var metadata = new HookEntityMetadata(e.PreSaveState);
					hook.HookObject(e.Entity, metadata);
				}
			}
		}

	}

}
