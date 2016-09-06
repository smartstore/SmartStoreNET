using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Data.Hooks
{
	/// <summary>
	/// Indicates that a hook instance should run in any case, even if hooking has been turned off.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ImportantAttribute : Attribute
	{
	}
}
