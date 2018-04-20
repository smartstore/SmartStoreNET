using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using SmartStore.Utilities;

namespace SmartStore.Core.IO
{
	public class LocalFileSystem : IFileSystem
	{
		private string _root;
		private string _publicPath;		// /Shop/base
		private string _storagePath;    // C:\SMNET\base	
		private bool _isCloudStorage;	// When public URL is outside of current app	

		public LocalFileSystem()
			: this(string.Empty, string.Empty)
		{
		}

		protected internal LocalFileSystem(string basePath)
			: this(basePath, string.Empty)
		{
		}

		protected internal LocalFileSystem(string basePath, string publicPath)
		{
			basePath = basePath.EmptyNull();

			var pathIsAbsolute = FileSystemHelper.IsFullPath(basePath);

			NormalizeStoragePath(ref basePath, pathIsAbsolute);

			_publicPath = NormalizePublicPath(publicPath, basePath, pathIsAbsolute);

			_root = basePath;
		}

		public bool IsCloudStorage
		{
			get { return _isCloudStorage; }
		}

		private void NormalizeStoragePath(ref string basePath, bool basePathIsAbsolute)
		{
			if (basePathIsAbsolute)
			{
				// Path is fully qualified (UNC or absolute with drive letter)

				// In this case this is our root path...
				basePath = basePath.TrimEnd(Path.DirectorySeparatorChar);

				// ...AND our physical storage path
				_storagePath = basePath;
			}
			else
			{
				// Path is relative to the app root
				basePath = basePath.TrimEnd('/').EnsureStartsWith("/");
				_storagePath = CommonHelper.MapPath("~" + basePath, false);
			}
		}

