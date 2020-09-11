using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Web;

namespace SmartStore.Web.Framework.Modelling
{
    internal abstract class FileResponder
    {
        protected FileResponder(IFileResponse fileResponse)
        {
            Guard.NotNull(fileResponse, nameof(fileResponse));
            FileResponse = fileResponse;
        }

        protected IFileResponse FileResponse { get; private set; }

        public virtual bool TrySendHeaders(HttpContextBase context)
        {
            var response = context.Response;

            var utcNow = DateTime.UtcNow;

            // Specify content type
            response.ContentType = FileResponse.ContentType;
            // We support byte ranges
            response.AppendHeader("Accept-Ranges", "bytes");
            //// Set the expires header for HTTP 1.0 cliets
            //response.Cache.SetExpires(utcNow.Add(FileResponse.MaxAge));
            // How often the browser should check that it has the latest version
            response.Cache.SetMaxAge(FileResponse.MaxAge);
            // The unique identifier for the entity
            response.Cache.SetETag(FileResponse.ETag);
            // Proxy and browser can cache response
            response.Cache.SetCacheability(HttpCacheability.Public);
            // Proxy cache should check with orginal server once cache has expired
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            //// The date the file was last modified
            //context.Response.Cache.SetLastModified(FileResponse.LastModifiedUtc);

            return true;
        }

        public abstract void SendFile(HttpContextBase context);
    }


    internal sealed class HeadFileResponder : FileResponder
    {
        public HeadFileResponder(IFileResponse fileResponse)
            : base(fileResponse)
        {
        }

        public override bool TrySendHeaders(HttpContextBase context)
        {
            var response = context.Response;

            base.TrySendHeaders(context);

            response.StatusCode = (int)HttpStatusCode.OK;
            response.AddHeader("Content-Length", (FileResponse.FileLength ?? 0).ToString(CultureInfo.InvariantCulture));

            //if (FileResponse.Dimensions != null)
            //{
            //	response.AddHeader("X-Width", FileResponse.Dimensions.Value.Width.ToString(CultureInfo.InvariantCulture));
            //	response.AddHeader("X-Height", FileResponse.Dimensions.Value.Height.ToString(CultureInfo.InvariantCulture));
            //}

            response.End();

            return true;
        }

        public override void SendFile(HttpContextBase context)
        {
            // Don't send any file.
        }
    }


    internal sealed class UnmodifiedFileResponder : FileResponder
    {
        public UnmodifiedFileResponder(IFileResponse fileResponse)
            : base(fileResponse)
        {
        }

        public override bool TrySendHeaders(HttpContextBase context)
        {
            var response = context.Response;

            base.TrySendHeaders(context);

            response.StatusCode = (int)HttpStatusCode.NotModified;
            response.StatusDescription = "Not Modified";

            // Explicitly set the Content-Length header so the client doesn't wait for
            // content but keeps the connection open for other requests
            response.AddHeader("Content-Length", "0");

            return true;
        }

        public override void SendFile(HttpContextBase context)
        {
            // Don't send file, it is unmodified. Let browser fetch from its cache.
        }
    }



    internal sealed class FullFileResponder : FileResponder
    {
        public FullFileResponder(IFileResponse fileResponse)
            : base(fileResponse)
        {
        }

        public override bool TrySendHeaders(HttpContextBase context)
        {
            base.TrySendHeaders(context);

            // The date the file was last modified
            context.Response.Cache.SetLastModified(FileResponse.LastModifiedUtc);

            return true;
        }

        public override void SendFile(HttpContextBase context)
        {
            var fileLength = FileResponse.FileLength ?? FileResponse.Transmitter.GetFileLength();
            FileResponse.Transmitter.TransmitFile(0, fileLength, fileLength, context);
        }
    }



    internal sealed class RangeFileResponder : FileResponder
    {
        internal struct ByteRange
        {
            internal long Offset;
            internal long Length;
        }

        private const string RANGE_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";
        private const string MULTIPART_CONTENT_TYPE = "multipart/byteranges; boundary=<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";
        private const string MULTIPART_RANGE_DELIMITER = "--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>\r\n";
        private const string MULTIPART_RANGE_END = "--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>--\r\n\r\n";
        private const string CONTENT_RANGE_FORMAT = "bytes {0}-{1}/{2}";
        private const int MAX_RANGE_ALLOWED = 5;

        private const int ERROR_ACCESS_DENIED = 5;

