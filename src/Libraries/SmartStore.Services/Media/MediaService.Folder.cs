using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;

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
            if (dupe != null)
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
                    var folderName = MediaHelper.NormalizeFolderName(folderNames[i]);
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
                if (destNames != null && _helper.CheckUniqueFileName(pathData.FileTitle, pathData.Extension, destNames, out var uniqueName))
                {
                    pathData.FileName = uniqueName;
                }
            }
        }

        public FolderDeleteResult DeleteFolder(string path, FileHandling fileHandling = FileHandling.SoftDelete)
        {
            Guard.NotEmpty(path, nameof(path));

            path = FolderService.NormalizePath(path);
            ValidateFolderPath(path, "DeleteFolder", nameof(path));

            var root = _folderService.GetNodeByPath(path);
            if (root == null)
            {
                throw _exceptionFactory.FolderNotFound(path);
            }

            // Collect all affected subfolders also
            var allNodes = root.FlattenNodes(true).Reverse().ToArray();
            var result = new FolderDeleteResult();

            using (new DbContextScope(autoDetectChanges: false))
            {
                using (_folderService.BeginScope(true))
                {
                    // Delete all from DB
                    foreach (var node in allNodes)
                    {
                        var folder = _folderService.GetFolderById(node.Value.Id);
                        if (folder != null)
                        {
                            InternalDeleteFolder(folder, node, root, result, fileHandling);
                        }
                    }
                }
            }

            return result;
        }

        private void InternalDeleteFolder(
            MediaFolder folder,
            TreeNode<MediaFolderNode> node,
            TreeNode<MediaFolderNode> root,
            FolderDeleteResult result,
            FileHandling strategy)
        {
            // (perf) We gonna check file tracks, so we should preload all tracks.
            _fileRepo.Context.LoadCollection(folder, (MediaFolder x) => x.Files, false, q => q.Include(f => f.Tracks));

            var files = folder.Files.ToList();
            var lockedFiles = new List<MediaFile>(files.Count);
            var trackedFiles = new List<MediaFile>(files.Count);

            // First delete files
            if (folder.Files.Any())
            {
                var albumId = strategy == FileHandling.MoveToRoot
                    ? _folderService.FindAlbum(folder.Id).Value.Id
                    : (int?)null;

                foreach (var batch in files.Slice(500))
                {
                    foreach (var file in batch)
                    {
                        if (strategy == FileHandling.Delete && file.Tracks.Any())
                        {
                            // Don't delete tracked files
                            trackedFiles.Add(file);
                            continue;
                        }

                        if (strategy == FileHandling.Delete)
                        {
                            try
                            {
                                result.DeletedFileNames.Add(file.Name);
                                DeleteFile(file, true);
                            }
                            catch (DeleteTrackedFileException)
                            {
                                trackedFiles.Add(file);
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
                            result.DeletedFileNames.Add(file.Name);
                        }
                        else if (strategy == FileHandling.MoveToRoot)
                        {
                            file.FolderId = albumId;
                            result.DeletedFileNames.Add(file.Name);
                        }
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

            if (lockedFiles.Count > 0)
            {
                var fullPath = CombinePaths(root.Value.Path, lockedFiles[0].Name);
                throw new IOException(T("Admin.Media.Exception.InUse", fullPath));
            }

            if (lockedFiles.Count == 0 && trackedFiles.Count == 0 && node.Children.All(x => result.DeletedFolderIds.Contains(x.Value.Id)))
            {
                // Don't delete folder if a containing file could not be deleted, 
                // any tracked file was found or any of its child folders could not be deleted..
                _folderService.DeleteFolder(folder);
                //_fileRepo.Context.SaveChanges();
                result.DeletedFolderIds.Add(folder.Id);
            }

            result.LockedFileNames = lockedFiles.Select(x => x.Name).ToList();
            result.TrackedFileNames = trackedFiles.Select(x => x.Name).ToList();
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