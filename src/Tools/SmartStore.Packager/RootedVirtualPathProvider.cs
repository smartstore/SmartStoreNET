using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;

namespace SmartStore.Packager
{
	public class RootedVirtualPathProvider : DefaultVirtualPathProvider
	{
		private readonly string _rootPath;

		public RootedVirtualPathProvider(string rootPath) : base(NullLogger.Instance)
		{
			Guard.NotEmpty(rootPath, nameof(rootPath));

			_rootPath = rootPath;
		}

		public override IEnumerable<string> ListFiles(string path)
		{
			var absPath = MapPath(path);
			var files = new DirectoryInfo(absPath).GetFiles().Select(f => Combine(path, f.Name));
			return files;
		}

		public override IEnumerable<string> ListDirectories(string path)
		{
			var absPath = MapPath(path);
			var files = new DirectoryInfo(absPath).GetDirectories().Select(d => Combine(path, d.Name));
			return files;
		}

		public override Stream OpenFile(string virtualPath)
		{
			return File.OpenRead(MapPath(virtualPath));
		}

		public override string MapPath(string virtualPath)
		{
			virtualPath = virtualPath.Replace("~/", "").TrimStart('/').Replace('/', '\\');
			return Path.Combine(_rootPath, virtualPath);
		}

		public override string Normalize(string virtualPath)
		{
			return virtualPath;
		}

		public override bool FileExists(string virtualPath)
		{
			return File.Exists(MapPath(virtualPath));
		}

		public override bool DirectoryExists(string virtualPath)
		{
			return Directory.Exists(MapPath(virtualPath));
		}
	}
}