        private static readonly string[] _httpDateFormats = new string[] { "r", "dddd, dd-MMM-yy HH':'mm':'ss 'GMT'", "ddd MMM d HH':'mm':'ss yyyy" };

        private readonly FileResponder _defaultResponder;
        private readonly string _rangeHeader;

        public RangeFileResponder(IFileResponse fileResponse, string rangeHeader)
            : base(fileResponse)
        {
            _defaultResponder = new FullFileResponder(fileResponse);
            _rangeHeader = rangeHeader;
        }

        public override bool TrySendHeaders(HttpContextBase context)
        {
            var fileLength = FileResponse.FileLength ?? FileResponse.Transmitter.GetFileLength();
            var etag = FileResponse.ETag;
            var lastModified = FileResponse.LastModifiedUtc;

            var handled = ExecuteRangeRequest(context, fileLength, _rangeHeader, etag, lastModified);
            if (!handled && _defaultResponder.TrySendHeaders(context))
            {
                _defaultResponder.SendFile(context);
                handled = true;
            }

            return handled;
        }

        public override void SendFile(HttpContextBase context)
        {
            // Do nothing here, we have handled everything in 'TrySendHeaders()' already
        }

        #region Range Utils

        // Most of the helpers here were copied over from the internal StaticFileHandler.cs

        private bool ExecuteRangeRequest(
            HttpContextBase context,
            long fileLength,
            string rangeHeader,
            string etag,
            DateTime lastModified)
        {
            var handled = false;
            var request = context.Request;
            var response = context.Response;

            // return "416 Requested range not satisfiable" if the file length is zero.
            if (fileLength <= 0)
            {
                SendRangeNotSatisfiable(response, fileLength);
                handled = true;
                return handled;
            }

            var ifRangeHeader = request.Headers["If-Range"];
            if (ifRangeHeader != null && ifRangeHeader.Length > 1)
            {
                // Is this an ETag or a Date? We only need to check two 
                // characters; an ETag either begins with W/ or it is quoted.
                if (ifRangeHeader[0] == '"')
                {
                    // it's a strong ETag
                    if (ifRangeHeader != etag)
                    {
                        // the etags do not match, and we will therefore return the entire response
                        return handled;
                    }
                }
                else if (ifRangeHeader[0] == 'W' && ifRangeHeader[1] == '/')
                {
                    // it's a weak ETag, and is therefore not usable for sub-range retrieval and
                    // we will return the entire response
                    return handled;
                }
                else
                {
                    // It's a date. If it is greater than or equal to the last-write time of the file, we can send the range.
                    if (IsOutDated(ifRangeHeader, lastModified))
                    {
                        return handled;
                    }
                }
            }

            // the expected format is "bytes = <range1>[, <range2>, ...]"
            // where <range> is "<first_byte_pos>-[<last_byte_pos>]" or "-<last_n_bytes>".
            int indexOfEquals = rangeHeader.IndexOf('=');
            if (indexOfEquals == -1 || indexOfEquals == rangeHeader.Length - 1)
            {
                // invalid syntax
                return handled;
            }

            int startIndex = indexOfEquals + 1;
            bool isRangeHeaderSyntacticallyValid = true;
            long offset;
            long length;
            bool isSatisfiable;
            bool exceededMax = false;
            ByteRange[] byteRanges = null;
            int byteRangesCount = 0;
            long totalBytes = 0;
            while (startIndex < rangeHeader.Length && isRangeHeaderSyntacticallyValid)
            {
                isRangeHeaderSyntacticallyValid = GetNextRange(rangeHeader, ref startIndex, fileLength, out offset, out length, out isSatisfiable);

                if (!isRangeHeaderSyntacticallyValid)
                {
                    break;
                }

                if (!isSatisfiable)
                {
                    continue;
                }

                if (byteRanges == null)
                {
                    byteRanges = new ByteRange[16];
                }

                if (byteRangesCount >= byteRanges.Length)
                {
                    // grow byteRanges array
                    ByteRange[] buffer = new ByteRange[byteRanges.Length * 2];
                    Array.Copy(byteRanges, buffer, byteRanges.Length);
                    byteRanges = buffer;
                }

                byteRanges[byteRangesCount].Offset = offset;
                byteRanges[byteRangesCount].Length = length;
                byteRangesCount++;

                // IIS imposes this limitation too, and sends "400 Bad Request" if exceeded
                totalBytes += length;
                if (totalBytes > fileLength * MAX_RANGE_ALLOWED)
                {
                    exceededMax = true;
                    break;
                }
            }

            if (!isRangeHeaderSyntacticallyValid)
            {
                return handled;
            }

            if (exceededMax)
            {
                SendBadRequest(response);
                handled = true;
                return handled;
            }

            if (byteRangesCount == 0)
            {
                // we parsed the Range header and found no satisfiable byte ranges, so return "416 Requested Range Not Satisfiable"
                SendRangeNotSatisfiable(response, fileLength);
                handled = true;
                return handled;
            }

            if (byteRangesCount == 1)
            {
                offset = byteRanges[0].Offset;
                length = byteRanges[0].Length;
                response.ContentType = FileResponse.ContentType;
                string contentRange = String.Format(CultureInfo.InvariantCulture, CONTENT_RANGE_FORMAT, offset, offset + length - 1, fileLength);
                response.AppendHeader("Content-Range", contentRange);

                SendResponseHeaders();
                FileResponse.Transmitter.TransmitFile(offset, length, fileLength, context);
            }
            else
            {
                response.ContentType = MULTIPART_CONTENT_TYPE;
                string contentRange;
                string partialContentType = "Content-Type: " + FileResponse.ContentType + "\r\n";

                SendResponseHeaders();

                for (int i = 0; i < byteRangesCount; i++)
                {
                    offset = byteRanges[i].Offset;
                    length = byteRanges[i].Length;
                    response.Write(MULTIPART_RANGE_DELIMITER);
                    response.Write(partialContentType);
                    response.Write("Content-Range: ");
                    contentRange = String.Format(CultureInfo.InvariantCulture, CONTENT_RANGE_FORMAT, offset, offset + length - 1, fileLength);
                    response.Write(contentRange);
                    response.Write("\r\n\r\n");
                    // Transmit
                    FileResponse.Transmitter.TransmitFile(offset, length, fileLength, context);
                    response.Write("\r\n");
                }
                response.Write(MULTIPART_RANGE_END);
            }

            handled = true;
            return handled;

            void SendResponseHeaders()
            {
                // Send a "206 Partial Content" status
                response.StatusCode = (int)HttpStatusCode.PartialContent;
                response.AppendHeader("Accept-Ranges", "bytes");
                response.Cache.SetLastModified(lastModified);
                response.Cache.SetETag(etag);
                response.Cache.SetCacheability(HttpCacheability.Public);
            }
        }

