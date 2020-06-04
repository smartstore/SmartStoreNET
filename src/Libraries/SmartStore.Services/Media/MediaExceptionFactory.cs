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
            return new MediaFileNotFoundException(T("Admin.Media.Exception.FileNotFound").Text.FormatInvariant(path));
        }

        public MediaFolderNotFoundException FolderNotFound(string path)
        {
            return new MediaFolderNotFoundException(T("Admin.Media.Exception.FolderNotFound").Text.FormatInvariant(path)); 
        }

        public DuplicateMediaFileException DuplicateFile(string fullPath, MediaFileInfo dupeFile)
        {
            return new DuplicateMediaFileException(T("Admin.Media.Exception.DuplicateFile").Text.FormatInvariant(fullPath), dupeFile);
        }

        public DuplicateMediaFolderException DuplicateFolder(string fullPath, MediaFolderNode dupeFolder)
        {
            return new DuplicateMediaFolderException(T("Admin.Media.Exception.DuplicateFolder").Text.FormatInvariant(fullPath), dupeFolder);
        }

        public NotSameAlbumException NotSameAlbum(string path1, string path2)
        {
            return new NotSameAlbumException(T("Admin.Media.Exception.NotSameAlbum").Text.FormatInvariant(path1, path2));
        }

        public DeniedMediaTypeException DeniedMediaType(string fileName, string currentType, string[] acceptedTypes = null)
        {
            var msg = T("Admin.Media.Exception.DeniedMediaType").Text.FormatInvariant(fileName); 
            if (acceptedTypes != null && acceptedTypes.Length > 0)
            {
                var types = string.Join(", ", acceptedTypes);
                msg += T("Admin.Media.Exception.DeniedMediaType.Hint").Text.FormatInvariant(types, currentType);
            }

            return new DeniedMediaTypeException(msg);
        }

        public ExtractThumbnailException ExtractThumbnail(string path, string reason = null)
        {
            return new ExtractThumbnailException(T("Admin.Media.Exception.ExtractThumbnail").Text.FormatInvariant(path, reason.NaIfEmpty()));
        }

        public ExtractThumbnailException ExtractThumbnail(string path, Exception innerException)
        {
            Guard.NotNull(innerException, nameof(innerException));
            return new ExtractThumbnailException(T("Admin.Media.Exception.ExtractThumbnail").Text.FormatInvariant(path, innerException.Message), innerException);
        }

        public MaxMediaFileSizeExceededException MaxFileSizeExceeded(string fileName, long fileSize, long maxSize)
        {
            return new MaxMediaFileSizeExceededException(T("Admin.Media.Exception.MaxFileSizeExceeded").Text.FormatInvariant(fileSize, fileName.NaIfEmpty(), maxSize));
        }
    }
}
