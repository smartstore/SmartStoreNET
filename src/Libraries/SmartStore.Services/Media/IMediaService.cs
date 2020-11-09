using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media.Imaging;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    #region Enums

    [Flags]
    public enum MediaLoadFlags
    {
        None = 0,
        WithBlob = 1 << 0,
        WithTags = 1 << 1,
        WithTracks = 1 << 2,
        WithFolder = 1 << 3,
        AsNoTracking = 1 << 4,
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

    public enum MimeValidationType
    {
        NoValidation,
        MimeTypeMustMatch,
        MediaTypeMustMatch
    }

    #endregion

    #region Result & Cargo objects

    public class DuplicateFileInfo
    {
        [JsonProperty("source")]
        public MediaFileInfo SourceFile { get; set; }

        [JsonProperty("dest")]
        public MediaFileInfo DestinationFile { get; set; }

        [JsonProperty("uniquePath")]
        public string UniquePath { get; set; }
    }

    public class FolderOperationResult
    {
        public string Operation { get; set; }
        public MediaFolderInfo Folder { get; set; }
        public DuplicateEntryHandling DuplicateEntryHandling { get; set; }
        public IList<DuplicateFileInfo> DuplicateFiles { get; set; }
    }

    public class FolderDeleteResult
    {
        public HashSet<int> DeletedFolderIds { get; set; } = new HashSet<int>();
        public IList<string> DeletedFileNames { get; set; } = new List<string>();
        public IList<string> TrackedFileNames { get; set; } = new List<string>();
        public IList<string> LockedFileNames { get; set; } = new List<string>();
    }

    public class FileOperationResult
    {
        public string Operation { get; set; }
        public MediaFileInfo SourceFile { get; set; }
        public MediaFileInfo DestinationFile { get; set; }
        public DuplicateFileHandling DuplicateFileHandling { get; set; }
        public bool IsDuplicate { get; set; }
        public string UniquePath { get; set; }
    }

    #endregion

    /// <summary>
    /// Media service interface.
    /// </summary>
    public partial interface IMediaService
    {
        /// <summary>
        /// Gets the underlying storage provider
        /// </summary>
        IMediaStorageProvider StorageProvider { get; }

        /// <summary>
        /// Gets or sets a value indicating whether image post-processing is enabled.
        /// It is recommended to turn this off during long-running processes - like product imports -
        /// as post-processing can heavily decrease processing time.
        /// </summary>
        public bool ImagePostProcessingEnabled { get; set; }

        /// <summary>
        /// Determines the number of files that match the filter criteria in <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query that defines the criteria.</param>
        /// <returns>The number of matching files.</returns>
        int CountFiles(MediaSearchQuery query);

        /// <summary>
        /// Determines the number of files that match the filter criteria in <paramref name="query"/> asynchronously.
        /// </summary>
        /// <param name="query">The query that defines the criteria.</param>
        /// <returns>The number of matching files.</returns>
        Task<int> CountFilesAsync(MediaSearchQuery query);

        /// <summary>
        /// Determines the number of files that match the filter criteria in <paramref name="query"/> and groups them by folders.
        /// </summary>
        /// <param name="query">The filter that defines the criteria.</param>
        /// <returns>The grouped file counts (all, trash, unassigned, transient, all folders as dictionary)</returns>
        FileCountResult CountFilesGrouped(MediaFilesFilter filter);

        /// <summary>
        /// Searches files that match the filter criteria in <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query that defines the criteria.</param>
        /// <param name="queryModifier">An optional modifier function for the LINQ query that was internally derived from <paramref name="query"/>. Can be null.</param>
        /// <param name="flags">Flags that affect the loading behaviour (eager-loading, tracking etc.)</param>
        /// <returns>The search result.</returns>
        MediaSearchResult SearchFiles(MediaSearchQuery query, Func<IQueryable<MediaFile>, IQueryable<MediaFile>> queryModifier, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);

        /// <summary>
        /// Searches files that match the filter criteria in <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query that defines the criteria.</param>
        /// <param name="queryModifier">An optional modifier function for the LINQ query that was internally derived from <paramref name="query"/>. Can be null.</param>
        /// <param name="flags">Flags that affect the loading behaviour (eager-loading, tracking etc.)</param>
        /// <returns>The search result.</returns>
        Task<MediaSearchResult> SearchFilesAsync(MediaSearchQuery query, Func<IQueryable<MediaFile>, IQueryable<MediaFile>> queryModifier, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);

        bool FileExists(string path);
        MediaFileInfo GetFileByPath(string path, MediaLoadFlags flags = MediaLoadFlags.None);
        MediaFileInfo GetFileById(int id, MediaLoadFlags flags = MediaLoadFlags.None);
        MediaFileInfo GetFileByName(int folderId, string fileName, MediaLoadFlags flags = MediaLoadFlags.None);
        IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        bool CheckUniqueFileName(string path, out string newPath);
        string CombinePaths(params string[] paths);

        /// <summary>
        /// Tries to find an equal file by comparing the source stream to a list of files.
        /// </summary>
        /// <param name="source">The source stream to find a match for.</param>
        /// <param name="files">The sequence of files to seek within for duplicates.</param>
        /// <param name="leaveOpen">Whether to leave the <paramref name="source"/>source stream</param> open.
        /// <param name="equalFile">A file from the <paramref name="files"/> collection whose content is equal to <paramref name="source"/>.</param>
        /// <returns><c>true</c> when a duplicate file was found, <c>false</c> otherwise.</returns>
        bool FindEqualFile(Stream source, IEnumerable<MediaFile> files, bool leaveOpen, out MediaFile equalFile);

        MediaFileInfo SaveFile(string path, Stream stream, bool isTransient = true, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        Task<MediaFileInfo> SaveFileAsync(string path, Stream stream, bool isTransient = true, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        void DeleteFile(MediaFile file, bool permanent, bool force = false);
        FileOperationResult CopyFile(MediaFileInfo mediaFile, string destinationFileName, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        MediaFileInfo MoveFile(MediaFile file, string destinationFileName, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError);
        MediaFileInfo ReplaceFile(MediaFile file, Stream inStream, string newFileName);
        Task<MediaFileInfo> ReplaceFileAsync(MediaFile file, Stream inStream, string newFileName);

        bool FolderExists(string path);
        MediaFolderInfo CreateFolder(string path);
        MediaFolderInfo MoveFolder(string path, string destinationPath);
        FolderOperationResult CopyFolder(string path, string destinationPath, DuplicateEntryHandling dupeEntryHandling = DuplicateEntryHandling.Skip);
        FolderDeleteResult DeleteFolder(string path, FileHandling fileHandling = FileHandling.SoftDelete);

        MediaFileInfo ConvertMediaFile(MediaFile file);
        string GetUrl(MediaFileInfo file, ProcessImageQuery query, string host = null, bool doFallback = true);
    }
}
