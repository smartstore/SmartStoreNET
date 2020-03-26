using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public partial class MediaServiceFileSystemAdapter : IMediaFileSystem
    {
        private readonly IMediaService _mediaService;
        private readonly MediaHelper _mediaHelper;
        private readonly IFolderService _folderService;
        private readonly IMediaStorageProvider _storageProvider;
        private readonly string _mediaRootPath;

        public MediaServiceFileSystemAdapter(
            IMediaService mediaService, 
            IFolderService folderService,
            MediaHelper mediaHelper)
        {
            _mediaService = mediaService;
            _folderService = folderService;
            _mediaHelper = mediaHelper;
            _storageProvider = mediaService.StorageProvider;
            _mediaRootPath = "media4/"; // MediaFileSystem.GetMediaPublicPath(); // TODO: (mm) switch
        }

        protected string Fix(string path)
        {
            return path.Replace('\\', '/');
        }

        #region IFileSystem

        public bool IsCloudStorage
        {
            get => _storageProvider.IsCloudStorage;
        }

        public string Root => string.Empty;

        public string GetPublicUrl(IFile file, bool forCloud = false)
        {
            if (file is MediaFileInfo mediaFile)
            {
                return _mediaService.GetUrl(mediaFile, null, string.Empty);
            }

            throw new ArgumentException("Type of file must be '{0}'.".FormatInvariant(typeof(MediaFileInfo).FullName), nameof(file));
        }

        public string GetPublicUrl(string path, bool forCloud = false)
        {
            throw new NotSupportedException();
        }

        public string GetStoragePath(string url)
        {
            url = Fix(url).TrimStart('/');

            if (!url.StartsWith(_mediaRootPath, StringComparison.OrdinalIgnoreCase))
            {
                // Is a folder path, no need to strip off public URL stuff.
                return url;
            }
            
            // Strip off root, e.g. "media/"
            var path = url.Substring(_mediaRootPath.Length);

            // Strip off media id from path, e.g. "123/"
            var firstSlashIndex = path.IndexOf('/');
            
            return path.Substring(firstSlashIndex);
        }

        public string Combine(string path1, string path2)
        {
            return Fix(Path.Combine(path1, path2));
        }

        public IEnumerable<IFile> ListFiles(string path)
        {
            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw new DirectoryNotFoundException("Directory '" + path + "' does not exist.");
            }

            var query = new MediaSearchQuery
            {
                FolderId = node.Value.Id
            };

            return _mediaService.SearchFiles(query);
        }

        public IEnumerable<IFolder> ListFolders(string path)
        {
            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw new DirectoryNotFoundException("Directory '" + path + "' does not exist.");
            }

            return node.Children.Select(x => new MediaFolderInfo(x));
        }

        public long CountFiles(string path, string pattern, Func<string, bool> predicate, bool deep = true)
        {
            if (predicate == null)
            {
                var node = _folderService.GetNodeByPath(path);
                if (node == null)
                {
                    throw new DirectoryNotFoundException("Directory '" + path + "' does not exist.");
                }

                var query = new MediaSearchQuery
                {
                    FolderId = node.Value.Id,
                    DeepSearch = deep,
                    Term = pattern
                };

                return _mediaService.CountFiles(query);
            }

            var files = SearchFiles(path, pattern, deep);
            return files.Count(predicate);
        }

        public IEnumerable<string> SearchFiles(string path, string pattern, bool deep = true)
        {
            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw new DirectoryNotFoundException("Directory '" + path + "' does not exist.");
            }
            
            var query = new MediaSearchQuery
            {
                FolderId = node.Value.Id,
                DeepSearch = deep,
                Term = pattern
            };

            return _mediaService.SearchFiles(query).Select(x => x.Path).ToList();
        }

        public IFile CreateFile(string path)
        {
            return _mediaService.CreateFile(path);
        }

        public Task<IFile> CreateFileAsync(string path)
        {
            return Task.FromResult(CreateFile(path));
        }

        public void CreateFolder(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            var file = _mediaService.GetFileByPath(path);
            if (file?.Exists == true)
            {
                _mediaService.DeleteFile(file, false);
            }
        }

        public void DeleteFolder(string path)
        {
            var node = _folderService.GetNodeByPath(path);
            if (node != null)
            {
                var folder = _folderService.GetFolderById(node.Value.Id);
                if (folder != null)
                {
                    _folderService.DeleteFolder(folder);
                }
            }
        }

        public bool FileExists(string path)
        {
            return _mediaService.FileExists(path);
        }

        public bool FolderExists(string path)
        {
            return _folderService.FolderExists(path);
        }

        public IFile GetFile(string path)
        {
            return _mediaService.GetFileByPath(path);
        }

        public IFolder GetFolder(string path)
        {
            var node = _folderService.GetNodeByPath(path);
            if (node != null)
            {
                return new MediaFolderInfo(node);
            }

            // TODO: (mm) return Node > .Exists = false;
            return null;
        }

        public IFolder GetFolderForFile(string path)
        {
            var dir = Fix(Path.GetDirectoryName(path));
            return GetFolder(dir);
        }

        public void CopyFile(string path, string newPath, bool overwrite = false)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(newPath, nameof(newPath));

            var sourceFile = (MediaFile)_mediaService.GetFileByPath(path);
            if (sourceFile == null)
            {
                throw new FileNotFoundException("File " + path + " does not exist.");
            }

            var targetFile = _mediaService.GetFileByPath(newPath);
            if (targetFile != null)
            {
                throw new ArgumentException("File " + newPath + " already exists.");
            }

            if (!_mediaHelper.TokenizePath(newPath, out var newPathData))
            {
                throw new DirectoryNotFoundException("Directory " + Fix(Path.GetDirectoryName(newPath)) + " does not exist.");
            }

            if (sourceFile.FolderId != newPathData.Folder.Id)
            {
                targetFile = _mediaService.CopyFile(sourceFile, newPathData.Folder.Id);
                if (!targetFile.Name.IsCaseInsensitiveEqual(newPathData.FileName))
                {
                    _mediaService.RenameFile(sourceFile, newPathData.FileName);
                }
            }
        }

        public void RenameFile(string path, string newPath)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(newPath, nameof(newPath));

            var sourceFile = (MediaFile)_mediaService.GetFileByPath(path);
            if (sourceFile == null)
            {
                throw new FileNotFoundException("File " + path + " does not exist.");
            }

            var targetFile = _mediaService.GetFileByPath(newPath);
            if (targetFile != null)
            {
                throw new ArgumentException("File " + newPath + " already exists.");
            }

            if (!_mediaHelper.TokenizePath(newPath, out var newPathData))
            {
                throw new DirectoryNotFoundException("Directory " + Fix(Path.GetDirectoryName(newPath)) + " does not exist.");
            }

            if (!sourceFile.Name.IsCaseInsensitiveEqual(newPathData.FileName))
            {
                _mediaService.RenameFile(sourceFile, newPathData.FileName);
            }

            if (sourceFile.FolderId != newPathData.Folder.Id)
            {
                _mediaService.MoveFile(sourceFile, newPathData.Folder.Id);
            }
        }

        public void RenameFolder(string path, string newPath)
        {
            throw new NotImplementedException();
        }

        public void SaveStream(string path, Stream inputStream)
        {
            throw new NotImplementedException();
        }

        public Task SaveStreamAsync(string path, Stream inputStream)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
