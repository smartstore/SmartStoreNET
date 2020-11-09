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
        private readonly string _root;
        private readonly string _publicPath;        // /Shop/base
        private string _storagePath;    // C:\SMNET\base	
        private bool _isCloudStorage;   // When public URL is outside of current app	

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

            var pathIsAbsolute = PathHelper.IsAbsolutePhysicalPath(basePath);

            NormalizeStoragePath(ref basePath, pathIsAbsolute);

            _publicPath = NormalizePublicPath(publicPath, basePath, pathIsAbsolute);

            _root = basePath;
        }

        public bool IsCloudStorage => _isCloudStorage;

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

        public string Root => _root;

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

        public string GetPublicUrl(IFile file, bool forCloud = false)
        {
            return MapPublic(file.Path);
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
            return new LocalFile(path, MapStorage(path));
        }

        public IFolder GetFolder(string path)
        {
            return new LocalFolder(path, MapStorage(path));
        }

        public IFolder GetFolderForFile(string path)
        {
            var fileInfo = new FileInfo(MapStorage(path));
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File " + path + " does not exist");
            }

            if (!fileInfo.Directory.Exists)
            {
                throw new DirectoryNotFoundException("Folder " + path + " does not exist");
            }

            // Get relative path of the folder
            var folderPath = Path.GetDirectoryName(path);

            return new LocalFolder(folderPath, fileInfo.Directory);
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

            return Directory
                .EnumerateFiles(MapStorage(path), pattern, deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(x => x.Substring(index));
        }

        public IEnumerable<IFile> ListFiles(string path)
        {
            var directoryInfo = new DirectoryInfo(MapStorage(path));

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException("Directory " + path + " does not exist");
            }

            return directoryInfo
                .EnumerateFiles()
                .Where(fi => !IsHidden(fi))
                .Select<FileInfo, IFile>(fi => new LocalFile(Path.Combine(path, fi.Name), fi));
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
                .Select<DirectoryInfo, IFolder>(di => new LocalFolder(Path.Combine(path, di.Name), di));
        }

        private static bool IsHidden(FileSystemInfo di)
        {
            return (di.Attributes & FileAttributes.Hidden) != 0;
        }

        public IFolder CreateFolder(string path)
        {
            return new LocalFolder(path, Directory.CreateDirectory(MapStorage(path)));
        }

        public void DeleteFolder(string path)
        {
            var directoryInfo = new DirectoryInfo(MapStorage(path));

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException("Directory " + path + " does not exist.");
            }

            directoryInfo.Delete(true);
        }

        public void RenameFolder(string path, string newPath)
        {
            var sourceDirectory = new DirectoryInfo(MapStorage(path));
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Directory " + path + "does not exist.");
            }

            var targetDirectory = new DirectoryInfo(MapStorage(newPath));
            if (targetDirectory.Exists)
            {
                throw new ArgumentException("Directory " + newPath + " already exists.");
            }

            Directory.Move(sourceDirectory.FullName, targetDirectory.FullName);
        }

        public void CopyFolder(string path, string destinationPath, bool overwrite = true)
        {
            var sourceDirectory = new DirectoryInfo(MapStorage(path));
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Directory " + path + "does not exist.");
            }

            var targetPath = Combine(MapStorage(destinationPath), sourceDirectory.Name);
            var destDirectory = new DirectoryInfo(targetPath);
            if (!overwrite && destDirectory.Exists)
            {
                throw new ArgumentException("Directory " + destinationPath + " already exists.");
            }

            if (!destDirectory.Exists)
            {
                destDirectory.Create();
            }

            FileSystemHelper.CopyDirectory(sourceDirectory, destDirectory, overwrite);
        }

        public bool CheckUniqueFileName(string path, out string newPath)
        {
            Guard.NotEmpty(path, nameof(path));

            newPath = null;

            var file = GetFile(path);
            if (!file.Exists)
            {
                return false;
            }

            var pattern = string.Concat(file.Title, "-*", file.Extension);
            var dir = file.Directory;
            var files = new HashSet<string>(SearchFiles(dir, pattern, false).Select(x => Path.GetFileName(x)), StringComparer.OrdinalIgnoreCase);

            int i = 1;
            while (true)
            {
                var fileName = string.Concat(file.Title, "-", i, file.Extension);
                if (!files.Contains(fileName))
                {
                    // Found our gap
                    newPath = Combine(dir, fileName);
                    return true;
                }

                i++;
            }
        }

        public IFile CreateFile(string path)
        {
            var fileInfo = new FileInfo(MapStorage(path));

            if (fileInfo.Exists)
            {
                throw new ArgumentException("File " + path + " already exists");
            }

            // Ensure the directory exists
            var dirName = Path.GetDirectoryName(fileInfo.FullName);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            File.WriteAllBytes(fileInfo.FullName, new byte[0]);

            fileInfo.Refresh();
            return new LocalFile(path, fileInfo);
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

            fileInfo.Refresh();
            return new LocalFile(path, fileInfo);
        }

        public void DeleteFile(string path)
        {
            var fileInfo = new FileInfo(MapStorage(path));

            if (fileInfo.Exists)
            {
                WaitForUnlockAndExecute(fileInfo, x => x.Delete());
            }
        }

        public void RenameFile(string path, string newPath)
        {
            var sourceFileInfo = new FileInfo(MapStorage(path));
            if (!sourceFileInfo.Exists)
            {
                throw new FileNotFoundException("File " + path + " does not exist.");
            }

            var targetFileInfo = new FileInfo(MapStorage(newPath));
            if (targetFileInfo.Exists)
            {
                throw new ArgumentException("File " + newPath + " already exists.");
            }

            WaitForUnlockAndExecute(sourceFileInfo, x => File.Move(x.FullName, targetFileInfo.FullName));
        }

        public void CopyFile(string path, string newPath, bool overwrite = false)
        {
            var sourceFileInfo = new FileInfo(MapStorage(path));
            if (!sourceFileInfo.Exists)
            {
                throw new FileNotFoundException("File " + path + " does not exist");
            }

            var targetPath = MapStorage(newPath);
            if (!overwrite)
            {
                var targetFileInfo = new FileInfo(targetPath);
                if (targetFileInfo.Exists)
                {
                    throw new ArgumentException("File " + newPath + " already exists");
                }
            }

            WaitForUnlockAndExecute(sourceFileInfo, x => File.Copy(x.FullName, targetPath, overwrite));
        }

        public void SaveStream(string path, Stream inputStream)
        {
            using (var outputStream = File.OpenWrite(MapStorage(path)))
            {
                outputStream.SetLength(0);
                inputStream.CopyTo(outputStream);
            }
        }

        public async Task SaveStreamAsync(string path, Stream inputStream)
        {
            using (var outputStream = File.OpenWrite(MapStorage(path)))
            {
                outputStream.SetLength(0);
                await inputStream.CopyToAsync(outputStream);
            }
        }

        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        private static void WaitForUnlockAndExecute(FileInfo fi, Action<FileInfo> action)
        {
            try
            {
                action(fi);
            }
            catch (IOException)
            {
                if (!fi.WaitForUnlock(250))
                {
                    throw;
                }

                action(fi);
            }
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

        public class LocalFile : IFile
        {
            private readonly string _localPath;
            private readonly string _relativePath;
            private string _name;
            private FileInfo _fileInfo;
            private Size? _dimensions;

            public LocalFile(string relativePath, string localPath)
            {
                _relativePath = Fix(relativePath);
                _localPath = localPath;
            }

            public LocalFile(string relativePath, FileInfo fileInfo)
            {
                _relativePath = Fix(relativePath);
                _localPath = fileInfo.FullName;
                _fileInfo = fileInfo;
            }

            private FileInfo GetFileInfo()
            {
                return _fileInfo ?? (_fileInfo = new FileInfo(_localPath));
            }

            public string Path => _relativePath;

            public string Directory => _relativePath.Substring(0, _relativePath.Length - Name.Length);

            public string Name => _fileInfo?.Name ?? _name ?? (_name = System.IO.Path.GetFileName(_localPath));

            public string Title => System.IO.Path.GetFileNameWithoutExtension(_localPath);

            public long Size => GetFileInfo().Length;

            public DateTime LastUpdated => GetFileInfo().LastWriteTimeUtc;

            public string Extension => _fileInfo?.Extension ?? System.IO.Path.GetExtension(_localPath);

            public Size Dimensions
            {
                get
                {
                    if (_dimensions == null)
                    {
                        try
                        {
                            var mime = MimeTypes.MapNameToMimeType(Name);
                            _dimensions = ImageHeader.GetDimensions(OpenRead(), mime, false);
                        }
                        catch
                        {
                            _dimensions = new Size();
                        }
                    }

                    return _dimensions.Value;
                }
                internal set => _dimensions = value;
            }

            public bool Exists
            {
                get
                {
                    if (_fileInfo != null)
                        return _fileInfo.Exists;

                    return System.IO.File.Exists(_localPath);
                }
            }

            public Stream OpenRead()
            {
                return new FileStream(_localPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            }

            public Stream OpenWrite()
            {
                var fi = GetFileInfo();
                if (!fi.Directory.Exists)
                {
                    System.IO.Directory.CreateDirectory(fi.Directory.FullName);
                }

                return new FileStream(_localPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            }

            public Stream CreateFile()
            {
                return new FileStream(_localPath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.Read, bufferSize: 4096, useAsync: true);
            }

            public Task<Stream> CreateFileAsync()
            {
                return Task.Run(() => CreateFile());
            }
        }

        public class LocalFolder : IFolder
        {
            private readonly string _localPath;
            private readonly string _relativePath;
            private string _name;
            private DirectoryInfo _dirInfo;

            public LocalFolder(string relativePath, string localPath)
            {
                _relativePath = Fix(relativePath);
                _localPath = localPath;
            }

            public LocalFolder(string relativePath, DirectoryInfo dirInfo)
            {
                _relativePath = Fix(relativePath);
                _localPath = dirInfo.FullName;
                _dirInfo = dirInfo;
            }

            private DirectoryInfo GetDirInfo()
            {
                return _dirInfo ?? (_dirInfo = new DirectoryInfo(_localPath));
            }

            public string Path => _relativePath;

            public string Name => _dirInfo?.Name ?? _name ?? (_name = GetDirName(_relativePath));

            public DateTime LastUpdated => GetDirInfo().LastWriteTimeUtc;

            public long Size => GetDirectorySize(GetDirInfo());

            public bool Exists
            {
                get
                {
                    if (_dirInfo != null)
                        return _dirInfo.Exists;

                    return System.IO.Directory.Exists(_localPath);
                }
            }

            public IFolder Parent
            {
                get
                {
                    var parent = GetDirInfo().Parent;

                    if (parent != null)
                    {
                        return new LocalFolder(System.IO.Path.GetDirectoryName(_relativePath), parent);
                    }

                    throw new InvalidOperationException("Directory " + _dirInfo.Name + " does not have a parent directory");
                }
            }

            private static string GetDirName(string fullPath)
            {
                if (fullPath.Length > 3)
                {
                    string path = fullPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                    return System.IO.Path.GetFileName(path);
                }

                return fullPath;
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