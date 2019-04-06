using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;

namespace SmartStore.Data.Caching
{
	public sealed class EfDbModelStore : DefaultDbModelStore
	{
		public EfDbModelStore(string location)
			: base(location)
		{ }

		public override DbCompiledModel TryLoad(Type contextType)
		{
			string path = GetFilePath(contextType);

			if (File.Exists(path))
			{
				var cachedModelCreatedOn = File.GetLastWriteTimeUtc(path);
				var assemblyCreatedOn = File.GetLastWriteTimeUtc(contextType.Assembly.Location);
				if (assemblyCreatedOn > cachedModelCreatedOn)
				{
					File.Delete(path);
					Debug.WriteLine("Cached db model obsolete. Re-creating cached db model edmx.");
				}
			}
			else
			{
				Debug.WriteLine("No cached db model found. Creating cached db model edmx.");
			}

			return base.TryLoad(contextType);
		}
	}
}
