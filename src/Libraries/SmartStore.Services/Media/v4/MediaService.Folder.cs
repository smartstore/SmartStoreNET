using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Services.Media.Storage;
using SmartStore.Core.IO;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace SmartStore.Services.Media
{
    public partial class MediaService : IMediaService
    {
        #region Folder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FolderExists(string path)
        {
            return _folderService.GetNodeByPath(path) != null;
        }

        public MediaFolderInfo CreateFolder(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            path = FolderService.NormalizePath(path, false);
            ValidateFolderPath(path, "CreateFolder", nameof(path));

            var sep = "/";
            var folderNames = path.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
            bool flag = false;
            int folderId = 0;

            path = string.Empty;

            using (_folderService.BeginScope(true))
            {
                for (int i = 0; i < folderNames.Length; i++)
                {
                    var folderName = folderNames[i].ToValidPath();
                    path += (i > 0 ? sep : string.Empty) + folderName;

                    if (!flag)
                    {
                        // Find the last existing node in path trail
                        var currentNode = _folderService.GetNodeByPath(path)?.Value;
                        if (currentNode != null)
                        {
                            folderId = currentNode.Id;
                        }
                        else
                        {
                            if (i == 0) throw new NotSupportedException($"Creating top-level (album) folders is not supported. Folder: {path}.");
                            flag = true;
                        }
                    }

                    if (flag)
                    {
                        // Create missing folders in trail
                        using (new DbContextScope(autoCommit: true))
                        {
                            var mediaFolder = new MediaFolder { Name = folderName, ParentId = folderId };
                            _folderService.InsertFolder(mediaFolder);
                            folderId = mediaFolder.Id;
                        }
                    }
                }
            }

            return new MediaFolderInfo(_folderService.GetNodeById(folderId));
        }

        public MediaFolderInfo MoveFolder(string path, string destinationPath)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(destinationPath, nameof(destinationPath));

            path = FolderService.NormalizePath(path);
            ValidateFolderPath(path, "MoveFolder", nameof(path));

            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw new MediaFolderNotFoundException(path);
            }

            if (node.Value.IsAlbum)
            {
                throw new NotSupportedException($"Moving or renaming root album folders is not supported. Folder: {node.Value.Name}.");
            }

            var folder = _folderService.GetFolderById(node.Value.Id);
            if (folder == null)
            {
                throw new MediaFolderNotFoundException(path);
            }

            ValidateFolderPath(destinationPath, "MoveFolder", nameof(destinationPath));

            // Destination must not exist
            if (FolderExists(destinationPath))
            {
                throw new ArgumentException("Folder '" + destinationPath + "' already exists.");
            }

            var destParent = FolderService.NormalizePath(Path.GetDirectoryName(destinationPath));

            // Get destination parent
            var destParentNode = _folderService.GetNodeByPath(destParent);
            if (destParentNode == null)
            {
                throw new MediaFolderNotFoundException(destinationPath);
            }

            // Cannot move outside source album
            if (!_folderService.AreInSameAlbum(folder.Id, destParentNode.Value.Id))
            {
                throw new NotSameAlbumException(node.Value.Path, destParent);
            }

            if (destParentNode.IsDescendantOf(node))
            {
                throw new ArgumentException("Destination folder '" + destinationPath + "' is not allowed to be a descendant of source folder '" + node.Value.Path + "'.");
            }

            // Set new values
            folder.ParentId = destParentNode.Value.Id;
            folder.Name = Path.GetFileName(destinationPath);

            // Commit
            _folderService.UpdateFolder(folder);

            _folderService.ClearCache();

            return new MediaFolderInfo(_folderService.GetNodeById(folder.Id));
        }

        public MediaFolderInfo CopyFolder(string path, string destinationPath, bool overwrite = false)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(destinationPath, nameof(destinationPath));

            path = FolderService.NormalizePath(path);
            ValidateFolderPath(path, "MoveFolder", nameof(path));

            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw new MediaFolderNotFoundException(path);
            }

            if (node.Value.IsAlbum)
            {
                throw new NotSupportedException($"Moving or renaming root album folders is not supported. Folder: {node.Value.Name}.");
            }

            var folder = _folderService.GetFolderById(node.Value.Id);
            if (folder == null)
            {
                throw new MediaFolderNotFoundException(path);
            }

            ValidateFolderPath(destinationPath, "MoveFolder", nameof(destinationPath));

            // Destination must not exist
            if (FolderExists(destinationPath))
            {
                throw new ArgumentException("Folder '" + destinationPath + "' already exists.");
            }

            var destParent = FolderService.NormalizePath(Path.GetDirectoryName(destinationPath));

            // Get destination parent
            var destParentNode = _folderService.GetNodeByPath(destParent);
            if (destParentNode == null)
            {
                throw new MediaFolderNotFoundException(destinationPath);
            }

            // Cannot move outside source album
            if (!_folderService.AreInSameAlbum(folder.Id, destParentNode.Value.Id))
            {
                throw new NotSameAlbumException(node.Value.Path, destParent);
            }

            if (destParentNode.IsDescendantOf(node))
            {
                throw new ArgumentException("Destination folder '" + destinationPath + "' is not allowed to be a descendant of source folder '" + node.Value.Path + "'.");
            }

            // Set new values
            folder.ParentId = destParentNode.Value.Id;
            folder.Name = Path.GetFileName(destinationPath);

            // Commit
            _folderService.UpdateFolder(folder);

            _folderService.ClearCache();

            return new MediaFolderInfo(_folderService.GetNodeById(folder.Id));
        }

        public void DeleteFolder(string path, FileDeleteStrategy strategy = FileDeleteStrategy.SoftDelete)
        {
            Guard.NotEmpty(path, nameof(path));

            path = FolderService.NormalizePath(path);
            ValidateFolderPath(path, "DeleteFolder", nameof(path));

            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw new MediaFolderNotFoundException(path);
            }

            // Collect all affected subfolder ids also
            var folderIds = _folderService.GetNodesFlattened(node.Value.Id, true).Select(x => x.Id).Reverse().ToArray();

            using (new DbContextScope(autoCommit: false))
            {
                using (_folderService.BeginScope(true))
                {
                    // Delete all from DB
                    foreach (var folderId in folderIds)
                    {
                        var folder = _folderService.GetFolderById(folderId);
                        if (folder != null)
                        {
                            InternalDeleteFolder(folder, strategy);
                        }
                    }
                }
            }
        }

        private int InternalDeleteFolder(MediaFolder folder, FileDeleteStrategy strategy)
        {
            int numFiles = 0;

            // First delete files
            if (folder.Files.Any())
            {
                var albumId = strategy == FileDeleteStrategy.MoveToRoot 
                    ? _folderService.FindAlbum(folder.Id).Value.Id 
                    : (int?)null;

                foreach (var batch in folder.Files.Slice(500))
                {
                    foreach (var file in batch)
                    {
                        numFiles++;

                        if (strategy == FileDeleteStrategy.Delete)
                        {
                            DeleteFile(file, true);
                        }
                        else if (strategy == FileDeleteStrategy.SoftDelete)
                        {
                            file.Deleted = true;
                            file.FolderId = null;
                        }
                        else if (strategy == FileDeleteStrategy.MoveToRoot)
                        {
                            file.FolderId = albumId;
                        }
                    }

                    _fileRepo.Context.SaveChanges();
                }     
            }

            // Now delete folder itself
            _folderService.DeleteFolder(folder);
            
            _fileRepo.Context.SaveChanges();

            return numFiles;
        }

        #endregion

        #region Utils

        private static void ValidateFolderPath(string path, string operation, string paramName)
        {
            if (!IsPath(path))
            {
                // Destination cannot be an album
                throw new ArgumentException("Invalid path specification '" + path + "' for '" + operation + "' operation.", paramName);
            }
        }

        #endregion
    }
}