		private string NormalizePublicPath(string publicPath, string basePath, bool basePathIsAbsolute)
		{
			publicPath = publicPath.EmptyNull();

			if (basePathIsAbsolute)
			{
				if (publicPath.IsEmpty() || (!publicPath.StartsWith("~/") && !publicPath.IsWebUrl(true)))
				{
					var streamMedia = CommonHelper.GetAppSetting<bool>("sm:StreamRemoteMedia", true);
					if (!streamMedia)
					{
						throw new ArgumentException(@"When the base path is a fully qualified path and remote media streaming is disabled, 
													the public path must not be empty, and either be a fully qualified URL or a virtual path (e.g.: ~/Media)", nameof(publicPath));
					}		
				}
			}

			var appVirtualPath = HostingEnvironment.IsHosted 
				? HostingEnvironment.ApplicationVirtualPath.TrimEnd('/') 
				: string.Empty;

			if (publicPath.StartsWith("~/"))
			{
				// prepend application virtual path
				return appVirtualPath + publicPath.Substring(1);
			}

			if (publicPath.IsEmpty() && !basePathIsAbsolute)
			{
				// > /MyAppRoot/Media
				return appVirtualPath + basePath;
			}

			_isCloudStorage = true;

			return publicPath;
		}

		public string Root
		{
			get { return _root; }
		}

		/// <summary>
		/// Maps a relative path into the storage path.
		/// </summary>
		/// <param name="path">The relative path to be mapped.</param>
		/// <returns>The relative path combined with the storage path.</returns>
		protected virtual string MapStorage(string path)
		{
			var mappedPath = string.IsNullOrEmpty(path) ? _storagePath : Path.Combine(_storagePath, Fix(path));
			return ValidatePath(_storagePath, mappedPath);
		}

		/// <summary>
		/// Maps a relative path into the public path.
		/// </summary>
		/// <param name="path">The relative path to be mapped.</param>
		/// <returns>The relative path combined with the public path in an URL friendly format ('/' character for directory separator).</returns>
		protected virtual string MapPublic(string path)
		{
			return string.IsNullOrEmpty(path) ? _publicPath : HttpUtility.UrlDecode(Path.Combine(_publicPath, path).Replace(Path.DirectorySeparatorChar, '/'));
		}

		static string Fix(string path)
		{
			return string.IsNullOrEmpty(path)
					   ? ""
					   : Path.DirectorySeparatorChar != '/'
							 ? path.Replace('/', Path.DirectorySeparatorChar).TrimStart('/', '\\')
							 : path.TrimStart('/', '\\');
		}

		public string GetPublicUrl(string path, bool forCloud = false)
		{
			return MapPublic(path);
		}

		public virtual string GetStoragePath(string url)
		{
			if (url.HasValue())
			{
				if (url.StartsWith("~/"))
				{
					url = VirtualPathUtility.ToAbsolute(url);
				}

				if (url.StartsWith(_publicPath))
				{
					return HttpUtility.UrlDecode(url.Substring(_publicPath.Length).Replace('/', Path.DirectorySeparatorChar));
				}
			}

			return null;
		}

		public bool FileExists(string path)
		{
			return File.Exists(MapStorage(path));
		}

		public bool FolderExists(string path)
		{
			return Directory.Exists(MapStorage(path));
		}

		public IFile GetFile(string path)
		{
			var fileInfo = new FileInfo(MapStorage(path));
			return new LocalFile(Fix(path), fileInfo);
		}

		public IFolder GetFolder(string path)
		{
			var directoryInfo = new DirectoryInfo(MapStorage(path));
			return new LocalFolder(Fix(path), directoryInfo);
		}

		public IFolder GetFolderForFile(string path)
		{
			var fileInfo = new FileInfo(MapStorage(path));
			if (!fileInfo.Exists)
			{
				throw new ArgumentException("File " + path + " does not exist");
			}

			if (!fileInfo.Directory.Exists)
			{
				throw new ArgumentException("Folder " + path + " does not exist");
			}

			// get relative path of the folder
			var folderPath = Path.GetDirectoryName(path);

			return new LocalFolder(Fix(folderPath), fileInfo.Directory);
		}

		public long CountFiles(string path, string pattern, Func<string, bool> predicate, bool deep = true)
		{
			var files = SearchFiles(path, pattern, deep).AsParallel();

			if (predicate != null)
			{
				return files.Count(predicate);
			}
			else
			{
				return files.Count();
			}
		}

		public IEnumerable<string> SearchFiles(string path, string pattern, bool deep = true)
		{
			// Get relative from absolute path
			var index = _storagePath.EmptyNull().Length;

			return Directory.EnumerateFiles(MapStorage(path), pattern, deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
				.Select(x => x.Substring(index));
		}

		public IEnumerable<IFile> ListFiles(string path)
		{
			var directoryInfo = new DirectoryInfo(MapStorage(path));

			if (!directoryInfo.Exists)
			{
				throw new ArgumentException("Directory " + path + " does not exist");
			}

			return directoryInfo
				.EnumerateFiles()
				.Where(fi => !IsHidden(fi))
				.Select<FileInfo, IFile>(fi => new LocalFile(Path.Combine(Fix(path), fi.Name), fi));
		}

		public IEnumerable<IFolder> ListFolders(string path)
		{
			var directoryInfo = new DirectoryInfo(MapStorage(path));

			if (!directoryInfo.Exists)
			{
				try
				{
					directoryInfo.Create();
				}
				catch (Exception ex)
				{
					if (ex.IsFatal())
					{
						throw;
					}
					throw new ArgumentException(string.Format("The folder could not be created at path: {0}. {1}", path, ex));
				}
			}

			return directoryInfo
				.EnumerateDirectories()
				.Where(di => !IsHidden(di))
				.Select<DirectoryInfo, IFolder>(di => new LocalFolder(Path.Combine(Fix(path), di.Name), di));
		}

		private static bool IsHidden(FileSystemInfo di)
		{
			return (di.Attributes & FileAttributes.Hidden) != 0;
		}

		public void CreateFolder(string path)
		{
			var directoryInfo = new DirectoryInfo(MapStorage(path));

			if (directoryInfo.Exists)
			{
				throw new ArgumentException("Directory " + path + " already exists");
			}

			Directory.CreateDirectory(directoryInfo.FullName);
		}

		public void DeleteFolder(string path)
		{
			var directoryInfo = new DirectoryInfo(MapStorage(path));

			if (!directoryInfo.Exists)
			{
				throw new ArgumentException("Directory " + path + " does not exist");
			}

			directoryInfo.Delete(true);
		}

		public void RenameFolder(string path, string newPath)
		{
			var sourceDirectory = new DirectoryInfo(MapStorage(path));
			if (!sourceDirectory.Exists)
			{
				throw new ArgumentException("Directory " + path + "does not exist");
			}

			var targetDirectory = new DirectoryInfo(MapStorage(newPath));
			if (targetDirectory.Exists)
			{
				throw new ArgumentException("Directory " + newPath + " already exists");
			}

			Directory.Move(sourceDirectory.FullName, targetDirectory.FullName);
		}

		public IFile CreateFile(string path)
		{
			var fileInfo = new FileInfo(MapStorage(path));

			if (fileInfo.Exists)
			{
				throw new ArgumentException("File " + path + " already exists");
			}

			// ensure the directory exists
			var dirName = Path.GetDirectoryName(fileInfo.FullName);
			if (!Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}

			File.WriteAllBytes(fileInfo.FullName, new byte[0]);

			return new LocalFile(Fix(path), fileInfo);
		}

		public async Task<IFile> CreateFileAsync(string path)
		{
			var fileInfo = new FileInfo(MapStorage(path));

			if (fileInfo.Exists)
			{
				throw new ArgumentException("File " + path + " already exists");
			}

			// ensure the directory exists
			var dirName = Path.GetDirectoryName(fileInfo.FullName);
			if (!Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}

			using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
			{
				await stream.WriteAsync(new byte[0], 0, 0);
			}

			return new LocalFile(Fix(path), fileInfo);
		}

		public void DeleteFile(string path)
		{
			var fileInfo = new FileInfo(MapStorage(path));

			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}

		public void RenameFile(string path, string newPath)
		{
			var sourceFileInfo = new FileInfo(MapStorage(path));
			if (!sourceFileInfo.Exists)
			{
				throw new ArgumentException("File " + path + " does not exist");
			}

			var targetFileInfo = new FileInfo(MapStorage(newPath));
			if (targetFileInfo.Exists)
			{
				throw new ArgumentException("File " + newPath + " already exists");
			}

			File.Move(sourceFileInfo.FullName, targetFileInfo.FullName);
		}

		public void CopyFile(string path, string newPath)
		{
			var sourceFileInfo = new FileInfo(MapStorage(path));
			if (!sourceFileInfo.Exists)
			{
				throw new ArgumentException("File " + path + " does not exist");
			}

			var targetFileInfo = new FileInfo(MapStorage(newPath));
			if (targetFileInfo.Exists)
			{
				throw new ArgumentException("File " + newPath + " already exists");
			}

			File.Copy(sourceFileInfo.FullName, targetFileInfo.FullName);
		}

		public void SaveStream(string path, Stream inputStream)
		{
			// Create the file.
			// The CreateFile method will map the still relative path
			var file = CreateFile(path);

			using (var outputStream = file.OpenWrite())
			{
				var buffer = new byte[8192];
				for (;;)
				{
					var length = inputStream.Read(buffer, 0, buffer.Length);
					if (length <= 0)
						break;
					outputStream.Write(buffer, 0, length);
				}
			}
		}

		public async Task SaveStreamAsync(string path, Stream inputStream)
		{
			// Create the file.
			// The CreateFile method will map the still relative path
			var file = await CreateFileAsync(path);

			using (var outputStream = file.OpenWrite())
			{
				var buffer = new byte[8192];
				for (;;)
				{
					var length = await inputStream.ReadAsync(buffer, 0, buffer.Length);
					if (length <= 0)
						break;
					await outputStream.WriteAsync(buffer, 0, length);
				}
			}
		}

		public string Combine(string path1, string path2)
		{
			return Path.Combine(path1, path2);
		}

		/// <summary>
		/// Determines if a path lies within the base path boundaries.
		/// If not, an exception is thrown.
		/// </summary>
		/// <param name="basePath">The base path which boundaries are not to be transposed.</param>
		/// <param name="mappedPath">The path to determine.</param>
		/// <rereturns>The mapped path if valid.</rereturns>
		/// <exception cref="ArgumentException">If the path is invalid.</exception>
		internal static string ValidatePath(string basePath, string mappedPath)
		{
			bool valid = false;
			string error = null;

			try
			{
				// Check that we are indeed within the storage directory boundaries
				valid = Path.GetFullPath(mappedPath).StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception exception)
			{
				// Make sure that if invalid for medium trust we give a proper exception
				valid = false;
				error = exception.Message;
			}

			if (!valid)
			{
				throw new ArgumentException($"{error ?? "Invalid path."} mappedPath: {mappedPath.NaIfEmpty()}");
			}

			return mappedPath;
		}


		private class LocalFile : IFile
		{
			private readonly string _path;
			private readonly FileInfo _fileInfo;
			private Size? _dimensions;

			public LocalFile(string path, FileInfo fileInfo)
			{
				_path = path;
				_fileInfo = fileInfo;
			}

			public string Path
			{
				get { return _path; }
			}

			public string Directory
			{
				get { return _path.Substring(0, _path.Length - Name.Length); }
			}

			public string Name
			{
				get { return _fileInfo.Name; }
			}

			public string Title
			{
				get { return System.IO.Path.GetFileNameWithoutExtension(_fileInfo.Name); }
			}

			public long Size
			{
				get { return _fileInfo.Length; }
			}

			public DateTime LastUpdated
			{
				get { return _fileInfo.LastWriteTime; }
			}

			public string Extension
			{
				get { return _fileInfo.Extension; }
			}

			public Size Dimensions
			{
				get
				{
					if (_dimensions == null)
					{
						try
						{
							var mime = MimeTypes.MapNameToMimeType(_fileInfo.Name);
							_dimensions = ImageHeader.GetDimensions(OpenRead(), mime, false);
						}
						catch
						{
							_dimensions = new Size();
						}
					}

					return _dimensions.Value;
				}
			}

			public bool Exists
			{
				get { return _fileInfo.Exists; }
			}

			public Stream OpenRead()
			{
				return new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
			}

			public Stream OpenWrite()
			{
				return new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
			}

			public Stream CreateFile()
			{
				return new FileStream(_fileInfo.FullName, FileMode.Truncate, FileAccess.ReadWrite, FileShare.Read, bufferSize: 4096, useAsync: true);
			}

			public Task<Stream> CreateFileAsync()
			{
				return Task.Run(() => CreateFile());
			}
		}

		private class LocalFolder : IFolder
		{
			private readonly string _path;
			private readonly DirectoryInfo _directoryInfo;

			public LocalFolder(string path, DirectoryInfo directoryInfo)
			{
				_path = path;
				_directoryInfo = directoryInfo;
			}

			public string Path
			{
				get { return _path; }
			}

			public string Name
			{
				get { return _directoryInfo.Name; }
			}

			public DateTime LastUpdated
			{
				get { return _directoryInfo.LastWriteTime; }
			}

			public long Size
			{
				get { return GetDirectorySize(_directoryInfo); }
			}

			public bool Exists
			{
				get { return _directoryInfo.Exists; }
			}

			public IFolder Parent
			{
				get
				{
					if (_directoryInfo.Parent != null)
					{
						return new LocalFolder(System.IO.Path.GetDirectoryName(_path), _directoryInfo.Parent);
					}
					throw new ArgumentException("Directory " + _directoryInfo.Name + " does not have a parent directory");
				}
			}

			private static long GetDirectorySize(DirectoryInfo directoryInfo)
			{
				long size = 0;

				FileInfo[] fileInfos = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in fileInfos)
				{
					if (!IsHidden(fileInfo))
					{
						size += fileInfo.Length;
					}
				}
				DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
				foreach (DirectoryInfo dInfo in directoryInfos)
				{
					if (!IsHidden(dInfo))
					{
						size += GetDirectorySize(dInfo);
					}
				}

				return size;
			}
		}

	}
}