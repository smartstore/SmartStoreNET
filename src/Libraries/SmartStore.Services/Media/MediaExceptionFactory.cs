using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Media
{
    #region Exception classes

    public sealed class MediaFileNotFoundException : SmartException
    {
        public MediaFileNotFoundException(string message) : base(message) { }
    }

    public sealed class MediaFolderNotFoundException : SmartException
    {
        public MediaFolderNotFoundException(string message) : base(message) { }
    }

    public sealed class DuplicateMediaFileException : SmartException
    {
        public DuplicateMediaFileException(string message, MediaFileInfo dupeFile) : base(message)
        {
            File = dupeFile;
        }

        public MediaFileInfo File { get; }
    }

    public sealed class DuplicateMediaFolderException : SmartException
    {
        public DuplicateMediaFolderException(string message, MediaFolderNode dupeFolder) : base(message)
        {
            Folder = dupeFolder;
        }

        public MediaFolderNode Folder { get; }
    }

    public sealed class NotSameAlbumException : SmartException
    {
        public NotSameAlbumException(string message) : base(message) { }
    }

    public sealed class DeniedMediaTypeException : SmartException
    {
        public DeniedMediaTypeException(string message) : base(message) { }
    }

    public sealed class ExtractThumbnailException : SmartException
    {
        public ExtractThumbnailException(string message) : base(message) { }
        public ExtractThumbnailException(string message, Exception innerException) : base(message, innerException) { }
    }

    public sealed class MaxMediaFileSizeExceededException : SmartException
    {
        public MaxMediaFileSizeExceededException(string message) : base(message) { }
    }

    #endregion

    public class MediaExceptionFactory
    {
        public Localizer T { get; set; } = NullLocalizer.Instance;

        public MediaFileNotFoundException FileNotFound(string path)
        {
            return new MediaFileNotFoundException($"Media file '{path}' does not exist."); // TODO: (mm) Loc
        }

        public MediaFolderNotFoundException FolderNotFound(string path)
        {
            return new MediaFolderNotFoundException($"Media folder '{path}' does not exist."); // TODO: (mm) Loc
        }

        public DuplicateMediaFileException DuplicateFile(string fullPath, MediaFileInfo dupeFile)
        {
            return new DuplicateMediaFileException($"File {fullPath} already exists.", dupeFile); // TODO: (mm) Loc
        }

        public DuplicateMediaFolderException DuplicateFolder(string fullPath, MediaFolderNode dupeFolder)
        {
            return new DuplicateMediaFolderException($"Folder {fullPath} already exists.", dupeFolder); // TODO: (mm) Loc
        }

        public NotSameAlbumException NotSameAlbum(string path1, string path2)
        {
            return new NotSameAlbumException($"The file operation requires that the destination path belongs to the source album. Source: {path1}, Destination: {path2}."); // TODO: (mm) Loc
        }

        public DeniedMediaTypeException DeniedMediaType(string fileName, string currentType, string[] acceptedTypes = null)
        {
            var msg = $"The media type of '{fileName}' does not match the list of accepted media types"; // TODO: (mm) Loc
            if (acceptedTypes != null && acceptedTypes.Length > 0)
            {
                msg += $" Accepted: {string.Join(", ", acceptedTypes)}, current: {currentType}."; // TODO: (mm) Loc
            }
            
            return new DeniedMediaTypeException(msg);
        }

        public ExtractThumbnailException ExtractThumbnail(string path, string reason = null)
        {
            return new ExtractThumbnailException($"Thumbnail extraction for file '{path}' failed. Reason: {reason.NaIfEmpty()}."); // TODO: (mm) Loc
        }

        public ExtractThumbnailException ExtractThumbnail(string path, Exception innerException)
        {
            Guard.NotNull(innerException, nameof(innerException));
            return new ExtractThumbnailException($"Thumbnail extraction for file '{path}' failed. Reason: {innerException.Message}.", innerException); // TODO: (mm) Loc
        }

        public MaxMediaFileSizeExceededException MaxFileSizeExceeded(string fileName, long fileSize, long maxSize)
        {
            return new MaxMediaFileSizeExceededException($"The size {fileSize:N0)} of file '{fileName.NaIfEmpty()}' exceeds the maximum allowed file size {maxSize:N0)}."); // TODO: (mm) Loc
        }
    }
}
