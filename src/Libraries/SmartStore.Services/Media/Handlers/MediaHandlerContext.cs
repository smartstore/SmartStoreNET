using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Security;
using SmartStore.Services.Media.Imaging;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
    public class MediaHandlerContext
    {
        private Lazy<IFile> _lazySourceFile;
        private IFile _sourceFile;

        public MediaHandlerContext()
        {
            _lazySourceFile = new Lazy<IFile>(() => GetSourceFile(), false);
        }

        public IMediaService MediaService { get; set; }
        public IPermissionService PermissionService { get; set; }
        public Customer CurrentCustomer { get; set; }
        public HttpContextBase HttpContext { get; set; }

        public int MediaFileId { get; set; }
        public string RawPath { get; set; }

        public MediaPathData PathData { get; set; }
        public ProcessImageQuery ImageQuery { get; set; }

        public IFile SourceFile
        {
            get => _sourceFile ?? _lazySourceFile.Value;
            set => _sourceFile = value;
        }

        public bool Executed { get; set; }
        public Exception Exception { get; set; }
        public IFile ResultFile { get; set; }
        public IImage ResultImage { get; set; }

        private IFile GetSourceFile()
        {
            if (MediaFileId == 0)
            {
                // This is most likely a request for a default placeholder image
                var fallbackImagePath = Path.Combine(MediaUrlGenerator.FallbackImagesRootPath, RawPath).Replace('\\', '/');
                var fi = new FileInfo(CommonHelper.MapPath("~/" + fallbackImagePath, false));
                if (!fi.Exists)
                    return null;

                return new LocalFileSystem.LocalFile(fallbackImagePath, fi);
            }
            else
            {
                // Get file from DB
                var mediaFile = MediaService.GetFileById(MediaFileId, MediaLoadFlags.AsNoTracking);

                // File must exist
                if (mediaFile == null)
                    return null;

                // Serve deleted or hidden files only with sufficient permission
                if ((mediaFile.Deleted || mediaFile.Hidden) && !PermissionService.Authorize(Permissions.Media.Update, CurrentCustomer))
                    return null;

                //// File's mime must match requested mime
                //if (!mediaFile.MimeType.IsCaseInsensitiveEqual(prevMime ?? pathData.MimeType))
                //	return null;

                // File path must match requested path and file title
                // TODO: (mm) (mc) what about MIME and EXT?
                if (mediaFile.FolderId != PathData.Folder?.Id || !mediaFile.Title.IsCaseInsensitiveEqual(PathData.FileTitle))
                    return null;

                return mediaFile;
            }
        }
    }
}