        private void SendBadRequest(HttpResponseBase response)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Write("<html><body>Bad Request</body></html>");
        }

        private void SendRangeNotSatisfiable(HttpResponseBase response, long fileLength)
        {
            response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
            response.ContentType = null;
            response.AppendHeader("Content-Range", "bytes */" + fileLength.ToString(NumberFormatInfo.InvariantInfo));
        }

        private static bool IsOutDated(string ifRangeHeader, DateTime lastModified)
        {
            try
            {
                var utcLastModified = lastModified.ToUniversalTime();
                var utc = UtcParse(ifRangeHeader);
                return (utc < utcLastModified);
            }
            catch
            {
                return true;
            }
        }

        private static DateTime UtcParse(string time)
        {
            Guard.NotNull(time, nameof(time));

            var dtStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
            if (DateTime.TryParseExact(time, _httpDateFormats, null, dtStyles, out var utcDate))
            {
                return utcDate;
            }

            throw new FormatException($"{time} is an invalid date expression.");
        }

        // initial space characters are skipped, and the string of digits up until the first non-digit
        // are converted to a long.  If digits are found the method returns true; otherwise false.
        private static bool GetLongFromSubstring(string s, ref int startIndex, out long result)
        {
            result = 0;

            // get index of first digit
            MovePastSpaceCharacters(s, ref startIndex);
            int beginIndex = startIndex;

            // get index of last digit
            MovePastDigits(s, ref startIndex);
            int endIndex = startIndex - 1;

            // are there any digits?
            if (endIndex < beginIndex)
            {
                return false;
            }

            long multipleOfTen = 1;
            for (int i = endIndex; i >= beginIndex; i--)
            {
                int digit = s[i] - '0';
                result += digit * multipleOfTen;
                multipleOfTen *= 10;
                // check for overflow
                if (result < 0)
                {
                    return false;
                }
            }

            return true;
        }

