using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using SmartStore.Utilities;

namespace SmartStore.Core.IO.Media
{
    public class LocalFileSystem : IFileSystem 
	{
        private readonly string _storagePath;
        private readonly string _publicPath;

	    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	    public LocalFileSystem()
        {
            var mediaPath = CommonHelper.MapPath("~/Media/", false);
            _storagePath = mediaPath;

            var appPath = "";

            if (HostingEnvironment.IsHosted)
			{
                appPath = HostingEnvironment.ApplicationVirtualPath;
            }

			appPath = appPath.EnsureStartsWith("/").EnsureEndsWith("/");

            _publicPath = appPath + "Media/";
        }

        private string Map(string path)
		{
            return string.IsNullOrEmpty(path) ? _storagePath : Path.Combine(_storagePath, path);
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

            return _publicPath + path.Replace(Path.DirectorySeparatorChar, '/');
        }

        public IFile GetFile(string path)
		{
            if (!File.Exists(Map(path)))
			{
                throw new ArgumentException("File " + path + " does not exist");
            }

            return new FileSystemStorageFile(Fix(path), new FileInfo(Map(path)));
        }

		public IEnumerable<string> SearchFiles(string path, string pattern)
		{
			return Directory.EnumerateFiles(Map(path), pattern);
		}

		public IEnumerable<IFile> ListFiles(string path)
		{
            if (!Directory.Exists(Map(path)))
			{
                throw new ArgumentException("Directory " + path + " does not exist");
            }

            return new DirectoryInfo(Map(path))
                .GetFiles()
                .Where(fi => !IsHidden(fi))
                .Select<FileInfo, IFile>(fi => new FileSystemStorageFile(Path.Combine(Fix(path), fi.Name), fi))
                .ToList();
        }

        public IEnumerable<IFolder> ListFolders(string path)
		{
            if (!Directory.Exists(Map(path)))
			{
                try
				{
                    Directory.CreateDirectory(Map(path));
                }
                catch (Exception ex)
				{
                    throw new ArgumentException(string.Format("The folder could not be created at path: {0}. {1}", path, ex));
                }
            }

            return new DirectoryInfo(Map(path))
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
            if (Directory.Exists(Map(path)))
			{
                throw new ArgumentException("Directory " + path + " already exists");
            }

            Directory.CreateDirectory(Map(path));
        }

        public void DeleteFolder(string path)
		{
            if (!Directory.Exists(Map(path)))
			{
                throw new ArgumentException("Directory " + path + " does not exist");
            }

            Directory.Delete(Map(path), true);
        }

        public void RenameFolder(string path, string newPath)
		{
            if (!Directory.Exists(Map(path)))
			{
                throw new ArgumentException("Directory " + path + "does not exist");
            }

            if (Directory.Exists(Map(newPath)))
			{
                throw new ArgumentException("Directory " + newPath + " already exists");
            }

            Directory.Move(Map(path), Map(newPath));
        }

        public IFile CreateFile(string path)
		{
            if (File.Exists(Map(path)))
			{
                throw new ArgumentException("File " + path + " already exists");
            }

            var fileInfo = new FileInfo(Map(path));
            File.WriteAllBytes(Map(path), new byte[0]);

            return new FileSystemStorageFile(Fix(path), fileInfo);
        }

        public void DeleteFile(string path)
		{
            if (!File.Exists(Map(path)))
			{
                throw new ArgumentException("File " + path + " does not exist");
            }

            File.Delete(Map(path));
        }

        public void RenameFile(string path, string newPath)
		{
            if (!File.Exists(Map(path)))
			{
                throw new ArgumentException("File " + path + " does not exist");
            }

            if (File.Exists(Map(newPath)))
			{
                throw new ArgumentException("File " + newPath + " already exists");
            }

            File.Move(Map(path), Map(newPath));
        }

        private class FileSystemStorageFile : IFile
		{
            private readonly string _path;
            private readonly FileInfo _fileInfo;

            public FileSystemStorageFile(string path, FileInfo fileInfo) {
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
        }

        private class FileSystemStorageFolder : IFolder {
            private readonly string _path;
            private readonly DirectoryInfo _directoryInfo;

            public FileSystemStorageFolder(string path, DirectoryInfo directoryInfo) {
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

            private static long GetDirectorySize(DirectoryInfo directoryInfo) {
                long size = 0;

                FileInfo[] fileInfos = directoryInfo.GetFiles();
                foreach (FileInfo fileInfo in fileInfos) {
                    if (!IsHidden(fileInfo)) {
                        size += fileInfo.Length;
                    }
                }
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                foreach (DirectoryInfo dInfo in directoryInfos) {
                    if (!IsHidden(dInfo)) {
                        size += GetDirectorySize(dInfo);
                    }
                }

                return size;
            }
        }

    }
}