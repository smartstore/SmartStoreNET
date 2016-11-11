//using System;
//using System.Collections.Generic;

//namespace SmartStore.Core.Data.Hooks
//{
//	public interface IHookHandler
//	{
//		bool HasImportantPreHooks();
//		bool HasImportantPostHooks();

//		/// <summary>
//		/// Triggers all pre action hooks
//		/// </summary>
//		/// <param name="entries">Entries</param>
//		/// <param name="requiresValidation"></param>
//		/// <param name="importantHooksOnly"></param>
//		/// <returns><c>true</c> if the state of any entry changed</returns>
//		bool TriggerPreActionHooks(IEnumerable<HookedEntityEntry> entries, bool requiresValidation, bool importantHooksOnly);

//		/// <summary>
//		/// Triggers all post action hooks
//		/// </summary>
//		/// <param name="entries">Entries</param>
//		/// <param name="importantHooksOnly"></param>
//		void TriggerPostActionHooks(IEnumerable<HookedEntityEntry> entries, bool importantHooksOnly);
//	}

//	public sealed class NullHookHandler : IHookHandler
//	{
//		private readonly static IHookHandler s_instance = new NullHookHandler();

//		public static IHookHandler Instance
//		{
//			get { return s_instance; }
//		}

//		public bool HasImportantPostHooks()
//		{
//			return false;
//		}

//		public bool HasImportantPreHooks()
//		{
//			return false;
//		}

//		public void TriggerPostActionHooks(IEnumerable<HookedEntityEntry> entries, bool importantHooksOnly)
//		{
//		}

//		public bool TriggerPreActionHooks(IEnumerable<HookedEntityEntry> entries, bool requiresValidation, bool importantHooksOnly)
//		{
//			return false;
//		}
//	}
//}
