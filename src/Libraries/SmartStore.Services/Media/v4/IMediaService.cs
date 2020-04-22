using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    [Flags]
    public enum MediaLoadFlags
    {
        None = 0,
        WithBlob = 1 << 0,
        WithTags = 1 << 1,
        WithTracks = 1 << 2,
        WithFolder = 1 << 3,
        AsNoTracking  = 1 << 4,
        Full = WithBlob | WithTags | WithTracks | WithFolder,
        FullNoTracking = Full | AsNoTracking
    }

    public enum SpecialMediaFolder
    {
        AllFiles = -500,
        Trash = -400,
        Orphans = -300,
        TransientFiles = -200,
        UnassignedFiles = -100
    }

    public enum FileHandling
    {
        SoftDelete,
        MoveToRoot,
        Delete
    }

    public enum DuplicateFileHandling
    {
        ThrowError,
        Overwrite,
        Rename
    }

    public enum DuplicateEntryHandling
    {
        ThrowError,
        Overwrite,
        // Folder: Overwrite, File: Rename
        Rename,
        Skip
    }

    public partial interface IMediaService
    {
        /// <summary>
        /// Gets the underlying storage provider
        /// </summary>
        IMediaStorageProvider StorageProvider { get; }

        int CountFiles(MediaSearchQuery query);
        Task<int> CountFilesAsync(MediaSearchQuery query);
        FileCountResult CountFilesGrouped(MediaFilesFilter filter);
        MediaSearchResult SearchFiles(MediaSearchQuery query, Func<IQueryable<MediaFile>, IQueryable<MediaFile>> queryModifier, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        Task<MediaSearchResult> SearchFilesAsync(MediaSearchQuery query, Func<IQueryable<MediaFile>, IQueryable<MediaFile>> queryModifier, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);

        bool FileExists(string path);
        MediaFileInfo GetFileByPath(string path, MediaLoadFlags flags = MediaLoadFlags.None);
        MediaFileInfo GetFileById(int id, MediaLoadFlags flags = MediaLoadFlags.None);
        IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        bool CheckUniqueFileName(string path, out string newPath);
        string CombinePaths(params string[] paths);

        /// <summary>
        /// Tries to find an equal file by comparing the source stream to a list of files.
        /// </summary>
        /// <param name="source">The source stream to find a match for.</param>
        /// <param name="files">The sequence of files to seek within for duplicates.</param>
        /// <param name="leaveOpen">Whether to leave the <paramref name="source"/>source stream</param> open.
        /// <param name="equalFileId">Id of equal file if any.</param>
        /// <returns><c>true</c> when a duplicate file was found, <c>false</c> otherwise.</returns>
        bool FindEqualFile(Stream source, IEnumerable<MediaFile> files, bool leaveOpen, out int equalFileId);

        MediaFileInfo SaveFile(string path, Stream stream, bool isTransient = true, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        Task<MediaFileInfo> SaveFileAsync(string path, Stream stream, bool isTransient = true, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        void DeleteFile(MediaFile file, bool permanent);
        MediaFileInfo CopyFile(MediaFile file, string destinationFileName, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        MediaFileInfo MoveFile(MediaFile file, string destinationFileName);

        bool FolderExists(string path);
        MediaFolderInfo CreateFolder(string path);
        MediaFolderInfo MoveFolder(string path, string destinationPath);
        MediaFolderInfo CopyFolder(string path, string destinationPath, DuplicateEntryHandling dupeEntryHandling = DuplicateEntryHandling.Skip);
        void DeleteFolder(string path, FileHandling fileHandling = FileHandling.SoftDelete);

        MediaFileInfo ConvertMediaFile(MediaFile file);
        string GetUrl(MediaFileInfo file, ProcessImageQuery query, string host = null, bool doFallback = true);
    }
}
