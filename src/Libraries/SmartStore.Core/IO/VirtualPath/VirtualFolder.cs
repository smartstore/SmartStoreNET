using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Logging;

namespace SmartStore.Core.IO
{
	public class VirtualFolder : IVirtualFolder
	{
		private readonly IVirtualPathProvider _vpp;
		private readonly ILogger _logger;
		private readonly string _root;

		public VirtualFolder(string root, IVirtualPathProvider vpp)
			: this(root, vpp, NullLogger.Instance)
		{
		}

		public VirtualFolder(string root, IVirtualPathProvider vpp, ILogger logger)
		{
			Guard.NotEmpty(root, nameof(root));
			Guard.NotNull(vpp, nameof(vpp));
			Guard.NotNull(logger, nameof(logger));

			if (!root.StartsWith("~/"))
			{
				throw new ArgumentException("Root path must be a valid application relative path starting with '~/'", nameof(root));
			}

			_root = root.Replace(Path.DirectorySeparatorChar, '/').EnsureEndsWith("/");
			_vpp = vpp;
			_logger = logger;
		}

		public IVirtualPathProvider VirtualPathProvider
		{
			get { return _vpp; }
		}

		public string RootPath
		{
			get { return _root; }
		}

		public virtual string MapPath(string relativePath)
		{
			return _vpp.MapPath(GetVirtualPath(relativePath));
		}

		public virtual string Combine(params string[] paths)
		{
			return _vpp.Combine(paths);
		}

		public virtual bool DirectoryExists(string relativePath)
		{
			return _vpp.DirectoryExists(GetVirtualPath(relativePath));
		}

		public virtual bool FileExists(string relativePath)
		{
			return _vpp.FileExists(GetVirtualPath(relativePath));
		}

		public virtual IEnumerable<string> ListDirectories(string relativePath)
		{
			var path = GetVirtualPath(relativePath);

			if (!_vpp.DirectoryExists(path))
			{
				return Enumerable.Empty<string>();
			}

			return _vpp.ListDirectories(path).Select(x => x.Substring(_root.Length));
		}

		public virtual IEnumerable<string> ListFiles(string relativePath, bool deep = false)
		{
			var path = GetVirtualPath(relativePath);

			if (!_vpp.DirectoryExists(path))
			{
				return Enumerable.Empty<string>();
			}

			var files = _vpp.ListFiles(path).Select(x => x.Substring(_root.Length));

			if (deep)
			{
				return files.Concat(ListDirectories(path).SelectMany(d => ListFiles(d, true)));
			}

			return files;
		}

		public virtual string GetDirectoryName(string relativePath)
		{
			return Path.GetDirectoryName(relativePath).Replace(Path.DirectorySeparatorChar, '/');
		}

		public virtual Stream OpenFile(string relativePath)
		{
			return _vpp.OpenFile(GetVirtualPath(relativePath));
		}

		public void CreateTextFile(string relativePath, string content)
		{
			Guard.NotEmpty(relativePath, nameof(relativePath));

			using (var stream = CreateFile(relativePath))
			{
				using (var tw = new StreamWriter(stream))
				{
					tw.Write(content);
				}
			}
		}

		public virtual Stream CreateFile(string relativePath)
		{
			var filePath = MapPath(relativePath);
			var folderPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}
				
			return File.Create(filePath);
		}

		public virtual DateTime GetFileLastWriteTimeUtc(string relativePath)
		{
			return File.GetLastWriteTimeUtc(MapPath(relativePath));
		}

		public virtual void DeleteFile(string relativePath)
		{
			File.Delete(MapPath(relativePath));
		}

		public virtual void CreateDirectory(string relativePath)
		{
			Directory.CreateDirectory(MapPath(relativePath));
		}

		public virtual void DeleteDirectory(string relativePath)
		{
			Directory.Delete(MapPath(relativePath), true);
		}

		public virtual string ReadFile(string relativePath)
		{
			Guard.NotEmpty(relativePath, nameof(relativePath));

			var path = GetVirtualPath(relativePath);

			if (!_vpp.FileExists(path))
			{
				return null;
			}

			using (var stream = _vpp.OpenFile(path))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public virtual void CopyFile(string relativePath, Stream destination)
		{
			Guard.NotEmpty(relativePath, nameof(relativePath));
			Guard.NotNull(destination, nameof(destination));

			var path = GetVirtualPath(relativePath);

			if (!_vpp.FileExists(path))
			{
				throw Error.InvalidOperation("File '{0}' does not exist".FormatInvariant(path));
			}

			using (var stream = _vpp.OpenFile(path))
			{
				stream.CopyTo(destination);
			}
		}

		public string GetVirtualPath(string relativePath)
		{
			Guard.NotNull(relativePath, nameof(relativePath));

			if (relativePath.StartsWith("~/"))
			{
				return relativePath;
			}

			return _root + relativePath.EmptyNull().Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
		}
	}
}
