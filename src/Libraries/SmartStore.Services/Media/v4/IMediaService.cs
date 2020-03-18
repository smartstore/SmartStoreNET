using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

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

    public partial interface IMediaService
    {
        int CountFiles(MediaSearchQuery query);
        Task<int> CountFilesAsync(MediaSearchQuery query);
        MediaSearchResult SearchFiles(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        Task<MediaSearchResult> SearchFilesAsync(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);

        bool FileExists(string path);
        MediaFileInfo GetFileByPath(string path);
        MediaFileInfo GetFileById(int id, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);
        IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking);

        MediaFileInfo CreateFile(string path);
        MediaFileInfo CreateFile(int folderId, string fileName);
        MediaFileInfo InsertFile(string album, MediaFile file, Stream stream, bool validate = true);
        void DeleteFile(MediaFile file, bool permanent);

        MediaFileInfo CopyFile(MediaFile file, string newPath, bool overwrite = false);
        MediaFileInfo MoveFile(MediaFile file, int destinationFolderId);
        MediaFileInfo ReplaceFile(MediaFile file, string fileName, string mimeType, Stream stream);
        MediaFileInfo RenameFile(MediaFile file, string newFileName);

        string GetUrl(MediaFileInfo file, ProcessImageQuery query, string host = null);
    }
}
