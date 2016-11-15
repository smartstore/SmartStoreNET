using System;

namespace SmartStore.Core.Data.Hooks
{
	public class HookMetadata
	{
		/// <summary>
		/// The type of entity
		/// </summary>
		public Type HookedType { get; set; }

		/// <summary>
		/// The type of the hook class itself
		/// </summary>
		public Type ImplType { get; set; }

		public bool IsLoadHook { get; set; }

		/// <summary>
		/// Whether the hook should run in any case, even if hooking has been turned off.
		/// </summary>
		public bool Important { get; set; }

		public static HookMetadata Create<THook>(Type hookedType, bool important = false) where THook : IDbHook
		{
			Guard.NotNull(hookedType, nameof(hookedType));

			return new HookMetadata
			{
				ImplType = typeof(THook),
				HookedType = hookedType,
				Important = important,
				IsLoadHook = typeof(IDbLoadHook).IsAssignableFrom(hookedType)
			};
		}
	}
}
