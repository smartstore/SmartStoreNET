using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using SmartStore.Utilities;

namespace SmartStore.Core.IO
{
	public class LocalFileSystem : IFileSystem
	{
		private readonly string _virtualPath;	// ~/base
		private readonly string _publicPath;	// /Shop/base
		private readonly string _storagePath;	// C:\SMNET\base

		public LocalFileSystem()
			: this(string.Empty)
		{
		}

		protected internal LocalFileSystem(string basePath)
		{
			// for testing purposes
			basePath = basePath.EmptyNull().EnsureStartsWith("/").EnsureEndsWith("/");

			_virtualPath = "~" + basePath;

			_storagePath = CommonHelper.MapPath(_virtualPath, false);

			var appPath = "";

			if (HostingEnvironment.IsHosted)
			{
				appPath = HostingEnvironment.ApplicationVirtualPath;
			}

			_publicPath = appPath.TrimEnd('/') + basePath;
		}

		/// <summary>
		/// Maps a relative path into the storage path.
		/// </summary>
		/// <param name="path">The relative path to be mapped.</param>
		/// <returns>The relative path combined with the storage path.</returns>
		private string MapStorage(string path)
		{
			var mappedPath = string.IsNullOrEmpty(path) ? _storagePath : Path.Combine(_storagePath, Fix(path));
			return ValidatePath(_storagePath, mappedPath);
		}

		/// <summary>
		/// Maps a relative path into the public path.
		/// </summary>
		/// <param name="path">The relative path to be mapped.</param>
		/// <returns>The relative path combined with the public path in an URL friendly format ('/' character for directory separator).</returns>
		private string MapPublic(string path)
		{
			return string.IsNullOrEmpty(path) ? _publicPath : Path.Combine(_publicPath, path).Replace(Path.DirectorySeparatorChar, '/').Replace(" ", "%20");
		}

		static string Fix(string path)
		{
			return string.IsNullOrEmpty(path)
					   ? ""
					   : Path.DirectorySeparatorChar != '/'
							 ? path.Replace('/', Path.DirectorySeparatorChar)
							 : path;
		}

		public string GetPublicUrl(string path)
		{
			return MapPublic(path);
		}

		public string GetStoragePath(string url)
		{
			if (url.HasValue())
			{
				if (url.StartsWith(_virtualPath))
				{
					return url.Substring(_virtualPath.Length).Replace('/', Path.DirectorySeparatorChar).Replace("%20", " ");
				}

				if (url.StartsWith(_publicPath))
				{
					return url.Substring(_publicPath.Length).Replace('/', Path.DirectorySeparatorChar).Replace("%20", " ");
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
			if (!fileInfo.Exists)
			{
				throw new ArgumentException("File " + path + " does not exist");
			}

			return new FileSystemStorageFile(Fix(path), fileInfo);
		}

		public IFolder GetFolder(string path)
		{
			var directoryInfo = new DirectoryInfo(MapStorage(path));

			if (!directoryInfo.Exists)
			{
				throw new ArgumentException("Folder " + path + " does not exist");
			}

			return new FileSystemStorageFolder(Fix(path), directoryInfo);
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

			return new FileSystemStorageFolder(Fix(folderPath), fileInfo.Directory);
		}

		public IEnumerable<string> SearchFiles(string path, string pattern)
		{
			// get relative from absolute path
			var index = _storagePath.EmptyNull().Length;

			return Directory.EnumerateFiles(MapStorage(path), pattern, SearchOption.AllDirectories)
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
				.GetFiles()
				.Where(fi => !IsHidden(fi))
				.Select<FileInfo, IFile>(fi => new FileSystemStorageFile(Path.Combine(Fix(path), fi.Name), fi))
				.ToList();
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
				.GetDirectories()
				.Where(di => !IsHidden(di))
				.Select<DirectoryInfo, IFolder>(di => new FileSystemStorageFolder(Path.Combine(Fix(path), di.Name), di))
				.ToList();
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

			return new FileSystemStorageFile(Fix(path), fileInfo);
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

			try
			{
				// Check that we are indeed within the storage directory boundaries
				valid = Path.GetFullPath(mappedPath).StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				// Make sure that if invalid for medium trust we give a proper exception
				valid = false;
			}

			if (!valid)
			{
				throw new ArgumentException("Invalid path");
			}

			return mappedPath;
		}


		private class FileSystemStorageFile : IFile
		{
			private readonly string _path;
			private readonly FileInfo _fileInfo;

			public FileSystemStorageFile(string path, FileInfo fileInfo)
			{
				_path = path;
				_fileInfo = fileInfo;
			}

			public string Path
			{
				get { return _path; }
			}

			public string Name
			{
				get { return _fileInfo.Name; }
			}

			public long Size
			{
				get { return _fileInfo.Length; }
			}

			public DateTime LastUpdated
			{
				get { return _fileInfo.LastWriteTime; }
			}

			public string FileType
			{
				get { return _fileInfo.Extension; }
			}

			public Stream OpenRead()
			{
				return new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);
			}

			public Stream OpenWrite()
			{
				return new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite);
			}

			public Stream CreateFile()
			{
				return new FileStream(_fileInfo.FullName, FileMode.Truncate, FileAccess.ReadWrite);
			}
		}

		private class FileSystemStorageFolder : IFolder
		{
			private readonly string _path;
			private readonly DirectoryInfo _directoryInfo;

			public FileSystemStorageFolder(string path, DirectoryInfo directoryInfo)
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

			public IFolder Parent
			{
				get
				{
					if (_directoryInfo.Parent != null)
					{
						return new FileSystemStorageFolder(System.IO.Path.GetDirectoryName(_path), _directoryInfo.Parent);
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