using System;

namespace SmartStore.Core.Data.Hooks
{
	/// <summary>
	/// Turns hooking for a specific entity type explicitly on or off.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class HookableAttribute : Attribute
	{
		public HookableAttribute(bool isHookable)
		{
			IsHookable = IsHookable;
		}

		public bool IsHookable
		{
			get;
			private set;
		}
	}
}
