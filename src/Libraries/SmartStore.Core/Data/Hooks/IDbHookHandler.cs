using System;
using System.Collections.Generic;

namespace SmartStore.Core.Data.Hooks
{
	public interface IDbHookHandler
	{
		bool HasImportantLoadHooks();
		bool HasImportantSaveHooks();

		/// <summary>
		/// Triggers all load hooks for a single entity
		/// </summary>
		/// <param name="importantHooksOnly"></param>
		/// <param name="entity">The loaded entity</param>
		void TriggerLoadHooks(BaseEntity entity, bool importantHooksOnly);

		/// <summary>
		/// Triggers all pre action hooks
		/// </summary>
		/// <param name="entries">Entries</param>
		/// <param name="importantHooksOnly"></param>
		/// <returns><c>true</c> if the state of any entry changed</returns>
		bool TriggerPreSaveHooks(IEnumerable<HookedEntity> entries, bool importantHooksOnly);

		/// <summary>
		/// Triggers all post action hooks
		/// </summary>
		/// <param name="entries">Entries</param>
		/// <param name="importantHooksOnly"></param>
		void TriggerPostSaveHooks(IEnumerable<HookedEntity> entries, bool importantHooksOnly);
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

		public void TriggerLoadHooks(BaseEntity entity, bool importantHooksOnly)
		{
		}

		public bool TriggerPreSaveHooks(IEnumerable<HookedEntity> entries, bool importantHooksOnly)
		{
			return false;
		}

		public void TriggerPostSaveHooks(IEnumerable<HookedEntity> entries, bool importantHooksOnly)
		{
		}
	}
}
