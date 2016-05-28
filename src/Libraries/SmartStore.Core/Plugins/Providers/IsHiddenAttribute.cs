using System;

namespace SmartStore.Core.Plugins
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class IsHiddenAttribute : Attribute
	{
		public IsHiddenAttribute(bool isHidden)
		{
			IsHidden = isHidden;
		}

		public bool IsHidden { get; set; }
	}
}
