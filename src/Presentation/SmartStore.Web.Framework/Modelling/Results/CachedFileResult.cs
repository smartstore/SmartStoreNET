using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Modelling
{
    public class CachedFileResult : ActionResult, IFileResponse
    {
        #region Ctor

        public CachedFileResult(string path, string contentType = null)
            : this(GetFileInfo(path), contentType)
        {
        }

        public CachedFileResult(FileInfo file, string contentType = null)
        {
            Guard.NotNull(file, nameof(file));

            if (!file.Exists)
            {
                throw new FileNotFoundException(file.FullName);
            }

            Transmitter = new FileStreamTransmitter(file.OpenRead);
            ContentType = contentType.NullEmpty() ?? MimeTypes.MapNameToMimeType(file.Name);
            FileLength = file.Length;
            LastModifiedUtc = file.LastWriteTimeUtc;
        }

        public CachedFileResult(IFile file, string contentType = null)
        {
            Guard.NotNull(file, nameof(file));

            if (!file.Exists)
            {
                throw new FileNotFoundException(file.Path);
            }

            Transmitter = new FileStreamTransmitter(file.OpenRead);
            ContentType = contentType.NullEmpty() ?? MimeTypes.MapNameToMimeType(file.Name);
            FileLength = file.Size;
            LastModifiedUtc = file.LastUpdated;
        }

        public CachedFileResult(VirtualFile file, DateTime? lastModifiedUtc = null, string contentType = null)
        {
            Guard.NotNull(file, nameof(file));

            try
            {
                var fi = GetFileInfo(file.VirtualPath);
                if (fi.Exists)
                {
                    ContentType = contentType.NullEmpty() ?? MimeTypes.MapNameToMimeType(fi.Name);
                    FileLength = fi.Length;
                    LastModifiedUtc = fi.LastWriteTimeUtc;
                }
            }
            finally
            {
                if (lastModifiedUtc == null)
                {
                    throw new ArgumentNullException(nameof(lastModifiedUtc), "A modification date must be provided if the VirtualFile cannot be mapped to a physical path.");
                }

                if (FileLength == null)
                {
                    ContentType = contentType.NullEmpty() ?? MimeTypes.MapNameToMimeType(file.Name);
                    using (var stream = file.Open())
                    {
                        FileLength = stream.Length;
                    }
                }
            }

            Transmitter = new FileStreamTransmitter(file.Open);
            LastModifiedUtc = lastModifiedUtc.Value;
        }

        public CachedFileResult(string contentType, DateTime lastModifiedUtc, Func<Stream> factory, long? fileLength = null)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotEmpty(contentType, nameof(contentType));

            Transmitter = new FileStreamTransmitter(factory);
            ContentType = contentType;
            FileLength = fileLength;
            LastModifiedUtc = lastModifiedUtc;
        }

        public CachedFileResult(string contentType, DateTime lastModifiedUtc, Func<byte[]> factory, long? fileLength = null)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotEmpty(contentType, nameof(contentType));

            Transmitter = new FileBufferTransmitter(factory);
            ContentType = contentType;
            FileLength = fileLength;
            LastModifiedUtc = lastModifiedUtc;
        }

        private static FileInfo GetFileInfo(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!PathHelper.IsAbsolutePhysicalPath(path))
            {
                path = CommonHelper.MapPath(path);
            }

            return new FileInfo(path);
        }

        #endregion

        #region Properties

        public string ContentType { get; private set; }

        public long? FileLength { get; set; }

        public DateTime LastModifiedUtc { get; set; }

        public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// If not set, will be auto-generated based on <see cref="LastModifiedUtc"/> property.
        /// </summary>
        public string ETag { get; set; }

        public FileTransmitter Transmitter { get; private set; }

        /// <summary>
        /// A callback that will be invoked on successful result execution
        /// </summary>
        public Action OnExecuted { get; set; }

        #endregion

        public override void ExecuteResult(ControllerContext context)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Fix Last Modified Time.  We might need it soon 
            // if we encounter a Range: and If-Range header
            // Using UTC time to avoid daylight savings time bug 83230
            var lastModified = new DateTime(LastModifiedUtc.Year,
                LastModifiedUtc.Month,
                LastModifiedUtc.Day,
                LastModifiedUtc.Hour,
                LastModifiedUtc.Minute,
                LastModifiedUtc.Second,
                0,
                DateTimeKind.Utc);

            // Because we can't set a "Last-Modified" header to any time
            // in the future, check the last modified time and set it to
            // DateTime.Now if it's in the future. 
            // This is to fix VSWhidbey #402323
            var utcNow = DateTime.UtcNow;
            if (lastModified > utcNow)
            {
                // use 1 second resolution
                lastModified = new DateTime(utcNow.Ticks - (utcNow.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            }

            LastModifiedUtc = lastModified;

            // Generate ETag if empty
            if (ETag.IsEmpty())
            {
                ETag = GenerateETag(httpContext, lastModified, utcNow);
            }

            // Determine applicable file responder
            var responder = ResolveResponder(request);

            // Execute response (send file)
            if (responder.TrySendHeaders(httpContext))
            {
                responder.SendFile(httpContext);
            }

            // Finish: invoke the optional callback
            OnExecuted?.Invoke();
        }

        private FileResponder ResolveResponder(HttpRequestBase request)
        {
            // Is this a HEAD request
            if (request.HttpMethod == "HEAD")
            {
                return new HeadFileResponder(this);
            }

            // Is this a request for an unmodified file?
            var ifNoneMatch = request.Headers["If-None-Match"];
            if (ifNoneMatch.HasValue() && ETag == ifNoneMatch)
            {
                return new UnmodifiedFileResponder(this);
            }

            // Is this a Range request?
            var rangeHeader = request.Headers["Range"];
            if (rangeHeader.HasValue() && rangeHeader.StartsWith("bytes", StringComparison.OrdinalIgnoreCase))
            {
                return new RangeFileResponder(this, rangeHeader);
            }

            // Responder for sending the whole file
            return new FullFileResponder(this);
        }

        private static string GenerateETag(HttpContextBase context, DateTime lastModified, DateTime now)
        {
            // Get 64-bit FILETIME stamp
            var lastModFileTime = lastModified.ToFileTime();
            var nowFileTime = now.ToFileTime();
            var hexFileTime = lastModFileTime.ToString("X8", CultureInfo.InvariantCulture);

            //// Do what IIS does to determine if this is a weak ETag.
            //// Compare the last modified time to now and if the difference is
            //// less than or equal to 3 seconds, then it is weak
            //if ((nowFileTime - lastModFileTime) <= 30000000)
            //{
            //	return "W/\"" + hexFileTime + "\"";
            //}

            return "\"" + hexFileTime + "\"";
        }
    }
}
