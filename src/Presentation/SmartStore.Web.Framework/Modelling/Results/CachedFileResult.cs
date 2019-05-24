using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Modelling
{
	public class CachedFileResult : ActionResult
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

		// Default buffer size as defined in BufferedStream type
		const int DefaultWriteBufferSize = 81920;

		private static readonly string[] _httpDateFormats = new string[] { "r", "dddd, dd-MMM-yy HH':'mm':'ss 'GMT'", "ddd MMM d HH':'mm':'ss yyyy" };

		private readonly Func<Stream> _streamReader;
		private readonly Func<byte[]> _bufferReader;

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

			_streamReader = file.OpenRead;

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

			_streamReader = file.OpenRead;

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

				if (FileLength == 0)
				{
					ContentType = contentType.NullEmpty() ?? MimeTypes.MapNameToMimeType(file.Name);
					using (var stream = file.Open())
					{
						FileLength = stream.Length;
					}
				}
			}

			LastModifiedUtc = lastModifiedUtc.Value;
			_streamReader = file.Open;
		}

		public CachedFileResult(string contentType, DateTime lastModifiedUtc, long fileLength, Func<Stream> reader)
		{
			Guard.NotNull(reader, nameof(reader));
			Guard.NotEmpty(contentType, nameof(contentType));
			Guard.IsPositive(fileLength, nameof(fileLength));

			ContentType = contentType;
			FileLength = fileLength;
			LastModifiedUtc = lastModifiedUtc;
			_streamReader = reader;
		}

		public CachedFileResult(string contentType, DateTime lastModifiedUtc, long fileLength, Func<byte[]> reader)
		{
			Guard.NotNull(reader, nameof(reader));
			Guard.NotEmpty(contentType, nameof(contentType));
			Guard.IsPositive(fileLength, nameof(fileLength));

			ContentType = contentType;
			FileLength = fileLength;
			LastModifiedUtc = lastModifiedUtc;
			_bufferReader = reader;
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

		public long FileLength { get; set; }

		public DateTime LastModifiedUtc { get; set; }

		public DateTime ExpiresOnUtc { get; set; } = DateTime.UtcNow.AddDays(1);

		/// <summary>
		/// If not set, will be auto-generated based on <see cref="LastModifiedUtc"/> property.
		/// </summary>
		public string ETag { get; set; }

		/// <summary>
		/// A callback that will be invoked on successful result execution
		/// </summary>
		public Action OnExecuted { get; set; }

		#endregion

		#region Utils

		// Most of the helpers here were copied over from the internal StaticFileHandler.cs

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

		private static void SendNotModified(HttpResponseBase response)
		{
			response.StatusCode = (int)HttpStatusCode.NotModified;
			response.StatusDescription = "Not Modified";

			// Explicitly set the Content-Length header so the client doesn't wait for
			// content but keeps the connection open for other requests
			response.AddHeader("Content-Length", "0");
		}

		private static void SendBadRequest(HttpResponseBase response)
		{
			response.StatusCode = (int)HttpStatusCode.BadRequest;
			response.Write("<html><body>Bad Request</body></html>");
		}

		private static void SendRangeNotSatisfiable(HttpResponseBase response, long fileLength)
		{
			response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
			response.ContentType = null;
			response.AppendHeader("Content-Range", "bytes */" + fileLength.ToString(NumberFormatInfo.InvariantInfo));
		}

		#endregion

		public override void ExecuteResult(ControllerContext context)
		{
			var httpContext = context.HttpContext;
			var request = httpContext.Request;
			var response = httpContext.Response;

			// Determine Last Modified Time.  We might need it soon 
			// if we encounter a Range: and If-Range header
			// Using UTC time to avoid daylight savings time bug 83230
			var lastModifiedInUtc = new DateTime(LastModifiedUtc.Year,
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
			if (lastModifiedInUtc > utcNow)
			{
				// use 1 second resolution
				lastModifiedInUtc = new DateTime(utcNow.Ticks - (utcNow.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
			}

			string etag = ETag.NullEmpty() ?? GenerateETag(httpContext, lastModifiedInUtc, utcNow);
			var fileLength = FileLength;

			// is this a Range request?
			var rangeHeader = request.Headers["Range"];
			if (rangeHeader.HasValue() && rangeHeader.StartsWith("bytes", StringComparison.OrdinalIgnoreCase))
			{
				if (ExecuteRangeRangeRequest(httpContext, fileLength, rangeHeader, etag, lastModifiedInUtc))
				{
					return;
				}
			}

			bool isNotModified = false;

			var ifNoneMatch = request.Headers["If-None-Match"];
			if (ifNoneMatch.HasValue() && etag == ifNoneMatch)
			{
				// File hasn't changed, so return HTTP 304 without retrieving the data
				SendNotModified(response);
				isNotModified = true;
			}
			else
			{
				// if we get this far, we're sending the entire file
				SendFile(0, fileLength, fileLength, httpContext);
			}

			// Specify content type
			response.ContentType = ContentType;
			// We support byte ranges
			response.AppendHeader("Accept-Ranges", "bytes");
			// Set an expires in the future (INFO: Chrome will not revalidate until expiration, but that's ok)
			response.Cache.SetExpires(ExpiresOnUtc);
			// always set ETag
			response.Cache.SetETag(etag);
			// always set Cache-Control to public
			response.Cache.SetCacheability(HttpCacheability.Public);
			// always set must-revalidate
			response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

			if (!isNotModified)
			{
				// always set Last-Modified
				response.Cache.SetLastModified(lastModifiedInUtc);
			}

			// Finish: invoke the optional callback
			OnExecuted?.Invoke();
		}

		private bool ExecuteRangeRangeRequest(
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
					//int byteCount = byteRanges.Length * Marshal.SizeOf(byteRanges[0]);
					//unsafe
					//{
					//	fixed (ByteRange* src = byteRanges, dst = buffer)
					//	{
					//		StringUtil.memcpyimpl((byte*)src, (byte*)dst, byteCount);
					//	}
					//}

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
				response.ContentType = ContentType;
				string contentRange = String.Format(CultureInfo.InvariantCulture, CONTENT_RANGE_FORMAT, offset, offset + length - 1, fileLength);
				response.AppendHeader("Content-Range", contentRange);

				SendFile(offset, length, fileLength, context);
			}
			else
			{
				response.ContentType = MULTIPART_CONTENT_TYPE;
				string contentRange;
				string partialContentType = "Content-Type: " + ContentType + "\r\n";
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
					SendFile(offset, length, fileLength, context);
					response.Write("\r\n");
				}
				response.Write(MULTIPART_RANGE_END);
			}

			// if we make it here, we're sending a "206 Partial Content" status
			response.StatusCode = (int)HttpStatusCode.PartialContent;
			response.AppendHeader("Accept-Ranges", "bytes");
			response.Cache.SetLastModified(lastModified);
			response.Cache.SetETag(etag);
			response.Cache.SetCacheability(HttpCacheability.Public);

			handled = true;
			return handled;
		}

		private void SendFile(long offset, long length, long fileLength, HttpContextBase context)
		{
			var response = context.Response;
			bool bufferOutput = response.BufferOutput;

			try
			{
				response.BufferOutput = true;

				if (_streamReader != null)
				{
					var stream = _streamReader();
					if (stream == null)
					{
						throw new NullReferenceException("File stream cannot be NULL.");
					}

					using (stream)
					{
						int bufferSize = (int)Math.Min(DefaultWriteBufferSize, length);
						byte[] buffer = new byte[bufferSize];

						int read;
						long remaining = length;

						stream.Seek(offset, SeekOrigin.Begin);
						while ((remaining > 0) && (read = stream.Read(buffer, 0, buffer.Length)) != 0)
						{
							response.OutputStream.Write(buffer, 0, read);
							//response.Flush();

							remaining -= read;
						}
					}
				}
				else if (_bufferReader != null)
				{
					var buffer = _bufferReader();
					if (buffer == null)
					{
						throw new NullReferenceException("File buffer cannot be NULL.");
					}

					response.OutputStream.Write(buffer, (int)offset, (int)length);
				}
			}
			finally
			{
				response.BufferOutput = bufferOutput;
			}
		}
	}
}
