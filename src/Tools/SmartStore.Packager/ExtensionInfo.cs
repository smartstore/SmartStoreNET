using System;
using System.Collections.Generic;

namespace SmartStore.Packager
{
	public class ExtensionInfo : Tuple<string, string>
	{
		public ExtensionInfo(string path, string name) : base(path, name)
		{
		}

		public string Path { get { return base.Item1; } }
		public string Name { get { return base.Item2; } }
	}
}