        // The Range header consists of one or more byte range specifiers.  E.g, "Range: bytes=0-1024,-1024" is a request
        // for the first and last 1024 bytes of a file. Before this method is called, startIndex points to the beginning
        // of a byte range specifier; and afterwards it points to the beginning of the next byte range specifier.  
        // If the current byte range specifier is syntactially inavlid, this function will return false indicating that the 
        // Range header must be ignored.  If the function returns true, then the byte range specifier will be converted to 
        // an offset and length, and the startIndex will be incremented to the next byte range specifier.  The byte range 
        // specifier (offset and length) returned by this function is satisfiable if and only if isSatisfiable is true.
        private static bool GetNextRange(string rangeHeader, ref int startIndex, long fileLength, out long offset, out long length, out bool isSatisfiable)
        {
            // startIndex is first char after '=', or first char after ','
            Debug.Assert(startIndex < rangeHeader.Length, "startIndex < rangeHeader.Length");

            offset = 0;
            length = 0;
            isSatisfiable = false;

            // A Range request to an empty file is never satisfiable, and will always receive a 416 status.
            if (fileLength <= 0)
            {
                // put startIndex at end of string so we don't try to call GetNextRange again
                startIndex = rangeHeader.Length;
                return true;
            }

            MovePastSpaceCharacters(rangeHeader, ref startIndex);

            if (startIndex < rangeHeader.Length && rangeHeader[startIndex] == '-')
            {
                // this range is of the form "-mmm"
                startIndex++;
                if (!GetLongFromSubstring(rangeHeader, ref startIndex, out length))
                {
                    return false;
                }

                if (length > fileLength)
                {
                    // send entire file
                    offset = 0;
                    length = fileLength;
                }
                else
                {
                    // send last N bytes
                    offset = fileLength - length;
                }

                isSatisfiable = IsRangeSatisfiable(offset, length, fileLength);
                // we parsed the current range, but need to successfully move the startIndex to the next range
                return IncrementToNextRange(rangeHeader, ref startIndex);
            }
            else
            {
                // this range is of the form "nnn-[mmm]"
                if (!GetLongFromSubstring(rangeHeader, ref startIndex, out offset))
                {
                    return false;
                }

                // increment startIndex past '-'
                if (startIndex < rangeHeader.Length && rangeHeader[startIndex] == '-')
                {
                    startIndex++;
                }
                else
                {
                    return false;
                }

                if (!GetLongFromSubstring(rangeHeader, ref startIndex, out long endPos))
                {
                    // assume range is of form "nnn-".  If it isn't,
                    // the call to IncrementToNextRange will return false
                    length = fileLength - offset;
                }
                else
                {
                    // if...greater than or equal to the current length of the entity-body, last-byte-pos 
                    // is taken to be equal to one less than the current length of the entity- body in bytes.
                    if (endPos > fileLength - 1)
                    {
                        endPos = fileLength - 1;
                    }

                    length = endPos - offset + 1;

                    if (length < 1)
                    {
                        // the byte range specifier is syntactially invalid 
                        // because the last-byte-pos < first-byte-pos
                        return false;
                    }
                }

                isSatisfiable = IsRangeSatisfiable(offset, length, fileLength);

                // we parsed the current range, but need to successfully move the startIndex to the next range      
                return IncrementToNextRange(rangeHeader, ref startIndex);
            }
        }

        private static bool IncrementToNextRange(string s, ref int startIndex)
        {
            // increment startIndex until next token and return true, unless the syntax is invalid
            MovePastSpaceCharacters(s, ref startIndex);

            if (startIndex < s.Length)
            {
                if (s[startIndex] != ',')
                {
                    return false;
                }
                // move to first char after ','
                startIndex++;
            }

            return true;
        }

        private static bool IsRangeSatisfiable(long offset, long length, long fileLength)
        {
            return (offset < fileLength && length > 0);
        }

        private static bool IsSecurityError(int ErrorCode)
        {
            return (ErrorCode == ERROR_ACCESS_DENIED);
        }

        private static void MovePastSpaceCharacters(string s, ref int startIndex)
        {
            while (startIndex < s.Length && s[startIndex] == ' ')
            {
                startIndex++;
            }
        }

        private static void MovePastDigits(string s, ref int startIndex)
        {
            while (startIndex < s.Length && s[startIndex] <= '9' && s[startIndex] >= '0')
            {
                startIndex++;
            }
        }

        #endregion
    }
}
