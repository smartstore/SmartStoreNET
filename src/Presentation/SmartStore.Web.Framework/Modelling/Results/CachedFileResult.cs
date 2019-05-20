using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Modelling
{
	public class CachedFileResult : FileResult
	{
		// Default buffer size as defined in BufferedStream type
		const int DefaultWriteBufferSize = 81920;

		private static char[] _commaSplitArray = new char[] { ',' };
		private static char[] _dashSplitArray = new char[] { '-' };
		private static string[] _httpDateFormats = new string[] { "r", "dddd, dd-MMM-yy HH':'mm':'ss 'GMT'", "ddd MMM d HH':'mm':'ss yyyy" };

		private HttpContextBase _httpContext;
		private string _etag;
		private DateTime? _lastModifiedDateUtc;

		private readonly string _path;
		private readonly Func<Stream> _streamReader;
		private readonly Func<byte[]> _bufferReader;

		public CachedFileResult(string path, string contentType = null)
			: base(contentType ?? MimeTypes.MapNameToMimeType(path))
		{
			_etag = GenerateETag(path);
			_path = path;
		}

		public CachedFileResult(FileInfo file, string contentType = null, Func<Stream> reader = null)
			: this(GenerateETag(file), contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
			LastModifiedUtc = file.LastWriteTimeUtc;
		}

		public CachedFileResult(IFile file, string contentType = null, Func<Stream> reader = null)
			: this(GenerateETag(file), contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
			LastModifiedUtc = file.LastUpdated;
		}

		public CachedFileResult(string etag, IFile file, string contentType = null, Func<Stream> reader = null)
			: this(etag, contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
			LastModifiedUtc = file.LastUpdated;
		}

		public CachedFileResult(VirtualFile file, string contentType = null, Func<Stream> reader = null)
			: this(GenerateETag(file), contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.Open)
		{
		}

		public CachedFileResult(string etag, string contentType, Func<Stream> reader)
			: base(contentType)
		{
			Guard.NotNull(reader, nameof(reader));
			SanitizeETag(etag);

			_streamReader = reader;
		}

		public CachedFileResult(string etag, string contentType, Func<byte[]> reader)
			: base(contentType)
		{
			Guard.NotNull(reader, nameof(reader));
			SanitizeETag(etag);

			_bufferReader = reader;
		}

		private void SanitizeETag(string etag)
		{
			Guard.NotEmpty(etag, nameof(etag));

			if (etag[0] != '"')
			{
				etag = string.Concat("\"", etag, "\"");
			}

			_etag = etag;
		}

		public DateTime? LastModifiedUtc
		{
			get
			{
				return _lastModifiedDateUtc;
			}
			set
			{
				if (value == null)
				{

				}
				_lastModifiedDateUtc = value == null
					? (DateTime?)null
					: FixLastModifiedDate(value.Value);
			}
		}

		public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(7);

		public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(7);

		#region ETag generators

		public static string GenerateETag(string path)
		{
			Guard.NotEmpty(path, nameof(path));

			if (!PathHelper.IsAbsolutePhysicalPath(path))
			{
				path = CommonHelper.MapPath(path);
			}

			return GenerateETag(new FileInfo(path));
		}

		public static string GenerateETag(VirtualFile file)
		{
			Guard.NotNull(file, nameof(file));

			var fi = new FileInfo(CommonHelper.MapPath(file.VirtualPath));
			return GenerateETag(fi);
		}

		public static string GenerateETag(FileInfo file)
		{
			Guard.NotNull(file, nameof(file));

			if (!file.Exists)
			{
				throw new FileNotFoundException("File to create ETag for must exist.", file.FullName);
			}

			return GenerateETag(file.FullName, file.Length.GetHashCode(), file.LastWriteTimeUtc);
		}

		public static string GenerateETag(IFile file)
		{
			Guard.NotNull(file, nameof(file));

			if (!file.Exists)
			{
				throw new FileNotFoundException("File to create ETag for must exist.", file.Path);
			}

			return GenerateETag(file.Path, file.Size.GetHashCode(), FixLastModifiedDate(file.LastUpdated));
		}

		public static string GenerateETag(params object[] tokens)
		{
			var sb = new StringBuilder();
			var tag = string.Empty;

			foreach (var token in tokens)
			{
				if (token is DateTime dt)
				{
					tag += FixLastModifiedDate(dt).ToUnixTime().ToString();
				}
				else
				{
					tag += token.Convert<string>();
				}
			}

			return "\"" + tag.Hash(Encoding.UTF8) + "\"";
		}

		private static DateTime FixLastModifiedDate(DateTime input)
		{
			var date = input.ToUniversalTime();
			var result = new DateTime(
				date.Year,
				date.Month,
				date.Day,
				date.Hour,
				date.Minute,
				date.Second,
				0,
				DateTimeKind.Utc);

			// Because we can't set a "Last-Modified" header to any time
			// in the future, check the last modified time and set it to
			// DateTime.Now if it's in the future. 
			// This is to fix VSWhidbey #402323
			DateTime utcNow = DateTime.UtcNow;
			if (result > utcNow)
			{
				// use 1 second resolution
				result = new DateTime(utcNow.Ticks - (utcNow.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
			}

			return result;
		}

		#endregion

		public override void ExecuteResult(ControllerContext context)
		{
			_httpContext = context.HttpContext;
			base.ExecuteResult(context);
		}

		protected override void WriteFile(HttpResponseBase response)
		{
			var ifNoneMatch = _httpContext.Request.Headers["If-None-Match"];
			if (ifNoneMatch.HasValue() && _etag == ifNoneMatch)
			{
				// File hasn't changed, so return HTTP 304 without retrieving the data
				response.StatusCode = (int)HttpStatusCode.NotModified;
				response.StatusDescription = "Not Modified";

				// Explicitly set the Content-Length header so the client doesn't wait for
				// content but keeps the connection open for other requests
				response.AddHeader("Content-Length", "0");

				ApplyResponseHeaders(response, false);
			}
			else
			{
				if (_path != null)
				{
					response.TransmitFile(_path);
				}
				else if (_streamReader != null)
				{
					var stream = _streamReader();
					if (stream == null)
					{
						throw new NullReferenceException("File stream cannot be NULL.");
					}

					var rangeInfo = GetRanges(_httpContext.Request, stream.Length);

					WriteFileStream(response, stream, 0, stream.Length);
				}
				else if (_bufferReader != null)
				{
					var buffer = _bufferReader();
					if (buffer == null)
					{
						throw new NullReferenceException("File buffer cannot be NULL.");
					}

					var rangeInfo = GetRanges(_httpContext.Request, buffer.Length);

					// Write buffer to output stream
					WriteFileContent(response, buffer, 0, buffer.Length);
				}

				ApplyResponseHeaders(response, true);

				// Set ETag for served file (revalidated on subsequent requests)
				response.Cache.SetETag(_etag);
			}
		}

		private void ApplyResponseHeaders(HttpResponseBase response, bool setLastModifiedDate)
		{
			var cache = response.Cache;

			// We support byte ranges
			response.AppendHeader("Accept-Ranges", "bytes");

			cache.SetCacheability(System.Web.HttpCacheability.Public);
			cache.VaryByHeaders["Accept-Encoding"] = true;
			cache.SetExpires(Expiration);
			cache.SetMaxAge(MaxAge);
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

			if (setLastModifiedDate && LastModifiedUtc.HasValue)
			{
				cache.SetLastModified(LastModifiedUtc.Value);
			}
		}

		private void WriteFileContent(HttpResponseBase response, byte[] buffer, int rangeStart, int rangeEnd)
		{
			bool bufferOutput = response.BufferOutput;

			response.BufferOutput = false;
			response.OutputStream.Write(buffer, rangeStart, rangeEnd);

			response.BufferOutput = bufferOutput;
		}

		private void WriteFileStream(HttpResponseBase response, Stream stream, long rangeStart, long rangeEnd)
		{
			using (stream)
			{
				int bufferSize = (int)Math.Min(DefaultWriteBufferSize, rangeEnd);
				byte[] buffer = new byte[bufferSize];

				int read;
				long remaining = rangeEnd;

				stream.Seek(rangeStart, SeekOrigin.Begin);
				while ((remaining > 0) && (read = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					response.OutputStream.Write(buffer, 0, read);
					response.Flush();

					remaining -= read;
				}
			}
		}

		#region Ranges

		private string GetHeader(HttpRequestBase request, string header, string defaultValue = "")
		{
			return string.IsNullOrEmpty(request.Headers[header]) ? defaultValue : request.Headers[header].Replace("\"", String.Empty);
		}

		private RangeRequestInfo GetRanges(HttpRequestBase request, long fileLength)
		{
			var rangeInfo = new RangeRequestInfo() { FileLength = fileLength };

			string rangesHeader = GetHeader(request, "Range");
			string ifRangeHeader = GetHeader(request, "If-Range", _etag);

			bool isIfRangeHeaderDate = DateTime.TryParseExact(
				ifRangeHeader, 
				_httpDateFormats, 
				null, 
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, 
				out var ifRangeHeaderDate);

			if (string.IsNullOrEmpty(rangesHeader) || (!isIfRangeHeaderDate && ifRangeHeader != _etag) || (isIfRangeHeaderDate && LastModifiedUtc.HasValue && LastModifiedUtc.Value > ifRangeHeaderDate))
			{
				rangeInfo.RangesStartIndexes = new long[] { 0 };
				rangeInfo.RangesEndIndexes = new long[] { fileLength - 1 };
				rangeInfo.IsRangeRequest = false;
				rangeInfo.IsMultipartRequest = false;
			}
			else
			{
				string[] ranges = rangesHeader.Replace("bytes=", String.Empty).Split(_commaSplitArray);

				rangeInfo.RangesStartIndexes = new long[ranges.Length];
				rangeInfo.RangesEndIndexes = new long[ranges.Length];
				rangeInfo.IsRangeRequest = true;
				rangeInfo.IsMultipartRequest = (ranges.Length > 1);

				for (int i = 0; i < ranges.Length; i++)
				{
					string[] currentRange = ranges[i].Split(_dashSplitArray);

					if (string.IsNullOrEmpty(currentRange[1]))
						rangeInfo.RangesEndIndexes[i] = fileLength - 1;
					else
						rangeInfo.RangesEndIndexes[i] = Int64.Parse(currentRange[1]);

					if (String.IsNullOrEmpty(currentRange[0]))
					{
						rangeInfo.RangesStartIndexes[i] = fileLength - rangeInfo.RangesEndIndexes[i];
						rangeInfo.RangesEndIndexes[i] = fileLength - 1;
					}
					else
						rangeInfo.RangesStartIndexes[i] = Int64.Parse(currentRange[0]);
				}
			}

			return rangeInfo;
		}

		private long GetContentLength(RangeRequestInfo range, string boundary)
		{
			long contentLength = 0;

			for (int i = 0; i < range.RangesStartIndexes.Length; i++)
			{
				contentLength += (range.RangesEndIndexes[i] - range.RangesStartIndexes[i]) + 1;

				if (range.IsMultipartRequest)
				{
					contentLength += boundary.Length 
						+ ContentType.Length 
						+ range.RangesStartIndexes[i].ToString("D").Length 
						+ range.RangesEndIndexes[i].ToString("D").Length 
						+ range.FileLength.ToString("D").Length 
						+ 49;
				}	
			}

			if (range.IsMultipartRequest)
			{
				contentLength += boundary.Length + 4;
			}		

			return contentLength;
		}

		private bool ValidateRanges(RangeRequestInfo range, HttpResponseBase response)
		{
			for (int i = 0; i < range.RangesStartIndexes.Length; i++)
			{
				if (range.RangesStartIndexes[i] > range.FileLength - 1 
					|| range.RangesEndIndexes[i] > range.FileLength - 1 
					|| range.RangesStartIndexes[i] < 0 
					|| range.RangesEndIndexes[i] < 0 
					|| range.RangesEndIndexes[i] < range.RangesStartIndexes[i])
				{
					response.StatusCode = 400;
					return false;
				}
			}

			return true;
		}

		class RangeRequestInfo
		{
			public long[] RangesStartIndexes { get; set; }
			public long[] RangesEndIndexes { get; set; }
			public bool IsRangeRequest { get; set; }
			public bool IsMultipartRequest { get; set; }
			public long FileLength { get; set; }
		}

		#endregion
	}
}
