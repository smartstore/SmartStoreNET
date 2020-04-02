using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

    public enum FileDeleteStrategy
    {
        SoftDelete,
        MoveToRoot,
        Delete
    }

    public partial interface IMediaService
    {
        /// <summary>
        /// Gets the underlying storage provider
        /// </summary>
        IMediaStorageProvider StorageProvider { get; }

        int CountFiles(MediaSearchQuery query);
        Task<int> CountFilesAsync(MediaSearchQuery query);
        MediaSearchResult SearchFiles(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        Task<MediaSearchResult> SearchFilesAsync(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);

        bool FileExists(string path);
        MediaFileInfo GetFileByPath(string path, MediaLoadFlags flags = MediaLoadFlags.None);
        MediaFileInfo GetFileById(int id, MediaLoadFlags flags = MediaLoadFlags.None);
        IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        bool CheckUniqueFileName(string path, out string newPath);

        /// <summary>
        /// Finds an equal file by comparing the binary buffer
        /// </summary>
        /// <param name="fileBuffer">Binary source file data to find a match for.</param>
        /// <param name="files">The sequence of files to seek within for duplicates.</param>
        /// <param name="equalFileId">Id of equal file if any</param>
        /// <returns>The passed file binary when no file equals in the sequence, <c>null</c> otherwise.</returns>
        byte[] FindEqualFile(byte[] fileBuffer, IEnumerable<MediaFile> files, out int equalFileId);

        MediaFileInfo SaveFile(string path, Stream stream, bool isTransient = true, bool overwrite = false);
        Task<MediaFileInfo> SaveFileAsync(string path, Stream stream, bool isTransient = true, bool overwrite = false);
        void DeleteFile(MediaFile file, bool permanent);
        MediaFileInfo CopyFile(MediaFile file, string destinationFileName, bool overwrite = false);
        MediaFileInfo MoveFile(MediaFile file, string destinationFileName);

        bool FolderExists(string path);
        MediaFolderInfo CreateFolder(string path);
        MediaFolderInfo MoveFolder(string path, string destinationPath);
        MediaFolderInfo CopyFolder(string path, string destinationPath, bool overwrite = false);
        void DeleteFolder(string path, FileDeleteStrategy strategy = FileDeleteStrategy.SoftDelete);

        string GetUrl(MediaFileInfo file, ProcessImageQuery query, string host = null);
    }
}
