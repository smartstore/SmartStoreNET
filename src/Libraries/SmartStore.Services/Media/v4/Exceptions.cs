using SmartStore.Core.Domain.Media;
using System;

namespace SmartStore.Services.Media
{
    public sealed class MediaFileNotFoundException : SmartException
    {
        public MediaFileNotFoundException(string path)
            : base($"Media file {path} does not exist.")
        {
        }
    }

    public sealed class MediaFolderNotFoundException : SmartException
    {
        public MediaFolderNotFoundException(string path)
            : base($"Media folder {path} does not exist.")
        {
        }
    }

    public sealed class DuplicateMediaFileException : SmartException
    {
        public DuplicateMediaFileException(string fullPath, MediaFileInfo dupeFile)
            : base($"File {fullPath} already exists.")
        {
            File = dupeFile;
        }

        public MediaFileInfo File { get; }
    }

    public sealed class DuplicateMediaFolderException : SmartException
    {
        public DuplicateMediaFolderException(string fullPath)
            : base($"Folder {fullPath} already exists.")
        {
        }
    }

    public sealed class NotSameAlbumException : SmartException
    {
        public NotSameAlbumException(string path1, string path2)
            : base($"The file operation requires that the destination path belongs to the source album. Source: {path1}, Destination: {path2}.")
        {
        }
    }

    public sealed class DeniedMediaTypeException : SmartException
    {
        public DeniedMediaTypeException(string fileName, string currentType, string[] acceptedTypes)
            : base($"The media type of '{fileName}' does not match the list of accesped media types. Accepted: {string.Join(", ", acceptedTypes)}, current: {currentType}")
        {
        }
    }

    public sealed class ExtractThumbnailException : SmartException
    {
        public ExtractThumbnailException(string path, Exception innerException)
            : base($"Thumbnail extraction for file '{path}' failed. See inner exception for details.", innerException)
        {
        }
    }
}
