using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using System.Runtime.CompilerServices;
using SmartStore.Collections;
using SmartStore.Core.Localization;

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

            var dupe = _folderService.GetNodeByPath(path);
            if (_folderService.GetNodeByPath(path) != null)
            {
                throw _exceptionFactory.DuplicateFolder(path, dupe.Value);
            }

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
                            if (i == 0) throw new NotSupportedException(T("Admin.Media.Exception.TopLevelAlbum", path));
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
                throw _exceptionFactory.FolderNotFound(path);
            }

            if (node.Value.IsAlbum)
            {
                throw new NotSupportedException(T("Admin.Media.Exception.AlterRootAlbum", node.Value.Name));
            }

            var folder = _folderService.GetFolderById(node.Value.Id);
            if (folder == null)
            {
                throw _exceptionFactory.FolderNotFound(path);
            }

            ValidateFolderPath(destinationPath, "MoveFolder", nameof(destinationPath));

            // Destination must not exist
            if (FolderExists(destinationPath))
            {
                throw new ArgumentException(T("Admin.Media.Exception.DuplicateFolder", destinationPath));
            }

            var destParent = FolderService.NormalizePath(Path.GetDirectoryName(destinationPath));

            // Get destination parent
            var destParentNode = _folderService.GetNodeByPath(destParent);
            if (destParentNode == null)
            {
                throw _exceptionFactory.FolderNotFound(destinationPath);
            }

            // Cannot move outside source album
            if (!_folderService.AreInSameAlbum(folder.Id, destParentNode.Value.Id))
            {
                throw _exceptionFactory.NotSameAlbum(node.Value.Path, destParent);
            }

            if (destParentNode.IsDescendantOfOrSelf(node))
            {
                throw new ArgumentException(T("Admin.Media.Exception.DescendantFolder", destinationPath, node.Value.Path));
            }

            // Set new values
            folder.ParentId = destParentNode.Value.Id;
            folder.Name = Path.GetFileName(destinationPath);

            // Commit
            _folderService.UpdateFolder(folder);

            _folderService.ClearCache();

            return new MediaFolderInfo(_folderService.GetNodeById(folder.Id));
        }

        public FolderOperationResult CopyFolder(string path, string destinationPath, DuplicateEntryHandling dupeEntryHandling = DuplicateEntryHandling.Skip)
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(destinationPath, nameof(destinationPath));

            path = FolderService.NormalizePath(path);
            ValidateFolderPath(path, "CopyFolder", nameof(path));

            destinationPath = FolderService.NormalizePath(destinationPath);
            if (destinationPath.EnsureEndsWith("/").StartsWith(path.EnsureEndsWith("/")))
            {
                throw new ArgumentException(T("Admin.Media.Exception.DescendantFolder", destinationPath, path), nameof(destinationPath));
            }

            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw _exceptionFactory.FolderNotFound(path);
            }

            if (node.Value.IsAlbum)
            {
                throw new NotSupportedException(T("Admin.Media.Exception.CopyRootAlbum", node.Value.Name));
            }

            using (new DbContextScope(autoCommit: false, validateOnSave: false, autoDetectChanges: false))
            {
                destinationPath += "/" + node.Value.Name;
                var dupeFiles = new List<DuplicateFileInfo>();
                
                // >>>> Do the heavy stuff
                var folder = InternalCopyFolder(node, destinationPath, dupeEntryHandling, dupeFiles);

                var result = new FolderOperationResult
                {
                    Operation = "copy",
                    DuplicateEntryHandling = dupeEntryHandling,
                    Folder = folder,
                    DuplicateFiles = dupeFiles
                };

                return result;
            }
        }

        private MediaFolderInfo InternalCopyFolder(TreeNode<MediaFolderNode> sourceNode, string destPath, DuplicateEntryHandling dupeEntryHandling, IList<DuplicateFileInfo> dupeFiles)
        {
            // Get dest node
            var destNode = _folderService.GetNodeByPath(destPath);

            // Dupe handling
            if (destNode != null && dupeEntryHandling == DuplicateEntryHandling.ThrowError)
            {
                throw _exceptionFactory.DuplicateFolder(sourceNode.Value.Path, destNode.Value);
            }

            var doDupeCheck = destNode != null;

            // Create dest folder
            if (destNode == null)
            {
                destNode = CreateFolder(destPath);
            }

            var ctx = _fileRepo.Context;

            // INFO: we gonna change file name during the files loop later.
            var destPathData = new MediaPathData(destNode, "placeholder.txt");

            // Get all source files in one go
            var files = _searcher.SearchFiles(
                new MediaSearchQuery { FolderId = sourceNode.Value.Id },
                MediaLoadFlags.AsNoTracking | MediaLoadFlags.WithTags).Load();

            IDictionary<string, MediaFile> destFiles = null;
            HashSet<string> destNames = null;

            if (doDupeCheck)
            {
                // Get all files in destination folder for faster dupe selection
                destFiles = _searcher.SearchFiles(new MediaSearchQuery { FolderId = destNode.Value.Id }, MediaLoadFlags.None).ToDictionarySafe(x => x.Name);

                // Make a HashSet from all file names in the destination folder for faster unique file name lookups
                destNames = new HashSet<string>(destFiles.Keys, StringComparer.CurrentCultureIgnoreCase);
            }

            // Holds source and copy together, 'cause we perform a two-pass copy (file first, then data)
            var tuples = new List<(MediaFile, MediaFile)>(500);

            // Copy files batched
            foreach (var batch in files.Slice(500))
            {
                foreach (var file in batch)
                {
                    destPathData.FileName = file.Name;

                    // >>> Do copy
                    var copy = InternalCopyFile(
                        file,
                        destPathData,
                        false /* copyData */,
                        dupeEntryHandling,
                        () => destFiles?.Get(file.Name),
                        UniqueFileNameChecker,
                        out var isDupe);

                    if (copy != null)
                    {
                        if (isDupe)
                        {
                            dupeFiles.Add(new DuplicateFileInfo
                            {
                                SourceFile = ConvertMediaFile(file, sourceNode.Value),
                                DestinationFile = ConvertMediaFile(copy, destNode.Value),
                                UniquePath = destPathData.FullPath
                            });
                        }
                        if (!isDupe || dupeEntryHandling != DuplicateEntryHandling.Skip)
                        {
                            // When dupe: add to processing queue only if file was NOT skipped
                            tuples.Add((file, copy));
                        }
                    }
                }

                // Save batch to DB (1st pass)
                ctx.SaveChanges();

                // Now copy file data
                foreach (var op in tuples)
                {
                    InternalCopyFileData(op.Item1, op.Item2);
                }

                // Save batch to DB (2nd pass)
                ctx.SaveChanges();

                ctx.DetachEntities<MediaFolder>();
                ctx.DetachEntities<MediaFile>();
                tuples.Clear();
            }

            // Copy folders
            foreach (var node in sourceNode.Children)
            {
                destPath = destNode.Value.Path + "/" + node.Value.Name;
                InternalCopyFolder(node, destPath, dupeEntryHandling, dupeFiles);
            }

            return new MediaFolderInfo(destNode);

            void UniqueFileNameChecker(MediaPathData pathData)
            {
                if (destNames != null && InternalCheckUniqueFileName(pathData.FileTitle, pathData.Extension, destNames, out var uniqueName))
                {
                    pathData.FileName = uniqueName;
                }
            }
        }

        public void DeleteFolder(string path, FileHandling fileHandling = FileHandling.SoftDelete)
        {
            Guard.NotEmpty(path, nameof(path));

            path = FolderService.NormalizePath(path);
            ValidateFolderPath(path, "DeleteFolder", nameof(path));

            var node = _folderService.GetNodeByPath(path);
            if (node == null)
            {
                throw _exceptionFactory.FolderNotFound(path);
            }

            // Collect all affected subfolder ids also
            var folderIds = _folderService.GetNodesFlattened(node.Value.Id, true).Select(x => x.Id).Reverse().ToArray();

            using (new DbContextScope(autoDetectChanges: false, autoCommit: false))
            {
                using (_folderService.BeginScope(true))
                {
                    // Delete all from DB
                    foreach (var folderId in folderIds)
                    {
                        var folder = _folderService.GetFolderById(folderId);
                        if (folder != null)
                        {
                            InternalDeleteFolder(folder, node.Value, fileHandling);
                        }
                    }
                }
            }
        }

        private int InternalDeleteFolder(MediaFolder folder, MediaFolderNode node, FileHandling strategy)
        {
            int numFiles = 0;
            List<MediaFile> lockedFiles = null;

            // First delete files
            if (folder.Files.Any())
            {
                var albumId = strategy == FileHandling.MoveToRoot 
                    ? _folderService.FindAlbum(folder.Id).Value.Id 
                    : (int?)null;

                var files = folder.Files.ToList();
                
                lockedFiles = new List<MediaFile>(files.Count);

                foreach (var batch in files.Slice(500))
                {
                    foreach (var file in batch)
                    {
                        if (strategy == FileHandling.Delete)
                        {
                            try
                            {
                                DeleteFile(file, true);
                            }
                            catch (IOException)
                            {
                                lockedFiles.Add(file);
                            }  
                        }
                        else if (strategy == FileHandling.SoftDelete)
                        {
                            DeleteFile(file, false);
                            file.FolderId = null;
                        }
                        else if (strategy == FileHandling.MoveToRoot)
                        {
                            file.FolderId = albumId;
                        }

                        numFiles++;
                    }

                    _fileRepo.Context.SaveChanges();
                }

                if (lockedFiles.Any())
                {
                    // Retry deletion of failed files due to locking.
                    // INFO: By default "LocalFileSystem" waits for 500ms until the lock is revoked or it throws.
                    foreach (var lockedFile in lockedFiles.ToArray())
                    {
                        try
                        {
                            DeleteFile(lockedFile, true);
                            lockedFiles.Remove(lockedFile);
                        }
                        catch { }
                    }

                    _fileRepo.Context.SaveChanges();
                }
            }

            if (lockedFiles == null || lockedFiles.Count == 0)
            {
                // Don't delete folder if a containing file could not be deleted.
                _folderService.DeleteFolder(folder);
                _fileRepo.Context.SaveChanges();
            }
            else
            {
                var fullPath = CombinePaths(node.Path, lockedFiles[0].Name);
                throw new IOException(T("Admin.Media.Exception.InUse", fullPath));
            }

            return numFiles;
        }

        #endregion

        #region Utils

        private void ValidateFolderPath(string path, string operation, string paramName)
        {
            if (!IsPath(path))
            {
                // Destination cannot be an album                
                throw new ArgumentException(T("Admin.Media.Exception.PathSpecification", path, operation), paramName);
            }
        }

        #endregion
    }
}
