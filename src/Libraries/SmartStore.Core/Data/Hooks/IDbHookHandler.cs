using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Data.Hooks
{
	public interface IDbHookHandler
	{
		bool HasImportantLoadHooks();
		bool HasImportantSaveHooks();

		/// <summary>
		/// Triggers all load hooks for a single entity
		/// </summary>
		/// <param name="importantHooksOnly">Whether to trigger only hooks marked with the <see cref="ImportantAttribute"/> attribute</param>
		/// <param name="entity">The loaded entity</param>
		/// <returns>The list of actually processed hook instances</returns>
		IEnumerable<IDbLoadHook> TriggerLoadHooks(BaseEntity entity, bool importantHooksOnly);

		/// <summary>
		/// Triggers all pre action hooks
		/// </summary>
		/// <param name="entries">Entries</param>
		/// <param name="importantHooksOnly">Whether to trigger only hooks marked with the <see cref="ImportantAttribute"/> attribute</param>
		/// <param name="anyStateChanged"><c>true</c> if the state of any entry changed</param>
		/// <returns>The list of actually processed hook instances</returns>
		IEnumerable<IDbSaveHook> TriggerPreSaveHooks(IEnumerable<IHookedEntity> entries, bool importantHooksOnly, out bool anyStateChanged);

		/// <summary>
		/// Triggers all post action hooks
		/// </summary>
		/// <param name="entries">Entries</param>
		/// <param name="importantHooksOnly">Whether to trigger only hooks marked with the <see cref="ImportantAttribute"/> attribute</param>
		/// <returns>The list of actually processed hook instances</returns>
		IEnumerable<IDbSaveHook> TriggerPostSaveHooks(IEnumerable<IHookedEntity> entries, bool importantHooksOnly);
	}

	public sealed class NullDbHookHandler : IDbHookHandler
	{
		private readonly static IDbHookHandler s_instance = new NullDbHookHandler();

		public static IDbHookHandler Instance
		{
			get { return s_instance; }
		}

		public bool HasImportantLoadHooks()
		{
			return false;
		}

		public bool HasImportantSaveHooks()
		{
			return false;
		}

		public IEnumerable<IDbLoadHook> TriggerLoadHooks(BaseEntity entity, bool importantHooksOnly)
		{
			return Enumerable.Empty<IDbLoadHook>();
		}

		public IEnumerable<IDbSaveHook> TriggerPreSaveHooks(IEnumerable<IHookedEntity> entries, bool importantHooksOnly, out bool anyStateChanged)
		{
			anyStateChanged = false;
			return Enumerable.Empty<IDbSaveHook>();
		}

		public IEnumerable<IDbSaveHook> TriggerPostSaveHooks(IEnumerable<IHookedEntity> entries, bool importantHooksOnly)
		{
			return Enumerable.Empty<IDbSaveHook>();
		}
	}
}
