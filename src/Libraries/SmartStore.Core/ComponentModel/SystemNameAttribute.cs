using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public class SystemNameAttribute : Attribute
	{
		public SystemNameAttribute(string name)
		{
			Guard.ArgumentNotEmpty(() => name);
			Name = name;
		}

		public string Name { get; set; }
	}
}
