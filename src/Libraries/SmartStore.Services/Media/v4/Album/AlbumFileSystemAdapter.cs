using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public partial class AlbumFileSystemAdapter : IFileSystem
    {
        private readonly IMediaService _mediaService;
        private readonly IFolderService _folderService;
        private readonly IMediaStorageProvider _storageProvider;
        private readonly string _rootPath;

        public AlbumFileSystemAdapter(
            AlbumInfo album,
            IMediaService mediaService,
            IFolderService folderService,
            IMediaStorageProvider storageProvider)
        {
            _mediaService = mediaService;
            _folderService = folderService;
            _storageProvider = storageProvider;

            Album = album;
            Node = _folderService.GetNodeById(album.Id);

            _rootPath = Node.Value.Path;
        }

        public AlbumInfo Album { get; }

        public TreeNode<MediaFolderNode> Node { get; }

        protected string MakeAbsolute(string path)
        {
            return Combine(_rootPath, path);
        }

        protected string MakeRelative(string path)
        {
            return Combine(_rootPath, path);
        }

        protected string Fix(string path)
        {
            return path.Replace('\\', '/');
        }

        #region IFileSystem

        public bool IsCloudStorage => false;

        public string Root => _rootPath;

        public string Combine(string path1, string path2)
        {
            return Fix(Path.Combine(path1, path2));
        }

        public void CopyFile(string path, string newPath, bool overwrite = false)
        {
            var sourceFile = _mediaService.GetFileByPath(MakeAbsolute(path));
            if (sourceFile == null)
            {
                throw new ArgumentException("File '" + path + "' does not exist.");
            }

            _mediaService.CopyFile(sourceFile, MakeAbsolute(newPath), overwrite);
        }

        public IEnumerable<IFile> ListFiles(string path)
        {
            var node = _folderService.GetNodeByPath(MakeAbsolute(path));
            if (node != null)
            {
                throw new ArgumentException("Directory '" + path + "' does not exist.");
            }

            var query = new MediaSearchQuery
            {
                FolderId = node.Value.Id
            };

            return _mediaService.SearchFiles(query);
        }

        public IEnumerable<IFolder> ListFolders(string path)
        {
            var node = _folderService.GetNodeByPath(MakeAbsolute(path));
            if (node != null)
            {
                throw new ArgumentException("Directory '" + path + "' does not exist.");
            }

            return node.Children.Select(x => new MediaFolderInfo(x));
        }

        public long CountFiles(string path, string pattern, Func<string, bool> predicate, bool deep = true)
        {
            var node = _folderService.GetNodeByPath(MakeAbsolute(path));
            if (node != null)
            {
                return 0;
            }

            var query = new MediaSearchQuery
            {
                FolderId = node.Value.Id,
                DeepSearch = deep,
                Term = pattern
            };

            return _mediaService.CountFiles(query);
        }

        public IEnumerable<string> SearchFiles(string path, string pattern, bool deep = true)
        {
            var node = _folderService.GetNodeByPath(MakeAbsolute(path));
            if (node != null)
            {
                throw new ArgumentException("Directory '" + path + "' does not exist.");
            }

            var query = new MediaSearchQuery
            {
                FolderId = node.Value.Id,
                DeepSearch = deep,
                Term = pattern
            };

            // Get relative from absolute path
            var index = _rootPath.EmptyNull().Length;

            return _mediaService.SearchFiles(query).Select(x => x.Path.Substring(index));
        }

        public IFile CreateFile(string path)
        {
            return _mediaService.CreateFile(MakeAbsolute(path));
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
            var file = _mediaService.GetFileByPath(MakeAbsolute(path));
            if (file?.Exists == true)
            {
                _mediaService.DeleteFile(file, false);
            }
        }

        public void DeleteFolder(string path)
        {
            var node = _folderService.GetNodeByPath(MakeAbsolute(path));
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
            return _mediaService.FileExists(MakeAbsolute(path));
        }

        public bool FolderExists(string path)
        {
            return _folderService.FolderExists(MakeAbsolute(path));
        }

        public IFile GetFile(string path)
        {
            return _mediaService.GetFileByPath(MakeAbsolute(path));
        }

        public IFolder GetFolder(string path)
        {
            var node = _folderService.GetNodeByPath(MakeAbsolute(path));
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

        public string GetPublicUrl(string path, bool forCloud = false)
        {
            throw new NotImplementedException();
        }

        public string GetStoragePath(string url)
        {
            throw new NotImplementedException();
        }

        public void RenameFile(string path, string newPath)
        {
            throw new NotImplementedException();
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
